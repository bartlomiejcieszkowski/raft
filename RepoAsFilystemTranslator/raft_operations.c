#include "raft_operations.h"

#include <git2.h>

#define LOG_NAME "raft_operations.c"
#include "raft_log.h"

#define DUMMY_FILE 1
#if DUMMY_FILE
const char dummy_text[] = "Hello Raft, lorem ipsum dolor sid omet.";
#define DUMMY_FILE_BUFFER_SIZE 4096
#define DUMMY_FILE_SIZE (4 + DUMMY_FILE_BUFFER_SIZE)
#endif

static NTSTATUS DOKAN_CALLBACK raft_operations_CreateFile(LPCWSTR FileName, PDOKAN_IO_SECURITY_CONTEXT SecurityContext,
	ACCESS_MASK DesiredAccess, ULONG FileAttributes,
	ULONG ShareAccess, ULONG CreateDisposition,
	ULONG CreateOptions, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("CreateFile: %ls 0x%X 0x%X 0x%X 0x%X 0x%X ", FileName, DesiredAccess, FileAttributes, ShareAccess, CreateDisposition, CreateOptions);

	if (CreateDisposition == CREATE_NEW) {
		LOG_DEBUG("CREATE_NEW");
	}
	else if (CreateDisposition == OPEN_ALWAYS) {
		LOG_DEBUG("OPEN_ALWAYS");
	}
	else if (CreateDisposition == CREATE_ALWAYS) {
		LOG_DEBUG("CREATE_ALWAYS");
	}
	else if (CreateDisposition == OPEN_EXISTING) {
		LOG_DEBUG("OPEN_EXISTING");
	}
	else if (CreateDisposition == TRUNCATE_EXISTING) {
		LOG_DEBUG("TRUNCATE_EXISTING");
	}
	else {
		LOG_DEBUG("UNKNOWN creationDisposition!");
	}

	ACCESS_MASK genericDesiredAccess;
	DWORD fileAttributesAndFlags;
	DWORD creationDisposition;

	DokanMapKernelToUserCreateFileFlags(
		DesiredAccess, FileAttributes, CreateOptions, CreateDisposition,
		&genericDesiredAccess, &fileAttributesAndFlags, &creationDisposition);


	if (DokanFileInfo->IsDirectory)
	{

	}
	else
	{
		// Truncate should always be used with write access
		if (creationDisposition == TRUNCATE_EXISTING)
		{
			genericDesiredAccess |= GENERIC_WRITE;
		}

#if DUMMY_FILE
		uint32_t* dummyFile = (uint32_t*)raft_malloc(DUMMY_FILE_SIZE);
		dummyFile[0] = 0;
		++dummyFile[0];
		DokanFileInfo->Context = (UINT64)dummyFile;
		char* textPointer = &dummyFile[1];
		memcpy_s(textPointer, DUMMY_FILE_BUFFER_SIZE, dummy_text, sizeof(dummy_text));

		// this is called a lot, we need to keep handles, and get them from dict
		// add refcount, and decrement, as this is what is expect to be done
		// CloseHandle() OpenHandle()
#endif
	}

	return status;
}

static void DOKAN_CALLBACK raft_operations_Cleanup(LPCWSTR FileName,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("Cleanup: %ls", FileName);
}

static void DOKAN_CALLBACK raft_operations_CloseFile(LPCWSTR FileName,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("CloseFile: %ls", FileName);
	HANDLE handle = (HANDLE)DokanFileInfo->Context;
	if (handle && handle != INVALID_HANDLE_VALUE)
	{
#if DUMMY_FILE
		uint32_t* dummyFile = (uint32_t*)handle;
		--dummyFile[0];
		if (dummyFile[0] == 0)
		{
			raft_free(dummyFile);
		}
#endif
	}
	DokanFileInfo->Context = 0;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_ReadFile(LPCWSTR FileName, LPVOID Buffer,
	DWORD BufferLength,
	LPDWORD ReadLength,
	LONGLONG Offset,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("ReadFile: %ls", FileName);
	HANDLE handle = (HANDLE)DokanFileInfo->Context;
	if (handle && handle != INVALID_HANDLE_VALUE)
	{
#if DUMMY_FILE
		uint32_t* dummyFile = (uint32_t*)handle;
		char* textPointer = &dummyFile[1];

		if (Buffer && ReadLength)
		{
			if (Offset < DUMMY_FILE_BUFFER_SIZE)
			{
				if ((BufferLength) >= (DUMMY_FILE_BUFFER_SIZE - Offset))
				{
					memcpy_s(Buffer, BufferLength, textPointer + Offset, DUMMY_FILE_BUFFER_SIZE - Offset);
					*ReadLength = DUMMY_FILE_BUFFER_SIZE - Offset;
				}
				else
				{
					memcpy_s(Buffer, BufferLength, textPointer + Offset, BufferLength);
					*ReadLength = BufferLength;
				}
			}
			else
			{
				*ReadLength = 0;
			}
		}
#endif
	}
	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_WriteFile(LPCWSTR FileName, LPCVOID Buffer,
	DWORD NumberOfBytesToWrite,
	LPDWORD NumberOfBytesWritten,
	LONGLONG Offset,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("WriteFile: %ls", FileName);
	HANDLE handle = (HANDLE)DokanFileInfo->Context;
	if (handle && handle != INVALID_HANDLE_VALUE)
	{
#if DUMMY_FILE
		uint32_t* dummyFile = (uint32_t*)handle;
		char* textPointer = &dummyFile[1];

		if (Buffer && NumberOfBytesWritten)
		{
			if (Offset < DUMMY_FILE_BUFFER_SIZE)
			{
				if ((NumberOfBytesToWrite) <= (DUMMY_FILE_BUFFER_SIZE - Offset))
				{
					memcpy_s(textPointer + Offset, DUMMY_FILE_BUFFER_SIZE - Offset, Buffer, NumberOfBytesToWrite);
					*NumberOfBytesWritten = NumberOfBytesToWrite;
				}
				else
				{
					memcpy_s(textPointer + Offset, DUMMY_FILE_BUFFER_SIZE - Offset,
						Buffer, DUMMY_FILE_BUFFER_SIZE - Offset);
					*NumberOfBytesWritten = DUMMY_FILE_BUFFER_SIZE - Offset;
				}
			}
			else
			{
				*NumberOfBytesWritten = 0;
			}
		}
#endif
	}
	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_FlushFileBuffers(LPCWSTR FileName, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("FlusFileBuffers: %ls", FileName);
	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_GetFileInformation(
	LPCWSTR FileName, LPBY_HANDLE_FILE_INFORMATION HandleFileInformation,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("GetFileInformation: %ls", FileName);
	return status;
}

wchar_t path_branch[] = L"branch";
wchar_t path_tags[] = L"tags";
wchar_t path_remotes[] = L"remotes";

#define STRING_PATH_SEPARATOR '\\'
#define WSTRING_PATH_SEPARATOR L'\\'
#define CSTRING_NULL '\0'
#define WSTRING_NULL L'\0'


bitmap_path_s cstring_get_offsets(char character, const char* cstring, int* count)
{
	bitmap_path_s retVal = { 0 };
	int offset = 0;
	if (count) { *count = 0; }
	while (cstring[offset] != CSTRING_NULL)
	{
		if (character == cstring[offset])
		{
			retVal.bitmap[offset / (sizeof(int) * 8)] |= (1 << (offset % (sizeof(int) * 8)));
			if (count) { *count += 1; }
		}
		++offset;
	}
	return retVal;
}

static int cstring_subcmp(const char* cstring, const char* substring, int start_offset)
{
	int offset = 0;
	while (substring[offset] != L'\0')
	{
		if (substring[offset] == cstring[start_offset + offset])
		{
			++offset;
			continue;
		}

		return substring[offset] < cstring[start_offset + offset] ? -1 : 1;

	}
	return 0;
}

bitmap_path_s wstring_get_offsets(wchar_t character, LPCWSTR wstring, int* count)
{
	bitmap_path_s retVal = { 0 };
	int offset = 0;
	if (count) { *count = 0; }
	while (wstring[offset] != WSTRING_NULL)
	{
		if (character == wstring[offset])
		{
			retVal.bitmap[offset / (sizeof(int) * 8)] |= (1 << (offset % (sizeof(int) * 8)));
			if (count) { *count += 1; }
		}
		++offset;
	}
	return retVal;
}

int bitmap_get_next_occurence(int last_offset, bitmap_path_s* bitmap_path)
{
	if (NULL == bitmap_path) { return -2; }

	if (last_offset < 0)
	{
		last_offset = 0;
	}
	else
	{
		++last_offset;
	}

	while (last_offset < DOKAN_MAX_PATH)
	{
		int result = last_offset / (sizeof(int) * 8);
		int modulo = last_offset % (sizeof(int) * 8);

		/*
		0000000001
		 ^------------ are any 1?
		 where val = 1 << offset;
		(~((val  - 1) | val)) & bitmap

		00000000 ~
		11111111 &
		11111110

		11111110 &
		00000001

		00000000

		is any here? nope

		bitmap
		10110101
		offset = 5

		00010000
		-1
		00001111 | val
		00011111 ~
		11100000 &
		11100000

		bitmap
		00010101

		00000000
		*/

		int val = 1 << modulo;
		int a = ~(val - 1);
		if (a & bitmap_path->bitmap[result])
		{
			while (modulo < (sizeof(int) * 8))
			{
				if ((bitmap_path->bitmap[result] & (1 << modulo)) != 0)
				{
					return result * (sizeof(int) * 8) + modulo;
				}
				++modulo;
			}
		}

		// 0, go next, eg. 2 -> 3
		// 4 * 8 = 32 * (2+1) = 96 -> 96 /32 = 3 96%32 = 0
		last_offset = (result+1) * (sizeof(int) * 8);
	}
	return -1;
}

static int wstring_subcmp(LPCWSTR wstring, LPCWSTR substring, int start_offset)
{
	int offset = 0;
	while (substring[offset] != L'\0')
	{
		if (substring[offset] == wstring[start_offset + offset])
		{
			++offset;
			continue;
		}

		return substring[offset] < wstring[start_offset + offset] ? -1 : 1;

	}
	return 0;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_find_files(LPCWSTR FileName, PFillFindData FillFindData, PDOKAN_FILE_INFO DokanFileInfo, int offset)
{

}

git_strarray* raft_context_get_remote_list(raft_context_s* this_)
{
	if (this_->repository)
	{
		if (this_->repository_remotes)
		{
			return this_->repository_remotes;
		}

		int err = git_remote_list(&this_->repository_remotes_internal, this_->repository);
		if (err == 0)
		{
			this_->repository_remotes = &this_->repository_remotes_internal;
			return this_->repository_remotes;
		}

		LOG_ERROR("Failed git_remote_list with ec=%d", err);
	}

	return NULL;
}

git_strarray* raft_context_get_tag_list(raft_context_s* this_)
{
	if (this_->repository)
	{
		if (this_->tags)
		{
			return this_->tags;
		}

		int err = git_tag_list(&this_->tags_internal, this_->repository);
		if (err == 0)
		{
			this_->tags = &this_->tags_internal;
			return this_->tags;
		}

		LOG_ERROR("Failed git_remote_list with ec=%d", err);
	}

	return NULL;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_find_files_remotes(LPCWSTR FileName, PFillFindData FillFindData, PDOKAN_FILE_INFO DokanFileInfo, int offset, bitmap_path_s* separator_bitmap)
{
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;

	/* we are at root of handler*/
	if (offset < 0)
	{
		git_strarray* remotes = raft_context_get_remote_list(this_);
		if (remotes)
		{
			// root
			WIN32_FIND_DATAW find_data = { 0 };
			size_t converted = 0;

			int i = 0;
			while (i < remotes->count)
			{
				mbstowcs_s(&converted, find_data.cFileName, (sizeof(find_data.cFileName) / sizeof(wchar_t)),
					remotes->strings[i], strlen(remotes->strings[i]));
				find_data.dwFileAttributes = FILE_ATTRIBUTE_DIRECTORY;
				FillFindData(&find_data, DokanFileInfo);
				++i;
			}
		}

		return STATUS_SUCCESS;
	}

	// subdir
	return STATUS_SUCCESS;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_traverse_commit(LPCWSTR FileName, PFillFindData FillFindData, PDOKAN_FILE_INFO DokanFileInfo, int offset, bitmap_path_s* separator_bitmap, git_tree* tree)
{
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;

	BOOL targetSubtree = offset == -1 ? TRUE : FALSE;
	while (targetSubtree != TRUE)
	{
		int next_offset = bitmap_get_next_occurence(offset, separator_bitmap);

		BOOL isLast = next_offset == -1 ? TRUE : FALSE;
		int substringLen = isLast == TRUE ?
			(wcslen(&FileName[offset])) : (next_offset - offset);

		char* subPath = (char*)malloc(substringLen);

		wcstombs_s(NULL, subPath, substringLen, &FileName[offset + 1], substringLen - 1);


		size_t tree_entrycount = git_tree_entrycount(tree);
		size_t i = 0;
		while (i < tree_entrycount)
		{
			const git_tree_entry* tree_entry = git_tree_entry_byindex(tree, i);
			git_object_t object_type = git_tree_entry_type(tree_entry);
			const char* name = git_tree_entry_name(tree_entry);
			if (0 == strcmp(subPath, name))
			{
				if (object_type != GIT_OBJECT_TREE)
				{
					LOG_ERROR("Request for directory, but is not a tree.. %s", subPath);
					free(subPath);
					return STATUS_INVALID_PARAMETER;
				}

				int error = git_tree_lookup(&tree, this_->repository, git_tree_entry_id(tree_entry));
				// TODO: do i need to free this tree?
				if (error != 0)
				{
					LOG_ERROR("Request for directory, but failed to get a tree.. %s", subPath);
					free(subPath);
					return STATUS_INVALID_PARAMETER;
				}

				break;
			}
			++i;
		}
		free(subPath);
		offset = next_offset;
		targetSubtree = offset == -1 ? TRUE : FALSE;
	}

	// we are at right subtree

	WIN32_FIND_DATAW find_data = { 0 };
	size_t tree_entrycount = git_tree_entrycount(tree);
	size_t i = 0;
	while (i < tree_entrycount)
	{
		const git_tree_entry* tree_entry = git_tree_entry_byindex(tree, i);

		git_object_t object_type = git_tree_entry_type(tree_entry);
		if (object_type == GIT_OBJECT_TREE)
		{
			// dir
			find_data.dwFileAttributes = FILE_ATTRIBUTE_DIRECTORY;
		}
		else if (object_type == GIT_OBJECT_BLOB)
		{
			// file ro
			find_data.dwFileAttributes = FILE_ATTRIBUTE_READONLY;
		}

		size_t converted;
		const char* name = git_tree_entry_name(tree_entry);
		mbstowcs_s(&converted, find_data.cFileName, (sizeof(find_data.cFileName) / sizeof(wchar_t)),
			name, strlen(name));
		FillFindData(&find_data, DokanFileInfo);
		++i;
	}
	
	return STATUS_SUCCESS;
}


static NTSTATUS DOKAN_CALLBACK raft_operations_find_files_branch(LPCWSTR FileName, PFillFindData FillFindData, PDOKAN_FILE_INFO DokanFileInfo, int offset, bitmap_path_s* separator_bitmap)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;

	/* we are at root of handler*/
	if (offset < 0)
	{
		git_branch_iterator* iterator;

		int err = git_branch_iterator_new(&iterator, this_->repository, GIT_BRANCH_LOCAL);
		if (err == 0)
		{
			// root
			WIN32_FIND_DATAW find_data = { 0 };
			size_t converted = 0;

			git_branch_t type;
			git_reference* reference;
			bitmap_path_s separator_reference;
			int count = 0;
			while (GIT_ITEROVER != git_branch_next(&reference, &type, iterator))
			{
				const char* name = git_reference_name(reference);


				separator_reference = cstring_get_offsets('/', name, &count);
				/* for is: refs/heads/master .. */
				if (count == 2) {
					int offset = bitmap_get_next_occurence(-1, &separator_reference);
					offset = bitmap_get_next_occurence(offset, &separator_reference); // get last?

					mbstowcs_s(&converted, find_data.cFileName, (sizeof(find_data.cFileName) / sizeof(wchar_t)),
						&name[offset+1], strlen(name) - offset - 1);
					find_data.dwFileAttributes = FILE_ATTRIBUTE_DIRECTORY;
					FillFindData(&find_data, DokanFileInfo);
				}
				git_reference_free(reference);

			}

			git_branch_iterator_free(iterator);
		}

		return STATUS_SUCCESS;
	}

	// subdir
	int next_offset = bitmap_get_next_occurence(offset, separator_bitmap);
	BOOL isBranchRoot = next_offset == -1 ? TRUE : FALSE;

	int substringLen = isBranchRoot == TRUE ?
		(wcslen(&FileName[offset])) : (next_offset - offset);

	char* localName = (char*)malloc(substringLen);

	wcstombs_s(NULL, localName, substringLen, &FileName[offset + 1], substringLen - 1);
	
	git_reference* ref = NULL;
	int ec = git_reference_dwim(&ref, this_->repository, localName);
	//iteratable_ =  raft_get_file_tree(this_, localName);

	if (ref)
	{
		if (git_reference_is_branch(ref))
		{
			// yay
			LOG_DEBUG("Branch %s", localName);
			git_oid* commit_oid;
			git_commit* commit = NULL;
			git_tree* tree = NULL;
			char oid_string[10];
			//ec = git_reference_name_to_id(&commit_oid, this_->repository, localName); // long name needed refs/head/blabla

			commit_oid = git_reference_target(ref);
			git_reference_free(ref);

			raft_entry_s* entry = raft_git_get_tree(this_, commit_oid);
			if (entry)
			{
				git_tree* tree = (git_tree*)entry->value;
				
				status = raft_operations_traverse_commit(FileName, FillFindData, DokanFileInfo, next_offset, separator_bitmap, tree);
				raft_entry_release(entry);
			}
		}
	}

	free(localName);
	


	return STATUS_SUCCESS;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_find_files_tags(LPCWSTR FileName, PFillFindData FillFindData, PDOKAN_FILE_INFO DokanFileInfo, int offset, bitmap_path_s* separator_bitmap)
{
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;

	/* we are at root of handler*/
	if (offset < 0)
	{
		git_branch_iterator* iterator;

		git_strarray* tags = raft_context_get_tag_list(this_);
		if (tags)
		{
			// root
			WIN32_FIND_DATAW find_data = { 0 };
			size_t converted = 0;

			int i = 0;
			while (i < tags->count)
			{
				mbstowcs_s(&converted, find_data.cFileName, (sizeof(find_data.cFileName) / sizeof(wchar_t)),
					tags->strings[i], strlen(tags->strings[i]));
				find_data.dwFileAttributes = FILE_ATTRIBUTE_DIRECTORY;
				FillFindData(&find_data, DokanFileInfo);
				++i;
			}
		}

		return STATUS_SUCCESS;
	}

	// subdir
	return STATUS_SUCCESS;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_FindFiles(LPCWSTR FileName,
	PFillFindData FillFindData,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	

	LOG_DEBUG("FindFiles: %ls", FileName);
	// CACHE RESULTS, as the explorer asks millions of times for file list -_-"

	if (this_->repository == NULL)
	{
		return STATUS_INVALID_PARAMETER;
	}

	int count;
	bitmap_path_s separators_bitmap = wstring_get_offsets(WSTRING_PATH_SEPARATOR, FileName, &count);
	int len = wcslen(FileName);
	int offset = bitmap_get_next_occurence(-1, &separators_bitmap);
	// so for root we have: "\\", but for subfolder we have "\\subfolder"
	if ((len - offset) == 1)
	{
		if (offset != 0)
		{
			LOG_ERROR("How did we even get here?");
		}
		else
		{
			WIN32_FIND_DATAW find_data;
			// root
			memcpy(find_data.cFileName, path_branch, (wcslen(path_branch) + 1) * 2);
			find_data.dwFileAttributes = FILE_ATTRIBUTE_DIRECTORY;
			FillFindData(&find_data, DokanFileInfo);

			memcpy(find_data.cFileName, path_tags, (wcslen(path_tags) + 1) * 2);
			find_data.dwFileAttributes = FILE_ATTRIBUTE_DIRECTORY;
			FillFindData(&find_data, DokanFileInfo);

			memcpy(find_data.cFileName, path_remotes, (wcslen(path_remotes) + 1) * 2);
			find_data.dwFileAttributes = FILE_ATTRIBUTE_DIRECTORY;
			FillFindData(&find_data, DokanFileInfo);
		}
	}
	else
	{
		int next_offset = bitmap_get_next_occurence(offset, &separators_bitmap);
		int delta = ((next_offset < 0) ? len : next_offset) - offset - 1;

		switch (delta)
		{
		case 7: /* remotes */
			if (wstring_subcmp(FileName, path_remotes, offset + 1) == 0)
			{
				status = raft_operations_find_files_remotes(FileName, FillFindData, DokanFileInfo, next_offset, &separators_bitmap);
			}
			break;
		case 6: /* branch*/
			if (wstring_subcmp(FileName, path_branch, offset + 1) == 0)
			{
				status = raft_operations_find_files_branch(FileName, FillFindData, DokanFileInfo, next_offset, &separators_bitmap);
			}
			break;
		case 4: /* tags */
			if (wstring_subcmp(FileName, path_tags, offset + 1) == 0)
			{
				status = raft_operations_find_files_tags(FileName, FillFindData, DokanFileInfo, next_offset, &separators_bitmap);
			}
			break;
		}
	}

	LOG_DEBUG("Exit, status=0x%08X.", status);
	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_SetFileAttributes(
	LPCWSTR FileName, DWORD FileAttributes, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("SetFileAttributes: %ls", FileName);

	return status;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_SetFileTime(LPCWSTR FileName, CONST FILETIME* CreationTime,
	CONST FILETIME* LastAccessTime, CONST FILETIME* LastWriteTime,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("SetFileTime: %ls", FileName);

	return status;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_DeleteFile(LPCWSTR FileName, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("DeleteFile: %ls", FileName);

	return status;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_DeleteDirectory(LPCWSTR FileName, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("DeleteDirectory: %ls", FileName);

	return status;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_MoveFile(LPCWSTR FileName, // existing file name
	LPCWSTR NewFileName, BOOL ReplaceIfExisting,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("MoveFile: %ls", FileName);

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_SetEndOfFile(
	LPCWSTR FileName, LONGLONG ByteOffset, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("SetEndOfFile: %ls", FileName);

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_SetAllocationSize(
	LPCWSTR FileName, LONGLONG AllocSize, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("SetAllocationSize: %ls", FileName);

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_LockFile(LPCWSTR FileName,
	LONGLONG ByteOffset,
	LONGLONG Length,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("LockFile: %ls", FileName);

	return status;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_UnlockFile(LPCWSTR FileName, LONGLONG ByteOffset, LONGLONG Length,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("UnlockFile: %ls", FileName);

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_GetFileSecurity(
	LPCWSTR FileName, PSECURITY_INFORMATION SecurityInformation,
	PSECURITY_DESCRIPTOR SecurityDescriptor, ULONG BufferLength,
	PULONG LengthNeeded, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("GetFileSecurity: %ls", FileName);

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_SetFileSecurity(
	LPCWSTR FileName, PSECURITY_INFORMATION SecurityInformation,
	PSECURITY_DESCRIPTOR SecurityDescriptor, ULONG SecurityDescriptorLength,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("SetFileSecurity: %ls", FileName);

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_DokanGetDiskFreeSpace(
	PULONGLONG FreeBytesAvailable, PULONGLONG TotalNumberOfBytes,
	PULONGLONG TotalNumberOfFreeBytes, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("DokanGetDiskFreeSpace");

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_GetVolumeInformation(
	LPWSTR VolumeNameBuffer, DWORD VolumeNameSize, LPDWORD VolumeSerialNumber,
	LPDWORD MaximumComponentLength, LPDWORD FileSystemFlags,
	LPWSTR FileSystemNameBuffer, DWORD FileSystemNameSize,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("GetVolumeInformation");
	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_Unmounted(PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("Unmounted");

	git_repository* repository = this_->repository;
	this_->repository = NULL;

	git_repository_free(repository);

	return status;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_FindStreams(LPCWSTR FileName, PFillFindStreamData FillFindStreamData,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	LOG_DEBUG("FindStreams: %ls", FileName);

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_Mounted(PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;

	LOG_DEBUG("Mount repo %s", this_->repository_path.buffer);
	int ec = git_repository_open(&this_->repository, this_->repository_path.buffer);
	if (ec != 0)
	{
		LOG_ERROR("Git repo open failed for %s with ec=0x%X", this_->repository_path.buffer, ec);
		status = STATUS_INVALID_PARAMETER;
	}

	return status;
}


void raft_set_dokan_operations(PDOKAN_OPERATIONS dokan_op)
{
	dokan_op->ZwCreateFile = raft_operations_CreateFile;
	dokan_op->Cleanup = raft_operations_Cleanup;
	dokan_op->CloseFile = raft_operations_CloseFile;
	dokan_op->ReadFile = raft_operations_ReadFile;
	dokan_op->WriteFile = raft_operations_WriteFile;
	dokan_op->FlushFileBuffers = raft_operations_FlushFileBuffers;
	dokan_op->GetFileInformation = raft_operations_GetFileInformation;
	dokan_op->FindFiles = raft_operations_FindFiles;
	dokan_op->FindFilesWithPattern = NULL;
	dokan_op->SetFileAttributes = raft_operations_SetFileAttributes;
	dokan_op->SetFileTime = raft_operations_SetFileTime;
	dokan_op->DeleteFile = raft_operations_DeleteFile;
	dokan_op->DeleteDirectory = raft_operations_DeleteDirectory;
	dokan_op->MoveFile = raft_operations_MoveFile;
	dokan_op->SetEndOfFile = raft_operations_SetEndOfFile;
	dokan_op->SetAllocationSize = raft_operations_SetAllocationSize;
	dokan_op->LockFile = raft_operations_LockFile;
	dokan_op->UnlockFile = raft_operations_UnlockFile;
	dokan_op->GetFileSecurity = raft_operations_GetFileSecurity;
	dokan_op->SetFileSecurity = raft_operations_SetFileSecurity;
	dokan_op->GetDiskFreeSpace = raft_operations_DokanGetDiskFreeSpace;
	dokan_op->GetVolumeInformation = raft_operations_GetVolumeInformation;
	dokan_op->Unmounted = raft_operations_Unmounted;
	dokan_op->FindStreams = raft_operations_FindStreams;
	dokan_op->Mounted = raft_operations_Mounted;
}