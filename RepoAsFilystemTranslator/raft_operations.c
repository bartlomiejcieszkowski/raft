#include "raft_operations.h"

#define LOG_NAME "raft_operations.c"
#include "raft_log.h"

static NTSTATUS DOKAN_CALLBACK raft_operations_CreateFile(LPCWSTR FileName, PDOKAN_IO_SECURITY_CONTEXT SecurityContext,
	ACCESS_MASK DesiredAccess, ULONG FileAttributes,
	ULONG ShareAccess, ULONG CreateDisposition,
	ULONG CreateOptions, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static void DOKAN_CALLBACK raft_operations_Cleanup(LPCWSTR FileName,
	PDOKAN_FILE_INFO DokanFileInfo)
{
}

static void DOKAN_CALLBACK raft_operations_CloseFile(LPCWSTR FileName,
	PDOKAN_FILE_INFO DokanFileInfo)
{
}

static NTSTATUS DOKAN_CALLBACK raft_operations_ReadFile(LPCWSTR FileName, LPVOID Buffer,
	DWORD BufferLength,
	LPDWORD ReadLength,
	LONGLONG Offset,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_WriteFile(LPCWSTR FileName, LPCVOID Buffer,
	DWORD NumberOfBytesToWrite,
	LPDWORD NumberOfBytesWritten,
	LONGLONG Offset,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_FlushFileBuffers(LPCWSTR FileName, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_GetFileInformation(
	LPCWSTR FileName, LPBY_HANDLE_FILE_INFORMATION HandleFileInformation,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_FindFiles(LPCWSTR FileName,
	PFillFindData FillFindData,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_SetFileAttributes(
	LPCWSTR FileName, DWORD FileAttributes, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_SetFileTime(LPCWSTR FileName, CONST FILETIME* CreationTime,
	CONST FILETIME* LastAccessTime, CONST FILETIME* LastWriteTime,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_DeleteFile(LPCWSTR FileName, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_DeleteDirectory(LPCWSTR FileName, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_MoveFile(LPCWSTR FileName, // existing file name
	LPCWSTR NewFileName, BOOL ReplaceIfExisting,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_SetEndOfFile(
	LPCWSTR FileName, LONGLONG ByteOffset, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_SetAllocationSize(
	LPCWSTR FileName, LONGLONG AllocSize, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_LockFile(LPCWSTR FileName,
	LONGLONG ByteOffset,
	LONGLONG Length,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_UnlockFile(LPCWSTR FileName, LONGLONG ByteOffset, LONGLONG Length,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_GetFileSecurity(
	LPCWSTR FileName, PSECURITY_INFORMATION SecurityInformation,
	PSECURITY_DESCRIPTOR SecurityDescriptor, ULONG BufferLength,
	PULONG LengthNeeded, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_SetFileSecurity(
	LPCWSTR FileName, PSECURITY_INFORMATION SecurityInformation,
	PSECURITY_DESCRIPTOR SecurityDescriptor, ULONG SecurityDescriptorLength,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_DokanGetDiskFreeSpace(
	PULONGLONG FreeBytesAvailable, PULONGLONG TotalNumberOfBytes,
	PULONGLONG TotalNumberOfFreeBytes, PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_GetVolumeInformation(
	LPWSTR VolumeNameBuffer, DWORD VolumeNameSize, LPDWORD VolumeSerialNumber,
	LPDWORD MaximumComponentLength, LPDWORD FileSystemFlags,
	LPWSTR FileSystemNameBuffer, DWORD FileSystemNameSize,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_Unmounted(PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK
raft_operations_FindStreams(LPCWSTR FileName, PFillFindStreamData FillFindStreamData,
	PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

	return status;
}

static NTSTATUS DOKAN_CALLBACK raft_operations_Mounted(PDOKAN_FILE_INFO DokanFileInfo)
{
	NTSTATUS status = STATUS_SUCCESS;

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