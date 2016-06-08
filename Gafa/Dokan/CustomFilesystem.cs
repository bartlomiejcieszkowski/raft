using DokanNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using LibGit2Sharp;
using Gafa.Dokan;

namespace Gafa.Dokan
{
	public class FolderEntry
	{
		public string Mountpoint;
		public string Folderpath;
	}

	public class Singleton<U> where U : class, new()
	{
		private static U _instance;

		public static U Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (safetyLock)
					{
						if (_instance == null)
						{
							_instance = new U();
						}
					}
				}
				return _instance;
			}
		}

		protected Singleton() { }

		private static object safetyLock = new object();
	}

	public class MountsList : Singleton<List<IFileSystem>>
	{
		private MountsList() : base() { }

		public static void Mount()
		{
			foreach(var entry in Instance)
			{
				entry.Mount();
			}
		}

		public static void Unmount()
		{
			foreach(var entry in Instance)
			{
				entry.Unmount();
			}
		}
	}

	public interface IFileSystem
	{
		void Mount();
		void Unmount();
	}

	public class FilesystemStringOperations
	{
		public static bool IsRoot(string fileName)
		{
			return fileName == @"\";
		}
	}

	public class FilesystemInformation
	{
		public DateTime OpenTime;
		public string VolumeLabel;
		public string Mountpoint;
	}

	public partial class GitFilesystem : IFileSystem, IDokanOperations
	{
		private RootHandler           m_RootHandler;
		private FilesystemInformation m_FilesystemInformation;
		private string                m_RepositoryPath;

		public GitFilesystem(string mountPoint, string repositoryPath)
		{
			m_FilesystemInformation = new FilesystemInformation()
			{
				VolumeLabel = Path.GetDirectoryName(repositoryPath),
				Mountpoint = mountPoint,
				OpenTime = DateTime.UtcNow
			};
			m_RepositoryPath = repositoryPath;
			m_RootHandler = new RootHandler("", m_RepositoryPath);
		}

		public void Mount()
		{
			new Thread(() =>
			{
				DokanNet.Dokan.Mount(this, m_FilesystemInformation.Mountpoint);
			}).Start();
		}

		public void Unmount()
		{
			new Thread(() =>
			{
				DokanNet.Dokan.RemoveMountPoint(m_FilesystemInformation.Mountpoint);
			}).Start();
		}
	}

	public static class Git2FilesystemExtensions
	{
		public static FileAttributes Convert(this Mode mode)
		{
			switch (mode)
			{
				case Mode.Directory:
					return FileAttributes.Directory;
				case Mode.ExecutableFile:
				case Mode.NonExecutableFile:
					return FileAttributes.Normal;
				default:
					return FileAttributes.Temporary;
			}
		}
	}

	public static class StucturesExtensions
	{
		public static Stack<T> ToStack<T>(this IEnumerable<T> enumerable)
		{
			return new Stack<T>(enumerable);
		}

		public static Queue<T> ToQueue<T>(this IEnumerable<T> enumerable)
		{
			return new Queue<T>(enumerable);
		}

	}

	public interface ISubFolderHandler
	{
		//bool Handles(string currPath);


		

		//NtStatus FindFiles(Array )
	}

	public abstract class SubFolderHandler
	{
		private string m_Subfolder;

		protected bool IsSubfolder(ref string subfolder) { return m_Subfolder.Equals(subfolder); }

		protected IDictionary<string, SubFolderHandler> m_Handlers;

		protected SubFolderHandler(string Subfolder, IEnumerable<SubFolderHandler> Handlers)
		{
			m_Handlers = Handlers.ToDictionary(p => p.m_Subfolder);
		}

		public FileInformation GetFileInformation()
		{
			return new FileInformation()
			{
				Attributes     = FileAttributes.Directory,
				CreationTime   = DateTime.UtcNow,
				LastWriteTime  = DateTime.UtcNow,
				LastAccessTime = DateTime.UtcNow,
				FileName       = m_Subfolder
			};
		}

		protected void AddHandlersAsSubfolders(ref IList<FileInformation> files)
		{
			foreach(var entry in m_Handlers)
			{
				files.Add(entry.Value.GetFileInformation());
			}
		}

		public abstract NtStatus FindFiles(ref Repository repository, ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info);
	}

	public class BranchHandler : SubFolderHandler
	{
		public BranchHandler(string Subfolder) : base(Subfolder, new List<SubFolderHandler>()) { }

		public override NtStatus FindFiles(ref Repository repository, ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info)
		{
			var currentPath = filePath.Dequeue();
			if (!IsSubfolder(ref currentPath))
			{
				return NtStatus.Error;
			}

			if (filePath.Count != 0)
			{
				// we need to go deeper

				var handler = m_Handlers[filePath.Peek()];
				if(null == handler)
				{
					return NtStatus.Error;
				}

				return handler.FindFiles(ref repository, ref filePath, ref files, info);
			}

			AddHandlersAsSubfolders(ref files);

			foreach (var branch in repository.Branches.Where(b => !b.IsRemote))
			{
				FileInformation branchInformation = new FileInformation()
				{
					Attributes = FileAttributes.Directory,
					LastWriteTime = branch.Tip.Committer.When.UtcDateTime,
					LastAccessTime = branch.Tip.Committer.When.UtcDateTime,
					CreationTime = branch.Tip.Committer.When.UtcDateTime,
					FileName = branch.FriendlyName
				};
				files.Add(branchInformation);
			}

			return DokanResult.Success;

		}
	}

	public class TagHandler : SubFolderHandler
	{
		public TagHandler(string Subfolder) : base(Subfolder, new List<SubFolderHandler>()) { }

		public override NtStatus FindFiles(ref Repository repository, ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info)
		{
			var currentPath = filePath.Dequeue();
			if (!IsSubfolder(ref currentPath))
			{
				return NtStatus.Error;
			}

			if (filePath.Count != 0)
			{
				// we need to go deeper

				var handler = m_Handlers[filePath.Peek()];
				if (null == handler)
				{
					return NtStatus.Error;
				}

				return handler.FindFiles(ref repository, ref filePath, ref files, info);
			}

			AddHandlersAsSubfolders(ref files);

			foreach (var tag in repository.Tags)
			{
				if (tag.PeeledTarget is Commit)
				{
					var taggedCommit = tag.PeeledTarget as Commit;

					FileInformation tagInformation = new FileInformation()
					{
						Attributes = FileAttributes.Directory,
						LastWriteTime = taggedCommit.Committer.When.UtcDateTime,
						LastAccessTime = taggedCommit.Committer.When.UtcDateTime,
						CreationTime = taggedCommit.Committer.When.UtcDateTime,
						FileName = tag.FriendlyName
					};
					files.Add(tagInformation);
				}
			}

			return DokanResult.Success;

		}
	}

	public class RootHandler : SubFolderHandler
	{
		private string m_RepositoryPath;
		public RootHandler(string Subfolder, string RepositoryPath) : base(
			Subfolder,
			new List<SubFolderHandler>()
			{
				new BranchHandler("branches"),
				new TagHandler("tags")
			})
		{
			m_RepositoryPath = RepositoryPath;
		}


		public override NtStatus FindFiles(ref Repository repository, ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info)
		{
			var currentPath = filePath.Dequeue();
			if (!IsSubfolder(ref currentPath))
			{
				return NtStatus.Error;
			}

			if (filePath.Count != 0)
			{
				var handler = m_Handlers[filePath.Peek()];
				if (null == handler)
				{
					return NtStatus.Error;
				}

				return handler.FindFiles(ref repository, ref filePath, ref files, info);
			}

			AddHandlersAsSubfolders(ref files);
			return DokanResult.Success;

			// this folder
			

			//	var splittedFilePath = fileName.Split('\\').ToStack();
			//	if ("" == splittedFilePath.Peek())
			//	{
			//		splittedFilePath.Pop();
			//	}

			//	splittedFilePath.Skip()
			//	var currentBranch = repo.Branches.SingleOrDefault(b => b.FriendlyName == splittedFilePath[1]);
			//	if (currentBranch == null)
			//	{
			//		return DokanResult.Error;
			//	}

			//	foreach (var treeEntry in currentBranch.Tip.Tree)
			//	{
			//		if (treeEntry.Mode == Mode.NonExecutableFile)
			//		{
			//			FileInformation treeEntryInformation = new FileInformation()
			//			{
			//				Attributes = treeEntry.Mode.Convert(),
			//				LastWriteTime = currentBranch.Tip.Committer.When.UtcDateTime,
			//				LastAccessTime = currentBranch.Tip.Committer.When.UtcDateTime,
			//				CreationTime = currentBranch.Tip.Committer.When.UtcDateTime,
			//				FileName = treeEntry.Name
			//			};
			//			files.Add(treeEntryInformation);
			//		}
			//	}
			//}

			//return DokanResult.Success;
			//return NtStatus.Success;
		}
	}

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
				fileInfo.LastAccessTime = m_FilesystemInformation.OpenTime;
				fileInfo.LastWriteTime = m_FilesystemInformation.OpenTime;
				fileInfo.CreationTime = m_FilesystemInformation.OpenTime;
				return DokanResult.Success;
			}

			//var fileNameWithPath = Path.Combine(Folderpath, fileName);
			//if (!File.Exists(fileNameWithPath))
			//{
			//	return DokanResult.Error;
			//}

			//var fileInfoI = new FileInfo(fileNameWithPath);

			//fileInfo.Attributes = fileInfoI.Attributes;
			//fileInfo.LastAccessTime = fileInfoI.LastAccessTimeUtc;
			//fileInfo.LastWriteTime = fileInfoI.LastWriteTimeUtc;
			//fileInfo.LastAccessTime = fileInfoI.LastAccessTimeUtc;
			//fileInfo.Length = fileInfoI.Length;
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
			filePath.Dequeue();

			var repo = new Repository(m_RepositoryPath);
			return m_RootHandler.FindFiles(ref repo, ref filePath, ref files, info);
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
			volumeLabel = m_FilesystemInformation.VolumeLabel;
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

		public string GetMountpoint() { return Mountpoint; }


		public CustomFilesystem(string mountpoint, string path) : base()
		{
			OpenTime = DateTime.UtcNow;
			Mountpoint = mountpoint;
			Folderpath = path;
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

			var fileNameWithPath =  Path.Combine(Folderpath, fileName);
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
			DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(Folderpath, fileName.TrimStart('\\')));
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
