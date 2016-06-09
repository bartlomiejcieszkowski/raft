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
			m_RootHandler = new RootHandler("", m_RepositoryPath,
			new List<SubFolderHandler>()
			{
				new BranchesHandler("branches", new List<SubFolderHandler>()
				{
					new BranchHandler("*", new List<SubFolderHandler>() {
						new TreeHandler("*")
					})
				}),
				new TagHandler("tags", new List<SubFolderHandler>() {
					new TreeHandler("*")
				})
			});
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

	public abstract class SubFolderHandler
	{
		protected string m_Subfolder;

		protected virtual bool IsSubfolder(ref string subfolder) { return m_Subfolder.Equals(subfolder); }

		protected IDictionary<string, SubFolderHandler> m_Handlers;

		protected SubFolderHandler(string Subfolder, IEnumerable<SubFolderHandler> Handlers)
		{
			m_Subfolder = Subfolder;
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
				if (entry.Value == this) continue;
				if (entry.Value.m_Subfolder == "*") continue;

				files.Add(entry.Value.GetFileInformation());
			}
		}

		public abstract NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null);
	}

	public class TreeHandler : SubFolderHandler
	{
		public TreeHandler(string Subfolder) : base(Subfolder, new List<SubFolderHandler>()) { }
		public TreeHandler(string Subfolder, IEnumerable<SubFolderHandler> Handlers) : base(Subfolder, Handlers) { }

		protected bool IsSubfolder(ref Repository repository, ref string subfolder)
		{
			string lSubfolder = subfolder;
			return repository.Branches.Single(b => b.FriendlyName.Equals(lSubfolder)) != null;
		}

		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null)
		{
			if (!(UnkObject is Tree))
			{
				return NtStatus.Error;
			}

			if (!(UnkObject2 is Commit))
			{
				return NtStatus.Error;
			}

			var tree = UnkObject as Tree;
			var commit = UnkObject2 as Commit;

			while(filePath.Count != 0)
			{
				var currentPath = filePath.Dequeue();
				var treeEntry = tree.Single(te => te.TargetType == TreeEntryTargetType.Tree && te.Name.Equals(currentPath));
				if(null == treeEntry)
				{
					return NtStatus.Error;
				}

				if(null == treeEntry.Target)
				{
					return NtStatus.Error;
				}

				tree = treeEntry.Target as Tree;
			}

			foreach (var treeEntry in tree)
			{
				FileInformation treeEntryInformation = new FileInformation()
				{
					FileName = treeEntry.Name,
					Attributes = treeEntry.Mode.Convert(),
					CreationTime = commit.Committer.When.UtcDateTime,
					LastWriteTime = commit.Committer.When.UtcDateTime,
					LastAccessTime = commit.Committer.When.UtcDateTime,
				};
				files.Add(treeEntryInformation);
			}

			return NtStatus.Success;
		}
	}

	public class BranchHandler : SubFolderHandler
	{
		public BranchHandler(string Subfolder) : base(Subfolder, new List<SubFolderHandler>()) { }
		public BranchHandler(string Subfolder, IEnumerable<SubFolderHandler> Handlers) : base(Subfolder, Handlers) { }

		protected bool IsSubfolder(ref Repository repository, ref string subfolder)
		{
			string lSubfolder = subfolder;
			return repository.Branches.Single(b => b.FriendlyName.Equals(lSubfolder)) != null;
		}

		

		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null)
		{
			if (!(UnkObject is Repository))
			{
				return NtStatus.Error;
			}

			var repository = UnkObject as Repository;

			var currentPath = filePath.Dequeue();
			if (!IsSubfolder(ref repository, ref currentPath))
			{
				return NtStatus.Error;
			}

			var currentBranch = repository.Branches.Single(b => b.FriendlyName.Equals(currentPath));

			
			SubFolderHandler handler;
			//if (!m_Handlers.TryGetValue(filePath.Peek(), out handler))
			//{
			if (!m_Handlers.TryGetValue("*", out handler))
			{
				return NtStatus.Error;
			}
			//}

			return handler.FindFiles(ref filePath, ref files, info, currentBranch.Tip.Tree, currentBranch.Tip);
		}
	}

	public class BranchesHandler : SubFolderHandler
	{
		public BranchesHandler(string Subfolder) : base(Subfolder, new List<SubFolderHandler>()) { }
		public BranchesHandler(string Subfolder, IEnumerable<SubFolderHandler> Handlers) : base(Subfolder, Handlers) { }

		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null)
		{
			if(!(UnkObject is Repository))
			{
				return NtStatus.Error;
			}

			var repository = UnkObject as Repository;

			var currentPath = filePath.Dequeue();
			if (!IsSubfolder(ref currentPath))
			{
				return NtStatus.Error;
			}

			if (filePath.Count != 0)
			{
				// we need to go deeper

				SubFolderHandler handler;
				if (!m_Handlers.TryGetValue(filePath.Peek(), out handler))
				{
					if (!m_Handlers.TryGetValue("*", out handler))
					{
						return NtStatus.Error;
					}
				}

				return handler.FindFiles(ref filePath, ref files, info, UnkObject);
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
		public TagHandler(string Subfolder, IEnumerable<SubFolderHandler> Handlers) : base(Subfolder, Handlers) { }

		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null)
		{
			if (!(UnkObject is Repository))
			{
				return NtStatus.Error;
			}

			var repository = UnkObject as Repository;

			var currentPath = filePath.Dequeue();
			if (!IsSubfolder(ref currentPath))
			{
				return NtStatus.Error;
			}

			if (filePath.Count != 0)
			{
				var nextPath = filePath.Dequeue();
				var currentTag = repository.Tags.Single(b => b.FriendlyName.Equals(nextPath));
				// we need to go deeper

				if(null == currentTag)
				{
					return NtStatus.Error;
				}

				SubFolderHandler handler;
				//if (!m_Handlers.TryGetValue(filePath.Peek(), out handler))
				//{
				if (!m_Handlers.TryGetValue("*", out handler))
				{
					return NtStatus.Error;
				}
				//}
				var currentCommit = currentTag.PeeledTarget as Commit;
				return handler.FindFiles(ref filePath, ref files, info, currentCommit.Tree, currentCommit);
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
		public RootHandler(string Subfolder, string RepositoryPath, IEnumerable<SubFolderHandler> Handlers) : base(
			Subfolder, Handlers)
		{
			m_RepositoryPath = RepositoryPath;
			m_Handlers.Add(m_Subfolder, this);
		}


		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null)
		{
			var currentPath = filePath.Dequeue();
			if (!IsSubfolder(ref currentPath))
			{
				return NtStatus.Error;
			}


			if (filePath.Count != 0)
			{
				SubFolderHandler handler;
				if (!m_Handlers.TryGetValue(filePath.Peek(), out handler))
				{
					if (!m_Handlers.TryGetValue("*", out handler))
					{
						return NtStatus.Error;
					}
				}

				return handler.FindFiles(ref filePath, ref files, info, UnkObject, UnkObject2);
			}

			AddHandlersAsSubfolders(ref files);
			return DokanResult.Success;
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
