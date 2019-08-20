
#define FUSE_USE_VERSION 30

#ifdef HAVE_CONFIG_H
#include <config.h>
#endif

#ifdef linux
/* For pread()/pwrite()/utimensat() */
#define _XOPEN_SOURCE 700
#endif

#include <git2.h>

#include <dokan/dokan.h>
#include <dokan/fileinfo.h>
#include <malloc.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <winbase.h>

#include <fcntl.h>
#include <sys/stat.h>
#include <errno.h>

#include "getopt.h"
#include "raft_operations.h"

#define LOG_NAME "main.c"
#include "raft_log.h"

//#define WIN10_ENABLE_LONG_PATH
#ifdef WIN10_ENABLE_LONG_PATH
//dirty but should be enough
#define DOKAN_MAX_PATH 32768
#else
#define DOKAN_MAX_PATH MAX_PATH
#endif // DEBUG


#ifndef S_ISDIR
#define	S_ISDIR(m)	(((m)&S_IFDIR) == S_IFDIR)
#endif

#define PROGRAM_NAME "RAFT - Repo As Filesystem Translator"

#define IS_EC(ret) (ret < 0)

#define DEFINE_BUFFER(type) \
	struct buffer_##type { \
		size_t length; \
		type * buffer;  \
    }; \
	typedef struct buffer_##type buffer_##type##_s


#define DEFINE_BUFFER_EMBEDDED(type) \
	struct buffer_em_##type { \
		size_t length; \
		type   buffer[1]; \
	}; \
	typedef struct buffer_em_##type buffer_em_##type##_s

DEFINE_BUFFER(char);
DEFINE_BUFFER_EMBEDDED(char);

DEFINE_BUFFER(wchar_t);

struct buffer {
	size_t length;
	
};

struct raft_context {
	DOKAN_OPERATIONS dokan_operations;
	DOKAN_OPTIONS dokan_options;
	buffer_wchar_t_s repository_path;
	buffer_wchar_t_s mount_point;
};

typedef struct raft_context raft_context_s;

void raft_set_dokan_options(PDOKAN_OPTIONS dokan_opt, PWCHAR mount_point)
{
	dokan_opt->Version = DOKAN_VERSION;
	dokan_opt->ThreadCount = 0; // fine tune later
	dokan_opt->MountPoint = mount_point;

	dokan_opt->Options |= DOKAN_OPTION_DEBUG;
	dokan_opt->Options |= DOKAN_OPTION_STDERR;

	dokan_opt->Options |= DOKAN_OPTION_MOUNT_MANAGER;

	dokan_opt->Timeout = 1000 * 60 * 5;

}

int main(int argc, char* argv[])
{
	raft_log_init();

	raft_context_s* pCtx = NULL;
	int ec = 0;

	LOG_INFO("Starting:      " PROGRAM_NAME );
	int retVal = git_libgit2_init();
	if (IS_EC(retVal))
	{
		LOG_ERROR("Failed libgit2_init, ec=0x%08X", retVal);
		return -1;
	}

	int opt;

	char* repoPath = NULL;
	char* mountPoint = NULL;
	while ((opt = getopt(argc, argv, ":r:m:")) != -1)
	{
		switch (opt)
		{
		case 'r':
			repoPath = optarg;
			break;
		case 'm':
			mountPoint = optarg;
			break;
		case ':':
			LOG_ERROR("option missing value");
			break;
		}
	}

	for (; optind < argc; optind++) {
		LOG_INFO("Unparsed arg: %s", argv[optind]);
	}

	if (mountPoint == NULL || repoPath == NULL)
	{
		LOG_ERROR("Missing all necessary args.");
		ec = -1;
		goto exit;
	}

	pCtx = (raft_context_s*)malloc(sizeof(raft_context_s));
	if (NULL == pCtx)
	{
		LOG_ERROR("Failed allocation of memory for ctx");
		ec = -1;
		goto exit;
	}

	memset(pCtx, 0, sizeof(raft_context_s));

	struct stat repoStat;

	// assume last arg as path to repo, and skip it for fuse
	if (stat(repoPath, &repoStat) < 0)
	{
		LOG_ERROR("Directory %s doesn't exist", argv[argc - 1]);
		ec = -1;
		goto exit;
	}
	else if (!S_ISDIR(repoStat.st_mode))
	{
		LOG_ERROR("%s is not a directory", argv[argc - 1]);
		ec = -1;
		goto exit;
	}

	size_t repoPathLen = strlen(repoPath) + 1;

	pCtx->repository_path.length = repoPathLen * 2;
	pCtx->repository_path.buffer = (wchar_t*)malloc(repoPathLen * 2);

	size_t converted = 0;
	mbstowcs_s(&converted, pCtx->repository_path.buffer, repoPathLen,
		repoPath, repoPathLen - 1);
	
	size_t mountPointLen = strlen(mountPoint) + 1;

	pCtx->mount_point.length = mountPointLen * 2;
	pCtx->mount_point.buffer = (wchar_t*)malloc(mountPointLen*2);

	mbstowcs_s(&converted, pCtx->mount_point.buffer, mountPointLen,
		mountPoint, mountPointLen - 1);

	raft_set_dokan_options(&pCtx->dokan_options, pCtx->mount_point.buffer);
	raft_set_dokan_operations(&pCtx->dokan_operations);
exit:
	LOG_INFO("Shutting down: " PROGRAM_NAME);
	git_libgit2_shutdown();
	
	if (pCtx) {
		if (pCtx->mount_point.buffer) {
			free(pCtx->mount_point.buffer);
		}

		if (pCtx->repository_path.buffer) {
			free(pCtx->repository_path.buffer);
		}
		free(pCtx);
	}

	raft_log_exit();
	return ec;
}
