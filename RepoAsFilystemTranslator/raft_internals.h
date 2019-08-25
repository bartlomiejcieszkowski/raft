#ifndef RAFT_INTERNALS_H
#define RAFT_INTERNALS_H

#include <stdint.h>
#include <stdlib.h>
#include <string>


enum raft_obj_type {
	RAFT_OBJ_TYPE_UNDEFINED = 0,
	RAFT_OBJ_TYPE_BRANCH = 1,
	RAFT_OBJ_TYPE_REMOTE = 2,
	RAFT_OBJ_TYPE_TAG = 3,
};

typedef enum raft_obj_type raft_obj_type_e;

struct raft_obj_header {
	raft_obj_type_e type;
	struct raft_obj_header* child_obj;
};

typedef struct raft_obj_header raft_obj_header_s;

int is_raft_obj(void* obj, raft_obj_type_e type)
{
	return ((raft_obj_header_s*)obj)->type == type;
}





#endif /* RAFT_INTERNALS_H */
