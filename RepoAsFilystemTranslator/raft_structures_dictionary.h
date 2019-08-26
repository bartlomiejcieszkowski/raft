#ifndef RAFT_STRUCTURES_DICTIONARY_H
#define RAFT_STRUCTURES_DICTIONARY_H

#include "raft_base.h"

#define DECLARE_DICTIONARY(key_type, value_type, name) \
typedef struct name##_entry { \
	key_type key; \
	value_type value; \
} name##_entry_s; \
\
typedef struct name {\
    name##_entry_s* entries; \
} name##_s; \
\
void name##_clear(name##_s *dict); \
value_type name##_get(name##_s *dict, key_type key); \
int name##_set(name##_s *dict, key_type key, value_type value); \
void name##_remove(name##_s *dict, key_type key); \
\
typedef int (*name##_entry_fn)(name##_entry_s* entry); \
void name##_foreach(name##_entry_fn function)




#endif /* RAFT_STRUCTURES_DICTIONARY_H */
