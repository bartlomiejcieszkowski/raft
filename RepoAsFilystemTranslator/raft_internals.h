#ifndef RAFT_INTERNALS_H
#define RAFT_INTERNALS_H

#include "raft_base.h"

int raft_is_raft_obj(void* obj, raft_obj_type_e type);

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

struct raft_entry
{
	RAFT_OBJ_HEADER_UNION(header, raft_obj_ll_parent_s);
	RAFT_REFCOUNT_TYPE refcount;
	free_fn free_;
	void* value; /* with void we should be able to store pretty much anything */
};

typedef struct raft_entry raft_entry_s;

// git_oid is 20 bytes, 5dwords.. yeah, we can afford it
struct raft_oid_entry
{
	RAFT_OBJ_HEADER(header);
	git_oid key; /* assume oid 0 is invalid or null is invalid.. yeah */
	raft_entry_s value;
};

typedef struct raft_oid_entry raft_oid_entry_s;

/* free wrappers */
DECLARE_FREE_FN(raft_git_reference_free);
DECLARE_FREE_FN(raft_git_commit_free);
DECLARE_FREE_FN(raft_git_tree_free);
DECLARE_FREE_FN(raft_git_repository_free);



#define RAFT_OID_ENTRIES_COUNT 10

struct raft_oid_entries
{
	RAFT_OBJ_HEADER_UNION(header, raft_obj_ll_s);
	raft_oid_entry_s entries[RAFT_OID_ENTRIES_COUNT];
};

typedef struct raft_oid_entries raft_oid_entries_s;

raft_entry_s* raft_get_entry(git_oid* oid);

RAFT_REFCOUNT_TYPE raft_entry_addref(raft_entry_s*);

RAFT_REFCOUNT_TYPE raft_entry_release(raft_entry_s*);


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

struct raft_context {
	buffer_char_s repository_path;
	buffer_wchar_t_s mount_point;
	git_repository* repository;
	git_strarray* repository_remotes;
	git_strarray repository_remotes_internal;
	git_strarray* tags;
	git_strarray tags_internal;

	raft_oid_entries_s tree_cache;
};




struct bitmap_path {
	int bitmap[DOKAN_MAX_PATH / (sizeof(int) * 8) + ((DOKAN_MAX_PATH % (sizeof(int) * 8) > 0) ? 1 : 0)];
};

typedef struct bitmap_path bitmap_path_s;


typedef struct raft_context raft_context_s;

raft_entry_s* raft_git_get_tree(raft_context_s*, git_oid*);



#endif /* RAFT_INTERNALS_H */
