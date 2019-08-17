using DokanNet;
using Gafa.FileSystem;
using Gafa.Logging;
using Gafa.Patterns;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

namespace Gafa.Dokan
{

	public static class GitFilesystemExtensions
	{
		
	}

	public partial class GitFilesystem : Logger, IFileSystem, IDokanOperations
	{
		private Tuple<PathType, string> ParsePath(string fileName, params object[] args)
		{
			var firstSeparator = fileName.IndexOf(Path.DirectorySeparatorChar);
			if (-1 == firstSeparator)
			{
				return null;
			}

			char[] array = { Path.DirectorySeparatorChar };
			var splittedPath = fileName.Split(array, 3);


			if (2 > splittedPath.Length)
			{
				Log.Error("Invalid path: {0}.", fileName);
				return null;
			}

			var pathType = splittedPath[1].ToPathType();
			if (PathType.unknown == pathType)
			{
				Log.Error("Unknown subfolder: {0}.", splittedPath[1]);
				return null;
			}

			return new Tuple<PathType, string>(pathType, splittedPath.Length > 2 ? splittedPath[2] : null);
		}

		public NtStatus Mounted(DokanFileInfo info)
		{
			Log.Log(Default, LogEnter);
			Log.Log(Default, LogExit);
			return DokanResult.Success;
		}

		public NtStatus Unmounted(DokanFileInfo info)
		{

			return DokanResult.Success;
		}

		public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			info.TryResetTimeout(1000 * 60 * 5);
			var tuple = ParsePath(fileName);
			fileInfo = new FileInformation();

			return m_Handler.GetFileInformation(tuple.Item2, ref fileInfo, info, m_Repository);
		}



		public void Cleanup(string fileName, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
		}

		public void CloseFile(string fileName, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
		}

		public NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1} Access: {2} Share: {3} Mode: {4} Options: {5} Attributes: {6}", fileName, info.ToStringDokanFileInfo(), access.ToString(), share.ToString(), mode.ToString(), options.ToString(), attributes.ToString());
			if (info.IsDirectory && mode == FileMode.CreateNew)
			{
				return DokanResult.AccessDenied;
			}
			return DokanResult.Success;
		}

		public NtStatus DeleteDirectory(string fileName, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			return DokanResult.Error;
		}

		public NtStatus DeleteFile(string fileName, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			return DokanResult.Error;
		}

		public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			info.TryResetTimeout(1000 * 60 * 5);
			files = new List<FileInformation>();
			var tuple = ParsePath(fileName);

			return m_Handler.FindFiles(tuple.Item2, ref files, info, m_Repository);
		}

		public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			streams = new List<FileInformation>();
			var tuple = ParsePath(fileName);

			return m_Handler.FindStreams(tuple.Item2, ref streams, info, m_Repository);
		}

		public NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			var tuple = ParsePath(fileName);
			return m_Handler.FlushFileBuffers(tuple.Item2, info, m_Repository);
		}

		public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, DokanFileInfo info)
		{
			//Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			freeBytesAvailable = 0;
			totalNumberOfBytes = 0;
			totalNumberOfFreeBytes = 0;

			return m_Handler.GetDiskFreeSpace(ref freeBytesAvailable, ref totalNumberOfBytes, ref totalNumberOfFreeBytes, info, m_Repository);

			//totalNumberOfBytes = 1024 * 1024 * 1024;
			//freeBytesAvailable = totalNumberOfBytes / 2;
			//totalNumberOfFreeBytes = freeBytesAvailable;
			//return DokanResult.Success;
		}



		public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			security = null;
			var tuple = ParsePath(fileName);
			return m_Handler.GetFileSecurity(tuple.Item2, ref security, sections, info, m_Repository);
		}

		public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info)
		{
			//Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			volumeLabel = m_FilesystemInformation.m_VolumeLabel;
			features = FileSystemFeatures.None;
			fileSystemName = String.Empty;
			return DokanResult.Success;
			//m_Handler.GetVolumeInformation(ref volumeLabel, ref features, ref fileSystemName, info, m_Repository);
		}

		public NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			var tuple = ParsePath(fileName);
			return m_Handler.LockFile(tuple.Item2, offset, length, info, m_Repository);
		}

		public NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
		{
			Log.Log(Default, "FileNameOld: {0} FileNameNew: {1} DokanInfo: {2}", oldName, newName, info.ToStringDokanFileInfo());
			var oldFilePath = oldName.PathToQueue();
			var newFilePath = newName.PathToQueue();
			return m_Handler.MoveFile(ref oldFilePath, ref newFilePath, replace, info, m_Repository);
		}

		public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			info.TryResetTimeout(1000 * 60 * 5);
			var tuple = ParsePath(fileName);
			bytesRead = 0;
			return m_Handler.ReadFile(tuple.Item2, buffer, ref bytesRead, offset, info, m_Repository);

		}

		public NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			var tuple = ParsePath(fileName);
			return m_Handler.SetAllocationSize(tuple.Item2, length, info, m_Repository);
		}

		public NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			var tuple = ParsePath(fileName);
			return m_Handler.SetEndOfFile(tuple.Item2, length, info, m_Repository);
		}

		public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			var tuple = ParsePath(fileName);
			return m_Handler.SetFileAttributes(tuple.Item2, attributes, info, m_Repository);
		}

		public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			var tuple = ParsePath(fileName);
			return m_Handler.SetFileSecurity(tuple.Item2, security, sections, info, m_Repository);
		}

		public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			var tuple = ParsePath(fileName);
			return m_Handler.SetFileTime(tuple.Item2, creationTime, lastAccessTime, lastWriteTime, info, m_Repository);
		}

		public NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			var tuple = ParsePath(fileName);
			return m_Handler.UnlockFile(tuple.Item2, offset, length, info, m_Repository);
		}

		public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
		{
			Log.Log(Default, "FileName: {0} DokanInfo: {1}", fileName, info.ToStringDokanFileInfo());
			var tuple = ParsePath(fileName);
			bytesWritten = 0;
			return m_Handler.WriteFile(tuple.Item2, buffer, ref bytesWritten, offset, info, m_Repository);
		}
	}

}
