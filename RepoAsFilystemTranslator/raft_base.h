#ifndef RAFT_BASE_H
#define RAFT_BASE_H

#include <stdint.h>
#include <stdlib.h>
#include <string.h>

#include <git2.h>

#include <dokan/dokan.h>
#include <windows.h>

//#define WIN10_ENABLE_LONG_PATH
#ifdef WIN10_ENABLE_LONG_PATH
//dirty but should be enough
#define DOKAN_MAX_PATH 32768
#else
#define DOKAN_MAX_PATH MAX_PATH
#endif // DEBUG

#define DEBUG 1

extern time_t start_time; /* from raft_log */

enum raft_obj_type {
	RAFT_OBJ_TYPE_UNDEFINED = 0,
	RAFT_OBJ_TYPE_BRANCH = 1,
	RAFT_OBJ_TYPE_REMOTE = 2,
	RAFT_OBJ_TYPE_TAG = 3,

	RAFT_OBJ_TYPE_ENTRY_MALLOC = 1004,
	RAFT_OBJ_TYPE_ENTRY_STATIC = 1005,
};

typedef enum raft_obj_type raft_obj_type_e;

struct raft_obj_header {
	raft_obj_type_e type;
};

typedef struct raft_obj_header raft_obj_header_s;

struct raft_debug_header {
	time_t creation_time;
	size_t obj_size; /* excl this header */
};

typedef struct raft_debug_header raft_debug_header_s;

// for debug purposes we will define an malloc/free internal call
inline void* raft_malloc(size_t size)
{
	void* ptr = malloc(size
#if DEBUG
		+ sizeof(raft_debug_header_s)
#endif
	);

	if (ptr) {
		memset(ptr, 0, size
#if DEBUG
			+ sizeof(raft_debug_header_s)
#endif
		);

#if DEBUG
		raft_debug_header_s* debug_header = (raft_debug_header_s*)ptr;
		debug_header->creation_time = time(NULL) - start_time;
		debug_header->obj_size = size;

		ptr = (void*)& debug_header[1];
#endif
	}

	return ptr;
}

// for debug purposes we will define an malloc/free internal call
inline void* raft_malloc_obj(size_t size, raft_obj_type_e type)
{
	void* ptr = malloc(size
#if DEBUG
		+ sizeof(raft_debug_header_s)
#endif
	);

	if (ptr) {
		memset(ptr, 0, size
#if DEBUG
			+ sizeof(raft_debug_header_s)
#endif
		);

#if DEBUG
		raft_debug_header_s* debug_header = (raft_debug_header_s*)ptr;
		debug_header->creation_time = time(NULL) - start_time;
		debug_header->obj_size = size;

		ptr = (void*)& debug_header[1];

#endif
		((raft_obj_header_s*)ptr)->type = type;
	}

	return ptr;
}

inline void raft_free(void* ptr)
{
#if DEBUG
	ptr = &((raft_debug_header_s*)ptr)[-1];
#endif
	free(ptr);
}


#endif /* RAFT_BASE_H */