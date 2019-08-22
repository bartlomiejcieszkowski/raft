#include "raft_operations.h"

#include <git2.h>

#define LOG_NAME "raft_operations.c"
#include "raft_log.h"

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

#define PATH_SEPARATOR L'\\'

bitmap_path_s get_offsets(wchar_t character, LPCWSTR wstring, int* count)
{
	bitmap_path_s retVal = { 0 };
	int offset = 0;
	if (count) { *count = 0; }
	while (wstring[offset] != L'\0')
	{
		if (character == wstring[offset])
		{
			retVal.bitmap[offset / (sizeof(int) * 8)] |= offset % (sizeof(int) * 8);
			if (count) { *count += 1; }
		}
		++offset;
	}
	return retVal;
}

int get_next_occurence(int offset, bitmap_path_s* bitmap_path)
{
	if (NULL == bitmap_path) { return -2; }

	if (offset < 0)
	{
		offset = 0;
	}
	else
	{
		++offset;
	}

	while (offset < DOKAN_MAX_PATH)
	{
		int result = offset / (sizeof(int) * 8);
		int modulo = offset % (sizeof(int) * 8);

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
		int a = ~(val | (val - 1));
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
		offset = (result+1) * (sizeof(int) * 8);
	}
	return -1;
}

static NTSTATUS raft_operations_find_files(LPCWSTR FileName, PFillFindData FillFindData, PDOKAN_FILE_INFO DokanFileInfo, int offset)
{

}

static int wsubstringcmp(LPCWSTR wstring, LPCWSTR substring, int start_offset)
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

static NTSTATUS DOKAN_CALLBACK
raft_operations_FindFiles(LPCWSTR FileName,
	PFillFindData FillFindData,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;
	raft_context_s* this_ = (raft_context_s*)DokanFileInfo->DokanOptions->GlobalContext;
	WIN32_FIND_DATAW find_data;

	LOG_DEBUG("FindFiles: %ls", FileName);

	if (this_->repository == NULL)
	{
		return STATUS_INVALID_PARAMETER;
	}

	int count;
	bitmap_path_s separators_bitmap = get_offsets(PATH_SEPARATOR, FileName, &count);

	int offset = get_next_occurence(-1, &separators_bitmap);

	if (1 == count)
	{
		if (offset != 0)
		{
			LOG_ERROR("How did we even get here?");
		}
		else
		{
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
		int next_offset = get_next_occurence(offset, &separators_bitmap);

		int delta = next_offset - offset - 1;

		switch (delta)
		{
		case 7: /* remotes */
			if (wsubstringcmp(FileName, path_remotes, offset + 1) == 0)
			{

			}
			break;
		case 6: /* branch*/
			if (wsubstringcmp(FileName, path_branch, offset + 1) == 0)
			{

			}
			break;
		case 4: /* tags */
			if (wsubstringcmp(FileName, path_tags, offset + 1) == 0)
			{

			}
			break;
		}


	}


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