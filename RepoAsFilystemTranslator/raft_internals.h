#ifndef RAFT_INTERNALS_H
#define RAFT_INTERNALS_H

#include <stdint.h>
#include <stdlib.h>
#include <string.h>

#include <git2.h>

#define DEBUG 1

enum raft_obj_type {
	RAFT_OBJ_TYPE_UNDEFINED = 0,
	RAFT_OBJ_TYPE_BRANCH = 1,
	RAFT_OBJ_TYPE_REMOTE = 2,
	RAFT_OBJ_TYPE_TAG = 3,
};

typedef enum raft_obj_type raft_obj_type_e;

struct raft_debug_header {
	time_t creation_time;
	size_t obj_size; /* excl this header */
};
typedef struct raft_debug_header raft_debug_header_s;

struct raft_obj_header {
	raft_obj_type_e type;
	struct raft_obj_header* child_obj;

};

extern time_t start_time; /* from raft_log */

// for debug purposes we will define an malloc/free internal call
inline void* raft_malloc(size_t size)
{
	void* ptr = malloc(size
#if DEBUG
		+ sizeof(raft_debug_header_s)
#endif
	);

#if DEBUG
	if (ptr) {
		raft_debug_header_s* debug_header = (raft_debug_header_s*)ptr;
		debug_header->creation_time = time(NULL) - start_time;
		debug_header->obj_size = size;

		ptr = (void*)& debug_header[1];
	}
#endif
}

inline void raft_free(void* ptr)
{
	free(ptr);
}

typedef struct raft_obj_header raft_obj_header_s;

int is_raft_obj(void* obj, raft_obj_type_e type)
{
	return ((raft_obj_header_s*)obj)->type == type;
}

/*
Soo..

We will have os asking for list of files,
we could hold in the memory local snapshot of a tree for a commit @ branch

dict<oid,filelist>

algo:
  . get branch oid
  . check if it is the one we have saved
  . if yes use snapshot
  . if not:
	. get filelist
	. for each file store reference - this is for faster lookup when opening files and doing things on them

open file:
  . get the file blob from oid
  . modify in memory
  . if contents changed
	. create commit, add this file, commit to this branch



*/




#endif /* RAFT_INTERNALS_H */
