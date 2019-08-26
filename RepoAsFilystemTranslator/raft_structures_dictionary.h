#ifndef RAFT_STRUCTURES_DICTIONARY_H
#define RAFT_STRUCTURES_DICTIONARY_H

#include "raft_base.h"

#define DECLARE_DICTIONARY(key_type, value_type, name) \
typedef key_type name##_key_s;\
typedef value_type name##_value_s;\
typedef int (*name##_compare_fn)(name##_entry_s* a, name##_entry_s* b);\
typedef int (*name##_key_compare_fn)(name##_key_s* a, name##_key_s* b);\
typedef void (*name##_entry_delete_fn)(name##_entry_s* entry);\
\
typedef struct name##_entry { \
	name##_key_s key; \
	name##_value_s value; \
} name##_entry_s; \
\
typedef struct name {\
	name##_key_compare_fn key_compare_fn; \
	name##_entry_delete_fn entry_delete_fn; \
	size_t size; \
	name##_entry_s* entries; \
} name##_s; \
\
int name##_init(name##_s *dict, size_t initial_size, name##_key_compare_fn compare_fn, name##_entry_delete_fn entry_delete_fn);\
void name##_clear(name##_s *dict); \
name##_value_s name##_get(name##_s *dict, name##_key_s* key); \
int name##_set(name##_s *dict, name##_key_s key, name##_value_s value); \
void name##_remove(name##_s *dict, name##_key_s key); \
\
typedef int (*name##_entry_fn)(name##_entry_s* entry); \
void name##_foreach(name##_s *dict, name##_entry_fn function)




#endif /* RAFT_STRUCTURES_DICTIONARY_H */
