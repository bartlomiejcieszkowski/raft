using DokanNet;
using Gafa.Patterns;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Gafa.Dokan
{
	public partial class GitFilesystem : IFileSystem, IDokanOperations
	{


		public NtStatus Mounted(DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus Unmounted(DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
		{
			fileInfo = new FileInformation();
			fileInfo.FileName = fileName;

			if (FilesystemStringOperations.IsRoot(fileName))
			{

				fileInfo.Attributes = FileAttributes.Directory;
				fileInfo.LastAccessTime = m_FilesystemInformation.m_OpenTime;
				fileInfo.LastWriteTime = m_FilesystemInformation.m_OpenTime;
				fileInfo.CreationTime = m_FilesystemInformation.m_OpenTime;
				return DokanResult.Success;
			}

			return DokanResult.Success;
		}


		public void Cleanup(string fileName, DokanFileInfo info)
		{
		}

		public void CloseFile(string fileName, DokanFileInfo info)
		{
		}

		public NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
		{
			if (info.IsDirectory && mode == System.IO.FileMode.CreateNew)
			{
				return DokanResult.AccessDenied;
			}
			return DokanResult.Success;
		}

		public NtStatus DeleteDirectory(string fileName, DokanFileInfo info)
		{
			return DokanResult.Error;
		}

		public NtStatus DeleteFile(string fileName, DokanFileInfo info)
		{
			return DokanResult.Error;
		}

		public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
		{

			files = new List<FileInformation>();
			var filePath = fileName.Split('\\').ToQueue();

			var repo = new Repository(m_RepositoryPath);
			return m_RootHandler.FindFiles(ref filePath, ref files, info, repo);
		}

		public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, DokanFileInfo info)
		{
			streams = new FileInformation[0];
			return DokanResult.NotImplemented;
		}

		public NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
		{
			return DokanResult.Error;
		}

		public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, DokanFileInfo info)
		{
			totalNumberOfBytes = 1024 * 1024 * 1024;
			freeBytesAvailable = totalNumberOfBytes / 2;
			totalNumberOfFreeBytes = freeBytesAvailable;
			return DokanResult.Success;
		}



		public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
		{
			security = null;
			return DokanResult.Error;
		}

		public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info)
		{
			volumeLabel = m_FilesystemInformation.m_VolumeLabel;
			features = FileSystemFeatures.None;
			fileSystemName = String.Empty;
			return DokanResult.Success;
		}

		public NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
		{
			return DokanResult.Error;
		}

		public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
		{
			bytesRead = 0;
			return DokanResult.Error;
		}

		public NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
		{
			return DokanResult.Error;
		}

		public NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
		{
			return DokanResult.Error;
		}

		public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
		{
			return DokanResult.Error;
		}

		public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
		{
			return DokanResult.Error;
		}

		public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
		{
			return DokanResult.Error;
		}

		public NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
		{
			return DokanResult.Success;
		}

		public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
		{
			bytesWritten = 0;
			return DokanResult.Error;
		}
	}

}
