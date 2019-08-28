
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
#include "raft_internals.h"
#include "raft_operations.h"

#define LOG_NAME "main.c"
#include "raft_log.h"




#ifndef S_ISDIR
#define	S_ISDIR(m)	(((m)&S_IFDIR) == S_IFDIR)
#endif

#define PROGRAM_NAME "RAFT - Repo As Filesystem Translator"

#define IS_EC(ret) (ret < 0)

void raft_set_dokan_options(PDOKAN_OPTIONS dokan_opt, PWCHAR mount_point, ULONG64 context)
{
	dokan_opt->Version = DOKAN_VERSION;
	dokan_opt->ThreadCount = 0; // fine tune later
	dokan_opt->MountPoint = mount_point;

	dokan_opt->Options |= DOKAN_OPTION_DEBUG;
	dokan_opt->Options |= DOKAN_OPTION_STDERR;

	// recycle_bin and whatnot, we dont want that
	//dokan_opt->Options |= DOKAN_OPTION_MOUNT_MANAGER;
	//dokan_opt->Options |= DOKAN_OPTION_REMOVABLE;

	dokan_opt->Timeout = 1000 * 60 * 5;

	dokan_opt->GlobalContext = context;
}

void print_help(char* name)
{
	printf("\nUsage:\n");
	printf("%s -r <path_to_repository> -m Mountpoint\n", name);
	printf("example:\n");
	printf("%s -r C:\\my_repo -m M:\\\n", name);
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
		case '?':
			LOG_ERROR("Invalid arg");
			print_help(argv[0]);
			return -1;
		}
	}

	for (; optind < argc; optind++) {
		LOG_INFO("Unparsed arg: %s", argv[optind]);
	}

	if (mountPoint == NULL || repoPath == NULL)
	{
		LOG_ERROR("Missing all necessary args.");
		ec = -1;
		print_help(argv[0]);
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

	//Check If Repository https://libgit2.org/docs/guides/101-samples/
	//	/* Pass NULL for the output parameter to check for but not open the repo */
	//	if (git_repository_open_ext(
	//		NULL, "/tmp/…", GIT_REPOSITORY_OPEN_NO_SEARCH, NULL) == 0) {
	//		/* directory looks like an openable repository */;
	//	}
	//

	pCtx->repository_path.length = repoPathLen;
	pCtx->repository_path.buffer = (char*)malloc(repoPathLen);

	memcpy(pCtx->repository_path.buffer, repoPath, repoPathLen);
	
	size_t mountPointLen = strlen(mountPoint) + 1;

	pCtx->mount_point.length = mountPointLen * 2;
	pCtx->mount_point.buffer = (wchar_t*)malloc(mountPointLen*2);

	size_t converted = 0;
	mbstowcs_s(&converted, pCtx->mount_point.buffer, mountPointLen,
		mountPoint, mountPointLen - 1);

	DOKAN_OPERATIONS* dokan_operations = raft_malloc_obj(sizeof(DOKAN_OPERATIONS), RAFT_OBJ_TYPE_UNDEFINED);
	DOKAN_OPTIONS* dokan_options = raft_malloc_obj(sizeof(DOKAN_OPTIONS), RAFT_OBJ_TYPE_UNDEFINED);

	raft_set_dokan_options(dokan_options, pCtx->mount_point.buffer, (ULONG64)pCtx);
	raft_set_dokan_operations(dokan_operations);

	int status = DokanMain(dokan_options, dokan_operations);
	switch (status) {
	case DOKAN_SUCCESS:
		fprintf(stderr, "Success\n");
		break;
	case DOKAN_ERROR:
		fprintf(stderr, "Error\n");
		break;
	case DOKAN_DRIVE_LETTER_ERROR:
		fprintf(stderr, "Bad Drive letter\n");
		break;
	case DOKAN_DRIVER_INSTALL_ERROR:
		fprintf(stderr, "Can't install driver\n");
		break;
	case DOKAN_START_ERROR:
		fprintf(stderr, "Driver something wrong\n");
		break;
	case DOKAN_MOUNT_ERROR:
		fprintf(stderr, "Can't assign a drive letter\n");
		break;
	case DOKAN_MOUNT_POINT_ERROR:
		fprintf(stderr, "Mount point error\n");
		break;
	case DOKAN_VERSION_ERROR:
		fprintf(stderr, "Version error\n");
		break;
	default:
		fprintf(stderr, "Unknown error: %d\n", status);
		break;
	}

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

		if (pCtx->repository_remotes)
		{
			pCtx->repository_remotes = NULL;
			git_strarray_free(&pCtx->repository_remotes_internal);
		}

		if (pCtx->tags)
		{
			pCtx->tags = NULL;
			git_strarray_free(&pCtx->tags_internal);
		}
		free(pCtx);
	}

	raft_log_exit();
	return ec;
}
