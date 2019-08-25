#ifndef RAFT_OPERATIONS_H
#define RAFT_OPERATIONS_H

#include <dokan/dokan.h>
#include <windows.h>
#include <stdlib.h>

//#define WIN10_ENABLE_LONG_PATH
#ifdef WIN10_ENABLE_LONG_PATH
//dirty but should be enough
#define DOKAN_MAX_PATH 32768
#else
#define DOKAN_MAX_PATH MAX_PATH
#endif // DEBUG

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

#include <git2.h>

struct raft_context {
	DOKAN_OPERATIONS dokan_operations;
	DOKAN_OPTIONS dokan_options;
	buffer_char_s repository_path;
	buffer_wchar_t_s mount_point;
	git_repository* repository;
	git_strarray* repository_remotes;
	git_strarray repository_remotes_internal;
	git_strarray* tags;
	git_strarray tags_internal;
};

struct bitmap_path {
	int bitmap[DOKAN_MAX_PATH / (sizeof(int) * 8) + ((DOKAN_MAX_PATH % (sizeof(int) * 8) > 0) ? 1 : 0)];
};

typedef struct bitmap_path bitmap_path_s;


typedef struct raft_context raft_context_s;

void raft_set_dokan_operations(PDOKAN_OPERATIONS);

#endif /* RAFT_OPERATIONS_H */