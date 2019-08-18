
#define FUSE_USE_VERSION 30

#ifdef HAVE_CONFIG_H
#include <config.h>
#endif

#ifdef linux
/* For pread()/pwrite()/utimensat() */
#define _XOPEN_SOURCE 700
#endif

#include <git2.h>

#include <fuse.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
#ifdef linux
#include <unistd.h>
#endif
#include <fcntl.h>
#include <sys/stat.h>
#ifdef linux
#include <dirent.h>
#endif
#include <errno.h>
#ifdef linux
#include <sys/time.h>
#endif
#ifdef HAVE_SETXATTR
#include <sys/xattr.h>
#endif

#ifndef S_ISDIR
#define	S_ISDIR(m)	(((m)&S_IFDIR) == S_IFDIR)
#endif


#define PROGRAM_NAME "RAFT - Repo As Filesystem Translator"

#define IS_EC(ret) (ret < 0)

/* Logging macros, for easy switch to file logging */

#define EOL "\n"

#define LOG_INFO(message, ...) printf(message EOL, ##__VA_ARGS__)
#define LOG_ERROR(message, ...) fprintf(stderr, message EOL, ##__VA_ARGS__)

static int raft_getattr(const char* path, struct FUSE_STAT * stbuf)
{
	return -1;
}

static int raft_access(const char* path, int mask)
{
	return -1;
}

static int raft_readlink(const char* path, char* buf, size_t size)
{
	return -1;
}

static int raft_readdir(const char* path, void* buf, fuse_fill_dir_t filler,
	FUSE_OFF_T offset, struct fuse_file_info *fi)
{
	return -1;
}

static int raft_mknod(const char* path, mode_t mode, dev_t rdev)
{
	return -1;
}

static int raft_mkdir(const char* path, mode_t mode)
{
	return -1;
}

static int raft_unlink(const char* path)
{
	return -1;
}

static int raft_rmdir(const char* path)
{
	return -1;
}

static int raft_symlink(const char* from, const char* to)
{
	return -1;
}

static int raft_rename(const char* from, const char* to)
{
	return -1;
}

static int raft_link(const char* from, const char* to)
{
	return -1;
}

static int raft_chmod(const char* path, mode_t mode)
{
	return -1;
}

static int raft_chown(const char* path, uid_t uid, gid_t gid)
{
	return -1;
}

static int raft_truncate(const char* path, FUSE_OFF_T size)
{
	return -1;
}

#ifdef HAVE_UTIMENSAT
static int raft_utimens(const char* path, const struct timespec ts[2])
{
	return -1;
}
#endif

static int raft_open(const char* path, struct fuse_file_info* fi)
{
	return -1;
}

static int raft_read(const char* path, char* buf, size_t size, FUSE_OFF_T offset,
	struct fuse_file_info* fi)
{
	return -1;
}

static int raft_write(const char* path, const char* buf, size_t size,
	FUSE_OFF_T offset, struct fuse_file_info* fi)
{
	return -1;
}

static int raft_statfs(const char* path, struct statvfs* stbuf)
{
	return -1;
}

static int raft_release(const char* path, struct fuse_file_info* fi)
{
	return -1;
}

static int raft_fsync(const char* path, int isdatasync,
	struct fuse_file_info* fi)
{
	return -1;
}

#ifdef HAVE_POSIX_FALLOCATE
static int raft_fallocate(const char* path, int mode,
	FUSE_OFF_T offset, FUSE_OFF_T length, struct fuse_file_info* fi)
{
	return -1;
}
#endif

#ifdef HAVE_SETXATTR
static int raft_setxattr(const char* path, const char* name, const char* value,
	size_t size, int flags)
{
	return -1;
}

static int raft_getxattr(const char* path, const char* name, char* value,
	size_t size)
{
	return -1;
}

static int raft_listxattr(const char* path, char* list, size_t size)
{
	return -1;
}

static int raft_removexattr(const char* path, const char* name)
{
	return -1;
}
#endif


static struct fuse_operations raft_operations = {
	.getattr = raft_getattr,
	.readlink = raft_readlink,
	.getdir = NULL,
	.mknod = raft_mknod,
	.mkdir = raft_mkdir,
	.unlink = raft_unlink,
	.rmdir = raft_rmdir,
	.symlink = raft_symlink,
	.rename = raft_rename,
	.link = raft_link,
	.chmod = raft_chmod,
	.chown = raft_chown,
	.truncate = raft_truncate,
	.utime = NULL,

	.open = raft_open,
	.read = raft_read,
	.write = raft_write,
	.statfs = raft_statfs,
	.flush = NULL,
	.release = raft_release,
	.fsync = raft_fsync,
#ifdef HAVE_SETXATTR
	.setxattr = raft_setxattr,
	.getxattr = raft_getxattr,
	.listxattr = raft_listxattr,
	.removexattr = raft_removexattr,
#endif
	.opendir = NULL,
	.readdir = raft_readdir,
	.releasedir = NULL,
	.fsyncdir = NULL,
	.init = NULL,
	.destroy = NULL,
	.access = raft_access,
	.create = NULL, // if not implemneted mknod -> open
	.ftruncate = NULL, // if not, truncate is called
	.fgetattr = NULL,
	.lock = NULL,
#ifdef HAVE_UTIMENSAT
	.utimens = raft_utimens,
#endif
	.bmap = NULL,
#ifdef _WIN32
	.win_get_attributes = NULL,
	.win_set_attributes = NULL,
	.win_set_times = NULL,
#endif
	
	
#ifdef HAVE_POSIX_FALLOCATE
	.fallocate = raft_fallocate,
#endif

};

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

struct buffer {
	size_t length;
	
};

struct raft_context {
	buffer_char_s repository_path;
};

typedef struct raft_context raft_context_s;

int main(int argc, char* argv[])
{
	raft_context_s* pCtx = NULL;
	int ec = 0;

	LOG_INFO("Starting:      " PROGRAM_NAME );
	int retVal = git_libgit2_init();
	if (IS_EC(retVal))
	{
		LOG_ERROR("Failed libgit2_init, ec=0x%08X", retVal);
		return -1;
	}

	if (argc < 2) {
		LOG_ERROR("Not enough args");
		ec = -1;
		goto exit;
	}

	struct stat repoDirectory;
	if (stat(argv[1], &repoDirectory) < 0)
	{
		LOG_ERROR("Directory %s doesn't exist", argv[1]);
		ec = -1;
		goto exit;
	}
	else if (!S_ISDIR(repoDirectory.st_mode))
	{
		LOG_ERROR("%s is not a directory", argv[1]);
		ec = -1;
		goto exit;
	}

	size_t dirname_len = strlen(argv[1]);

	pCtx = (raft_context_s *)malloc(sizeof(raft_context_s) + dirname_len);
	if (NULL == pCtx)
	{
		LOG_ERROR("Failed allocation of memory for ctx");
		ec = -1;
		goto exit;
	}

	pCtx->repository_path.length = dirname_len;
	pCtx->repository_path.buffer = (char*)&pCtx[1];

#ifdef linux
	umask(0);
#endif
	fuse_main(argc, argv, &raft_operations, &pCtx);

exit:
	LOG_INFO("Shutting down: " PROGRAM_NAME);
	git_libgit2_shutdown();
	
	if (pCtx) {
		free(pCtx);
	}

	return ec;
}
