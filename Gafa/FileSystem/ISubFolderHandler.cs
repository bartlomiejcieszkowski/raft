using DokanNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Gafa.FileSystem
{
	public interface ISubFolderHandler
	{
		NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null);
		//NtStatus ReadFile(ref Queue<string> filePath, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null);

		//void Cleanup(ref Queue<string> filePath, DokanFileInfo info);
		//void CloseFile(ref Queue<string> filePath, DokanFileInfo info);
		//NtStatus CreateFile(ref Queue<string> filePath, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info);
		//NtStatus DeleteDirectory(ref Queue<string> filePath, DokanFileInfo info);
		//NtStatus DeleteFile(ref Queue<string> filePath, DokanFileInfo info);
		//NtStatus FindFiles(ref Queue<string> filePath, out IList<FileInformation> files, DokanFileInfo info);
		//NtStatus FindStreams(ref Queue<string> filePath, out IList<FileInformation> streams, DokanFileInfo info);
		//NtStatus FlushFileBuffers(ref Queue<string> filePath, DokanFileInfo info);
		//NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, DokanFileInfo info);
		//NtStatus GetFileInformation(ref Queue<string> filePath, out FileInformation fileInfo, DokanFileInfo info);
		//NtStatus GetFileSecurity(ref Queue<string> filePath, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info);
		//NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info);
		//NtStatus LockFile(ref Queue<string> filePath, long offset, long length, DokanFileInfo info);
		//NtStatus Mounted(DokanFileInfo info);
		//NtStatus MoveFile(ref Queue<string> oldFilePath, ref Queue<string> newFilePath, bool replace, DokanFileInfo info);
		
		//NtStatus SetAllocationSize(ref Queue<string> filePath, long length, DokanFileInfo info);
		//NtStatus SetEndOfFile(ref Queue<string> filePath, long length, DokanFileInfo info);
		//NtStatus SetFileAttributes(ref Queue<string> filePath, FileAttributes attributes, DokanFileInfo info);
		//NtStatus SetFileSecurity(ref Queue<string> filePath, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info);
		//NtStatus SetFileTime(ref Queue<string> filePath, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info);
		//NtStatus UnlockFile(ref Queue<string> filePath, long offset, long length, DokanFileInfo info);
		//NtStatus Unmounted(DokanFileInfo info);
		//NtStatus WriteFile(ref Queue<string> filePath, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info);
	}
}
