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


struct raft_obj_header {
	raft_obj_type_e type;
};

typedef struct raft_obj_header raft_obj_header_s;

int is_raft_obj(void* obj, raft_obj_type_e type)
{
	return ((raft_obj_header_s*)obj)->type == type;
}

struct raft_obj_ll {
	raft_obj_header_s header;
	raft_obj_header_s* next;
};

typedef struct raft_obj_ll raft_obj_ll_s;

struct raft_obj_ll_parent {
	raft_obj_header_s header;
	raft_obj_header_s* next;
	raft_obj_header_s* child;
};

typedef struct raft_obj_ll_parent raft_obj_ll_parent_s;


// define so we can not define it if not needed
#define RAFT_OBJ_HEADER(name) \
    raft_obj_header_s name

#define RAFT_OBJ_ENTRY_HEADER(name) \
    raft_obj_ll_s name

#define RAFT_OBJ_ENTRY_PARENT_HEADER(name) \
    raft_obj_ll_parent_s name

#define RAFT_OBJ_HEADER_UNION(name, subtype) \
    union { \
        raft_obj_header_s name; \
        subtype name## _sub; \
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

#define RAFT_REFCOUNT_TYPE int32_t

typedef void (*free_fn)(void*);

#define DECLARE_FREE_FN(name) \
    void name(void*)



// git_oid is 20 bytes, 5dwords.. yeah, we can afford it
struct raft_oid_entry
{
	RAFT_OBJ_HEADER_UNION(header, raft_obj_ll_parent_s);
	RAFT_REFCOUNT_TYPE refcount;
	git_oid key; /* assume oid 0 is invalid or null is invalid.. yeah */
	void* value; /* with void we should be able to store pretty much anything */
	free_fn free_;
};

/* free wrappers */
DECLARE_FREE_FN(raft_git_reference_free);
DECLARE_FREE_FN(raft_git_commit_free);
DECLARE_FREE_FN(raft_git_tree_free);
DECLARE_FREE_FN(raft_git_repository_free);

typedef struct raft_oid_entry raft_oid_entry_s;

#define RAFT_OID_ENTRIES_COUNT 10

struct raft_oid_entries
{
	RAFT_OBJ_HEADER_UNION(header, raft_obj_ll_s);
	raft_oid_entry_s entries[RAFT_OID_ENTRIES_COUNT];
};

typedef struct raft_oid_entries raft_oid_entries_s;

raft_oid_entry_s raft_oid_get_entry(git_oid* oid);

RAFT_REFCOUNT_TYPE raft_oid_entry_addref(raft_oid_entry_s*);

RAFT_REFCOUNT_TYPE raft_oid_entry_release(raft_oid_entry_s*);

#endif /* RAFT_INTERNALS_H */
