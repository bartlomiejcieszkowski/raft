#include "raft_base.h"

raft_memory_statistics_s g_raft_memory_statistics = { 0 };

static inline void memory_usage_add(size_t size)
{
	++g_raft_memory_statistics.current_allocations;
	if (g_raft_memory_statistics.peak_allocations < g_raft_memory_statistics.current_allocations)
	{
		g_raft_memory_statistics.peak_allocations = g_raft_memory_statistics.current_allocations;
	}
#if DEBUG
	g_raft_memory_statistics.current_memory_usage += size;
	if (g_raft_memory_statistics.peak_memory_usage < g_raft_memory_statistics.current_memory_usage)
	{
		g_raft_memory_statistics.peak_memory_usage = g_raft_memory_statistics.current_memory_usage;
	}
#endif
}

static inline void memory_usage_sub(size_t size)
{
#if DEBUG
	g_raft_memory_statistics.current_memory_usage -= size;
#endif
	--g_raft_memory_statistics.current_allocations;
}

void* raft_malloc(size_t size)
{
	void* ptr = malloc(size
#if DEBUG
		+ sizeof(raft_debug_header_s)
#endif
	);

	if (ptr) {
		memory_usage_add(size);

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


void* raft_malloc_obj(size_t size, raft_obj_type_e type)
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

		memory_usage_add(size);

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

void raft_free(void* ptr)
{
#if DEBUG
	ptr = &((raft_debug_header_s*)ptr)[-1];
	memory_usage_sub(((raft_debug_header_s*)ptr)->obj_size);
#else
	memory_usage_sub(0);
#endif
	free(ptr);
	
}