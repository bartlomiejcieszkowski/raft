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
		void Cleanup(ref Queue<string> filePath, DokanFileInfo info, params object[] list);
		void CloseFile(ref Queue<string> filePath, DokanFileInfo info, params object[] list);
		NtStatus CreateFile(ref Queue<string> filePath, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info, params object[] list);
		NtStatus DeleteDirectory(ref Queue<string> filePath, DokanFileInfo info, params object[] list);
		NtStatus DeleteFile(ref Queue<string> filePath, DokanFileInfo info, params object[] list);
		NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, params object[] list);
		NtStatus FindStreams(ref Queue<string> filePath, ref IList<FileInformation> streams, DokanFileInfo info, params object[] list);
		NtStatus FlushFileBuffers(ref Queue<string> filePath, DokanFileInfo info, params object[] list);
		NtStatus GetDiskFreeSpace(ref long freeBytesAvailable, ref long totalNumberOfBytes, ref long totalNumberOfFreeBytes, DokanFileInfo info, params object[] list);
		NtStatus GetFileInformation(ref Queue<string> filePath, ref FileInformation fileInfo, DokanFileInfo info, params object[] list);
		NtStatus GetFileSecurity(ref Queue<string> filePath, ref FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info, params object[] list);
		NtStatus GetVolumeInformation(ref string volumeLabel, ref FileSystemFeatures features, ref string fileSystemName, DokanFileInfo info, params object[] list);
		NtStatus LockFile(ref Queue<string> filePath, long offset, long length, DokanFileInfo info, params object[] list);
		NtStatus Mounted(DokanFileInfo info, params object[] list);
		NtStatus MoveFile(ref Queue<string> oldFilePath, ref Queue<string> newFilePath, bool replace, DokanFileInfo info, params object[] list);
		NtStatus ReadFile(ref Queue<string> filePath, byte[] buffer, ref int bytesRead, long offset, DokanFileInfo info, params object[] list);
		NtStatus SetAllocationSize(ref Queue<string> filePath, long length, DokanFileInfo info, params object[] list);
		NtStatus SetEndOfFile(ref Queue<string> filePath, long length, DokanFileInfo info, params object[] list);
		NtStatus SetFileAttributes(ref Queue<string> filePath, FileAttributes attributes, DokanFileInfo info, params object[] list);
		NtStatus SetFileSecurity(ref Queue<string> filePath, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info, params object[] list);
		NtStatus SetFileTime(ref Queue<string> filePath, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info, params object[] list);
		NtStatus UnlockFile(ref Queue<string> filePath, long offset, long length, DokanFileInfo info, params object[] list);
		NtStatus Unmounted(DokanFileInfo info, params object[] list);
		NtStatus WriteFile(ref Queue<string> filePath, byte[] buffer, ref int bytesWritten, long offset, DokanFileInfo info, params object[] list);
	}
}
