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
		void Cleanup(string fileName, DokanFileInfo info, params object[] list);
		void CloseFile(string fileName, DokanFileInfo info, params object[] list);
		NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info, params object[] list);
		NtStatus DeleteDirectory(string fileName, DokanFileInfo info, params object[] list);
		NtStatus DeleteFile(string fileName, DokanFileInfo info, params object[] list);
		NtStatus FindFiles(string fileName, ref IList<FileInformation> files, DokanFileInfo info, params object[] list);
		NtStatus FindStreams(string fileName, ref IList<FileInformation> streams, DokanFileInfo info, params object[] list);
		NtStatus FlushFileBuffers(string fileName, DokanFileInfo info, params object[] list);
		NtStatus GetDiskFreeSpace(ref long freeBytesAvailable, ref long totalNumberOfBytes, ref long totalNumberOfFreeBytes, DokanFileInfo info, params object[] list);
		NtStatus GetFileInformation(string fileName, ref FileInformation fileInfo, DokanFileInfo info, params object[] list);
		NtStatus GetFileSecurity(string fileName, ref FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info, params object[] list);
		NtStatus GetVolumeInformation(ref string volumeLabel, ref FileSystemFeatures features, ref string fileSystemName, DokanFileInfo info, params object[] list);
		NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info, params object[] list);
		NtStatus Mounted(DokanFileInfo info, params object[] list);
		NtStatus MoveFile(ref Queue<string> oldFilePath, ref Queue<string> newFilePath, bool replace, DokanFileInfo info, params object[] list);
		NtStatus ReadFile(string fileName, byte[] buffer, ref int bytesRead, long offset, DokanFileInfo info, params object[] list);
		NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info, params object[] list);
		NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info, params object[] list);
		NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info, params object[] list);
		NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info, params object[] list);
		NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info, params object[] list);
		NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info, params object[] list);
		NtStatus Unmounted(DokanFileInfo info, params object[] list);
		NtStatus WriteFile(string fileName, byte[] buffer, ref int bytesWritten, long offset, DokanFileInfo info, params object[] list);
	}
}
