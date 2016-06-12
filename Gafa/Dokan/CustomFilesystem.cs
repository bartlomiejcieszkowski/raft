using DokanNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using LibGit2Sharp;
using Gafa.Patterns;
using Gafa.Logging;
using Gafa.GitToFileSystem;
using Gafa.FileSystem;

namespace Gafa.Dokan
{
	public class FolderEntry
	{
		public string m_Mountpoint;
		public string m_Folderpath;
	}






	public class CustomFilesystem : FolderEntry, IFileSystem, IDokanOperations
	{
		DateTime OpenTime;
		string VolumeLabel = "RFS";

		public void Mount()
		{
			new Thread(() =>
			{
				DokanNet.Dokan.Mount(this, GetMountpoint());
			}).Start();
		}

		public void Unmount()
		{
			new Thread(() =>
			{
				DokanNet.Dokan.RemoveMountPoint(GetMountpoint());
			}).Start();
		}

		public string GetMountpoint() { return m_Mountpoint; }


		public CustomFilesystem(string mountpoint, string path) : base()
		{
			OpenTime = DateTime.UtcNow;
			m_Mountpoint = mountpoint;
			m_Folderpath = path;
		}

		public NtStatus Mounted(DokanFileInfo info)
		{
			OpenTime = DateTime.UtcNow;
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

			if(FilesystemStringOperations.IsRoot(fileName))
			{

				fileInfo.Attributes = FileAttributes.Directory;
				fileInfo.LastAccessTime = OpenTime;
				fileInfo.LastWriteTime = OpenTime;
				fileInfo.CreationTime = OpenTime;
				return DokanResult.Success;
			}

			var fileNameWithPath =  Path.Combine(m_Folderpath, fileName);
			if(!File.Exists(fileNameWithPath))
			{
				return DokanResult.Error;
			}

			var fileInfoI = new FileInfo(fileNameWithPath);

			fileInfo.Attributes = fileInfoI.Attributes;
			fileInfo.LastAccessTime = fileInfoI.LastAccessTimeUtc;
			fileInfo.LastWriteTime = fileInfoI.LastWriteTimeUtc;
			fileInfo.LastAccessTime = fileInfoI.LastAccessTimeUtc;
			fileInfo.Length = fileInfoI.Length;
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
			if(info.IsDirectory && mode == System.IO.FileMode.CreateNew)
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
			DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(m_Folderpath, fileName.TrimStart('\\')));
			//if(fileName == @"\")
			//{

			foreach (var directoryInfo in dirInfo.GetDirectories())
			{
				FileInformation fileInfoD = new FileInformation();
				fileInfoD.FileName = directoryInfo.Name;
				fileInfoD.Attributes = directoryInfo.Attributes;
				fileInfoD.LastAccessTime = directoryInfo.LastAccessTimeUtc;
				fileInfoD.LastWriteTime = directoryInfo.LastWriteTimeUtc;
				fileInfoD.CreationTime = directoryInfo.CreationTimeUtc;
				files.Add(fileInfoD);

			}

			foreach (var fileinfo in dirInfo.GetFiles())
			{
				FileInformation fileInfoD = new FileInformation();
				fileInfoD.FileName = fileinfo.Name;
				fileInfoD.Attributes = fileinfo.Attributes;
				fileInfoD.LastAccessTime = fileinfo.LastAccessTimeUtc;
				fileInfoD.LastWriteTime = fileinfo.LastWriteTimeUtc;
				fileInfoD.CreationTime = fileinfo.CreationTimeUtc;
				fileInfoD.Length = fileinfo.Length;
				files.Add(fileInfoD);
			}
			//}

			return DokanResult.Success;
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
			volumeLabel = VolumeLabel;
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
