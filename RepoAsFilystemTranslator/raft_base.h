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

#endif /* RAFT_BASE_H */