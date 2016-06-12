using DokanNet;
using Gafa.GitToFileSystem;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gafa.FileSystem
{

	public class TreeHandler : SubFolderHandler
	{
		public TreeHandler(string Subfolder) : base(Subfolder) { }
		public TreeHandler(string Subfolder, SubFolderHandler Handler) : base(Subfolder, Handler) { }
		public TreeHandler(string Subfolder, IEnumerable<SubFolderHandler> Handlers) : base(Subfolder, Handlers) { }

		protected bool IsSubfolder(ref Repository repository, ref string subfolder)
		{
			string lSubfolder = subfolder;
			return repository.Branches.Single(b => b.FriendlyName.Equals(lSubfolder)) != null;
		}

		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null)
		{
			Log.Log(Default, LogEnter);
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

			while (filePath.Count != 0)
			{
				var currentPath = filePath.Dequeue();
				var treeEntry = tree.Single(te => te.TargetType == TreeEntryTargetType.Tree && te.Name.Equals(currentPath));
				if (null == treeEntry)
				{
					return NtStatus.Error;
				}

				if (null == treeEntry.Target)
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

			Log.Log(Default, LogExit);
			return NtStatus.Success;
		}
	}

	public class BranchHandler : SubFolderHandler
	{
		public BranchHandler(string Subfolder) : base(Subfolder, new List<SubFolderHandler>()) { }
		public BranchHandler(string Subfolder, SubFolderHandler Handler) : base(Subfolder, Handler) { }
		public BranchHandler(string Subfolder, IEnumerable<SubFolderHandler> Handlers) : base(Subfolder, Handlers) { }

		protected bool IsSubfolder(ref Repository repository, ref string subfolder)
		{
			string lSubfolder = subfolder;
			return repository.Branches.Single(b => b.FriendlyName.Equals(lSubfolder)) != null;
		}



		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null)
		{
			Log.Log(Default, LogEnter);
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
			Log.Log(Default, LogExit);
			return handler.FindFiles(ref filePath, ref files, info, currentBranch.Tip.Tree, currentBranch.Tip);
		}
	}

	public class BranchesHandler : SubFolderHandler
	{
		public BranchesHandler(string Subfolder) : base(Subfolder) { }
		public BranchesHandler(string Subfolder, SubFolderHandler Handler) : base(Subfolder, Handler) { }
		public BranchesHandler(string Subfolder, IEnumerable<SubFolderHandler> Handlers) : base(Subfolder, Handlers) { }

		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null)
		{
			Log.Log(Default, LogEnter);
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
			Log.Log(Default, LogExit);
			return DokanResult.Success;

		}
	}

	public class TagHandler : SubFolderHandler
	{
		public TagHandler(string Subfolder) : base(Subfolder, new List<SubFolderHandler>()) { }
		public TagHandler(string Subfolder, SubFolderHandler Handler) : base(Subfolder, Handler) { }
		public TagHandler(string Subfolder, IEnumerable<SubFolderHandler> Handlers) : base(Subfolder, Handlers) { }

		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null)
		{
			Log.Log(Default, LogEnter);
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

				if (null == currentTag)
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
			Log.Log(Default, LogExit);
			return DokanResult.Success;

		}
	}

	public class RootHandler : SubFolderHandler
	{
		private string m_RepositoryPath;
		public RootHandler(string Subfolder, string RepositoryPath) : base(Subfolder)
		{
			Log.Log(Default, LogEnter);
			m_RepositoryPath = RepositoryPath;
			m_Handlers.Add(m_Subfolder, this);
			Log.Log(Default, LogExit);
		}

		public RootHandler(string Subfolder, string RepositoryPath, SubFolderHandler Handler) : base(Subfolder, Handler)
		{
			Log.Log(Default, LogEnter);
			m_RepositoryPath = RepositoryPath;
			m_Handlers.Add(m_Subfolder, this);
			Log.Log(Default, LogExit);
		}

		public RootHandler(string Subfolder, string RepositoryPath, IEnumerable<SubFolderHandler> Handlers) : base(
			Subfolder, Handlers)
		{
			Log.Log(Default, LogEnter);
			m_RepositoryPath = RepositoryPath;
			m_Handlers.Add(m_Subfolder, this);
			Log.Log(Default, LogExit);
		}


		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null)
		{
			Log.Log(Default, LogEnter);
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
			Log.Log(Default, LogExit);
			return DokanResult.Success;
		}
	}


}
