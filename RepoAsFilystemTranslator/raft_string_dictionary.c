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
	return 0;
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

cstring_dictionary_value_s* cstring_dictionary_get(cstring_dictionary_s* dict, cstring_dictionary_key_s key)
{
	if (dict->entries && dict->size)
	{
		size_t i = 0;
		while (i < dict->size) {
			if (dict->entries[i].valid && dict->key_compare_fn(key, dict->entries[i].key) == 0) {
				return dict->entries[i].value;
			}
		}
	}

	return NULL;
}

int cstring_dictionary_resize(cstring_dictionary_s* dict, size_t size)
{
	if (size < dict->size)
	{
		// !truncate!
		return -1;
	}

	cstring_dictionary_entry_s* new_entries = (cstring_dictionary_entry_s*)raft_malloc(size);

	memcpy_s(new_entries, size, dict->entries, dict->size);
	cstring_dictionary_entry_s* old_entries = dict->entries;
	dict->entries = new_entries;
	dict->size = size;

	raft_free(old_entries);
	return 0;
}

int cstring_dictionary_set(cstring_dictionary_s* dict, cstring_dictionary_key_s key, cstring_dictionary_value_s value)
{
	cstring_dictionary_value_s* entry_value = cstring_dictionary_get(dict, key);
	if (entry_value) {
		// update
		*entry_value = value;
		return 0;
	}

	size_t i = 0;
	while (i < dict->size) {
		if (dict->entries[i].valid == 0) {
			dict->entries[i].key = key;
			dict->entries[i].value = value;
			dict->entries[i].valid = 1;
			return 0;
		}
	}

	// resize
	cstring_dictionary_resize(dict, dict->size * 2);

	while (i < dict->size) {
		if (dict->entries[i].valid == 0) {
			dict->entries[i].key = key;
			dict->entries[i].value = value;
			dict->entries[i].valid = 1;
			return 0;
		}
	}

	return -1;
}

void cstring_dictionary_remove(cstring_dictionary_s* dict, cstring_dictionary_key_s key)
{
	if (dict->entries && dict->size)
	{
		size_t i = 0;
		while (i < dict->size) {
			if (dict->entries[i].valid && dict->key_compare_fn(key, dict->entries[i].key) == 0) {
				if (dict->entry_delete_fn)
				{
					dict->entry_delete_fn(&dict->entries[i]);
					
				}
				dict->entries[i].valid = 0;
			}
		}
	}
}

void cstring_dictionary_foreach(cstring_dictionary_s* dict, cstring_dictionary_entry_fn function)
{
	size_t i = 0;
	while (i < dict->size) {
		if (dict->entries[i].valid)
		{
			function(&dict->entries[i]);
		}
		++i;
	}
}