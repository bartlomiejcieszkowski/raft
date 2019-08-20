#ifndef RAFT_OPERATIONS_H
#define RAFT_OPERATIONS_H

#include <dokan/dokan.h>
#include <windows.h>

void raft_set_dokan_operations(PDOKAN_OPERATIONS);

#endif /* RAFT_OPERATIONS_H */