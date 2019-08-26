#ifndef RAFT_STRING_DICTIONARY_H
#define RAFT_STRING_DICTIONARY_H

#include "raft_structures_dictionary.h"

typedef struct cstring_key {
	size_t len; /* we are going to use it a lot */
	char* string;
} cstring_key_s;

typedef struct wcstring_key {
	size_t len; /* we are going to use it a lot */
	wchar_t* string;
} wcstring_key_s;

DECLARE_DICTIONARY(cstring_key_s, void*, cstring_dictionary);

DECLARE_DICTIONARY(wcstring_key_s, void*, wcstring_dictionary);

#endif /* RAFT_STRING_DICTIONARY_H */
