#include "raft_string_dictionary.h"

int cstring_dictionary_init(cstring_dictionary_s* dict, size_t initial_size, cstring_dictionary_key_compare_fn compare_fn, cstring_dictionary_entry_delete_fn entry_delete_fn)
{
	cstring_dictionary_entry_s* entries = (cstring_dictionary_entry_s*)raft_malloc(sizeof(cstring_dictionary_entry_s) * initial_size);
	if (entries == NULL)
		return -ENOMEM;

	dict->key_compare_fn = compare_fn;
	dict->entry_delete_fn = entry_delete_fn;
	dict->entries = entries;
	dict->size = initial_size;
}

void cstring_dictionary_clear(cstring_dictionary_s* dict)
{
	if (dict->entries)
	{
		if (dict->entry_delete_fn) {
			while (dict->size > 0) {
				dict->entry_delete_fn(&dict->entries[dict->size - 1]);
				--dict->size;
			}
		}
		else {
			dict->size = 0;
		}

		raft_free(dict->entries);
		dict->entries = NULL;
	}
}

cstring_dictionary_value_s cstring_dictionary_get(cstring_dictionary_s* dict, cstring_dictionary_key_s* key)
{

}

int cstring_dictionary_set(cstring_dictionary_s* dict, cstring_dictionary_key_s key, cstring_dictionary_value_s value)
{

}

void cstring_dictionary_remove(cstring_dictionary_s* dict, cstring_dictionary_key_s key)
{

}

void cstring_dictionary_foreach(cstring_dictionary_s* dict, cstring_dictionary_entry_fn function)
{

}