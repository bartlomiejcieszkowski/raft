#define LOG_NAME "raft_log.c"
#include "raft_log.h"

time_t start_time = 0;

void raft_log_init(void)
{
	start_time = time(NULL);
}

void raft_log_exit(void)
{
	time_t total_time = time(NULL) - start_time;
	if (total_time == 0) total_time = 1;
	LOG_INFO("Total running time: %02lld:%02lld:%02lld", (total_time / 60) / 60, (total_time / 60) % 60, total_time % 60);
}
