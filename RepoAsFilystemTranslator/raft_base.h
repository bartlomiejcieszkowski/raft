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

struct raft_memory_statistics {
#if DEBUG
	size_t peak_memory_usage;
	size_t current_memory_usage;
#endif
	size_t peak_allocations;
	size_t current_allocations;
};

typedef struct raft_memory_statistics raft_memory_statistics_s;

extern raft_memory_statistics_s g_raft_memory_statistics;

// for debug purposes we will define an malloc/free internal call
void* raft_malloc(size_t size);

// for debug purposes we will define an malloc/free internal call
void* raft_malloc_obj(size_t size, raft_obj_type_e type);

void raft_free(void* ptr);


#endif /* RAFT_BASE_H */