#include "raft_internals.h"

#define LOG_NAME "raft_internals.c"
#include "raft_log.h"

/* free wrappers */
void raft_git_reference_free(void* ptr)
{
	git_reference_free((git_reference*)ptr);
}

void raft_git_commit_free(void* ptr)
{
	git_commit_free((git_commit*)ptr);
}

void raft_git_tree_free(void* ptr)
{
	git_tree_free((git_tree*)ptr);
}

void raft_git_repository_free(void* ptr)
{
	git_repository_free((git_repository*)ptr);
}


raft_entry_s* raft_get_entry(git_oid* oid)
{
	// searches, if not found, gets and caches value
	return NULL;
}

RAFT_REFCOUNT_TYPE raft_entry_addref(raft_entry_s* entry)
{
	if (entry)
	{
		return ++entry->refcount;
	}

	return 0;
}

inline void raft_entry_reset(raft_entry_s* entry)
{
	memset(entry, 0, sizeof(raft_oid_entry_s));
}

RAFT_REFCOUNT_TYPE raft_entry_release(raft_entry_s* entry)
{
	if (entry == NULL)
	{
		return 0;
	}

	RAFT_REFCOUNT_TYPE refcount = --entry->refcount;

	if (refcount == 0)
	{
		/* cleanup git references or commits or whatnot */
		free_fn free_ = entry->free_;
		if (free_)
		{
			free_(entry->value);
		}

		if (entry->header.type == RAFT_OBJ_TYPE_ENTRY_MALLOC)
		{
			raft_free(entry);
		}
		else
		{
			raft_entry_reset(entry);
		}
	}

	return refcount;
}


int raft_is_raft_obj(void* obj, raft_obj_type_e type)
{
	return ((raft_obj_header_s*)obj)->type == type;
}



raft_entry_s* raft_git_get_tree(raft_context_s* this_, git_oid* oid)
{
	int i = 0;
	while (i < RAFT_OID_ENTRIES_COUNT)
	{
		if (0 == memcmp(this_->tree_cache.entries[i].key.id, oid->id, sizeof(oid->id)))
		{
			raft_entry_addref(&this_->tree_cache.entries[i].value);
			return &this_->tree_cache.entries[i].value;
		}
		++i;
	}

	i = 0;
	git_oid oid_zero = { 0 };
	while (i < RAFT_OID_ENTRIES_COUNT)
	{
		if (0 != memcmp(this_->tree_cache.entries[i].key.id, oid_zero.id, sizeof(oid_zero.id)))
		{
			++i;
			continue;
		}
		
		break;
	}

	/*
	 . Search for entry
	 .. if found return
	 . get commit
	 . get tree
	 . store tree
	 . addref
	 . return
	*/
	int ec;
	git_commit* commit = NULL;
	git_tree* tree = NULL;




	ec = git_commit_lookup(&commit, this_->repository, oid);

	if (ec != 0) {
		char oid_string[10];
		git_oid_tostr(oid_string, 10, oid);
		LOG_ERROR("No commit for id %s, ec=%d", oid_string, ec);
		return NULL;
	}

	ec = git_commit_tree(&tree, commit);
	if (ec != 0) {
		char oid_string[10];
		git_oid_tostr(oid_string, 10, oid);
		LOG_ERROR("Failed getting tree for id %s, ec=%d", oid_string, ec);
		git_commit_free(commit);
		return NULL;
	}

	raft_entry_s* entry = NULL;

	/* we have space for cache */
	if (i < RAFT_OID_ENTRIES_COUNT)
	{
		this_->tree_cache.entries[i].key = *oid;
		entry = &this_->tree_cache.entries[i].value;
	}
	else
	{
		entry = (raft_entry_s*) raft_malloc_obj(sizeof(raft_entry_s), RAFT_OBJ_TYPE_ENTRY_MALLOC);
	}

	entry->free_ = &raft_git_tree_free;
	entry->value = (void*)tree;
	
	git_commit_free(commit);

	/* 1 for cache */
	raft_entry_addref(entry);

	/* 2nd for caller */
	raft_entry_addref(entry);
	return entry;
}