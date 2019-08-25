#include "raft_internals.h"

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


raft_oid_entry_s raft_oid_get_entry(git_oid* oid)
{
	// searches, if not found, gets and caches value
}

RAFT_REFCOUNT_TYPE raft_oid_entry_addref(raft_oid_entry_s* entry)
{
	if (entry)
	{
		return ++entry->refcount;
	}

	return 0;
}

inline void raft_oid_entry_reset(raft_oid_entry_s* entry)
{
	memset(entry, 0, sizeof(raft_oid_entry_s));
}

RAFT_REFCOUNT_TYPE raft_oid_entry_release(raft_oid_entry_s* entry)
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

		raft_oid_entry_reset(entry);
	}

	return refcount;
}


