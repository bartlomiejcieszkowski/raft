#ifndef RAFT_LOG_H
#define RAFT_LOG_H

#ifndef LOG_NAME
#error LOG_NAME NOT DEFINED!
#endif

#include <stdio.h>
#include <time.h>
#include <inttypes.h>

extern time_t start_time;

void raft_log_init(void);
void raft_log_exit(void);


/* Logging macros, for easy switch to file logging */
/* 5 zeroes is ok, its about 27 days */
#define LOG_PREFIX "[%05" PRId64 "]["LOG_NAME ":%" PRId32 "]"
#define LOG_PREFIX_PARAMS  (time(NULL) - start_time), __LINE__

#define LOG_EOL "\n"

#define LOG_INFO(message, ...) \
 printf(LOG_PREFIX message LOG_EOL, LOG_PREFIX_PARAMS, ##__VA_ARGS__)
#define LOG_ERROR(message, ...) fprintf(stderr, LOG_PREFIX message LOG_EOL, LOG_PREFIX_PARAMS, ##__VA_ARGS__)

#else /* RAFT_LOG_H */
#error include this only in .c files
#endif