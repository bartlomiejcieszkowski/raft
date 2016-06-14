using DokanNet;
using Gafa.GitToFileSystem;
using Gafa.Patterns;
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

		public override NtStatus GetFileInformation(ref Queue<string> filePath, ref FileInformation fileInfo, DokanFileInfo info, params object[] args)
		{
			Log.Log(Default, LogEnter);

			if (!args.RequiredArgs(typeof(Tree), typeof(Commit)))
			{
				Log.Error("Missing required args.");
				return NtStatus.Error;
			}

			var tree = args.GetArg<Tree>();
			var commit = args.GetArg<Commit>();

			if (filePath.Count == 0)
			{
				return NtStatus.Error;
			}

			var treeEntry = GetTreeEntry(ref filePath, tree);
			if (null == treeEntry)
			{
				return NtStatus.Error;
			}

			if (null == treeEntry.Target)
			{
				return NtStatus.Error;
			}

			fileInfo.FileName = filePath.Dequeue();
			fileInfo.Attributes = treeEntry.Mode.Convert();
			fileInfo.CreationTime = commit.Committer.When.UtcDateTime;
			fileInfo.LastWriteTime = commit.Committer.When.UtcDateTime;
			fileInfo.LastAccessTime = commit.Committer.When.UtcDateTime;
			fileInfo.Length = treeEntry.TargetType == TreeEntryTargetType.Blob ? (treeEntry.Target as Blob).Size : 0;

			Log.Log(Default, LogExit);
			return DokanResult.Success;
		}

		public override NtStatus ReadFile(ref Queue<string> filePath, byte[] buffer, ref int bytesRead, long offset, DokanFileInfo info, params object[] args)
		{
			Log.Log(Default, LogEnter);

			if (!args.RequiredArgs(typeof(Tree), typeof(Commit)))
			{
				Log.Error("Missing required args.");
				return NtStatus.Error;
			}

			var tree = args.GetArg<Tree>();
			var commit = args.GetArg<Commit>();

			if (filePath.Count == 0)
			{
				return NtStatus.Error;
			}

			var treeEntry = GetTreeEntry(ref filePath, tree);
			if (null == treeEntry)
			{
				return NtStatus.Error;
			}

			if (null == treeEntry.Target)
			{
				return NtStatus.Error;
			}

			if (TreeEntryTargetType.Blob != treeEntry.TargetType)
			{
				return NtStatus.Error;
			}

			var blob = treeEntry.Target as Blob;

			var contentStream = blob.GetContentStream();
			contentStream.Position = offset;
			
			using (var br = new BinaryReader(contentStream))
			{
				int bytesLeft =  (int)(contentStream.Length - contentStream.Position);
				bytesRead = bytesLeft > buffer.Length ? buffer.Length : bytesLeft;
				br.Read(buffer, 0, bytesRead);
				//br.ReadBytes(bytesRead);
			}

			Log.Log(Default, LogExit);
			return NtStatus.Success;
		}

		private TreeEntry GetTreeEntry(ref Queue<string> filePath, params object[] args)
		{
			if (!args.RequiredArgs(typeof(Tree)))
			{
				Log.Error("Missing required args.");
				return null;
			}

			var tree = args.GetArg<Tree>();

			if(filePath.Count == 0)
			{
				return null;
			}

			while (filePath.Count > 1)
			{
				var descendPath = filePath.Dequeue();
				var treeEntry = tree.Single(te => te.TargetType == TreeEntryTargetType.Tree && te.Name.Equals(descendPath));
				if(null == treeEntry)
				{
					return null;
				}

				if(null == treeEntry)
				{
					return null;
				}

				tree = treeEntry.Target as Tree;
			}

			var currentPath = filePath.Peek();
			return tree.Single(te => te.Name.Equals(currentPath));

		}

		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, params object[] args)
		{
			Log.Log(Default, LogEnter);


			if (!args.RequiredArgs(typeof(Tree), typeof(Commit)))
			{
				Log.Error("Missing required args.");
				return NtStatus.Error;
			}

			var tree = args.GetArg<Tree>();
			var commit = args.GetArg<Commit>();

			if(filePath.Count > 0)
			{
				var treeEntry = GetTreeEntry(ref filePath, tree);
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
					Length = treeEntry.TargetType == TreeEntryTargetType.Blob ? (treeEntry.Target as Blob).Size : 0,
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

		public override NtStatus GetFileInformation(ref Queue<string> filePath, ref FileInformation fileInfo, DokanFileInfo info, params object[] args)
		{
			Log.Log(Default, LogEnter);

			if (!args.RequiredArgs(typeof(Repository)))
			{
				Log.Error("Missing required args.");
				return NtStatus.Error;
			}

			var repository = args.GetArg<Repository>();

			var currentPath = filePath.Dequeue();
			if (!IsSubfolder(ref currentPath))
			{
				return NtStatus.Error;
			}

			var currentBranch = repository.Branches.Single(b => b.FriendlyName.Equals(currentPath));

			if (filePath.Count != 0)
			{
				SubFolderHandler handler = GetRedirection(filePath.Peek());
				if (null == handler)
				{
					return NtStatus.Error;
				}

				return handler.GetFileInformation(ref filePath, ref fileInfo, info, currentBranch.Tip.Tree, currentBranch.Tip);
			}



			fileInfo.FileName = currentPath;

			var firstCommit = repository.Commits.QueryBy(new CommitFilter() { SortBy = CommitSortStrategies.Reverse | CommitSortStrategies.Time }).GetEnumerator().Current;

			fileInfo.Attributes = FileAttributes.Directory;
			fileInfo.LastAccessTime = firstCommit.Committer.When.UtcDateTime; // @TODO: Add more sense
			fileInfo.LastWriteTime = firstCommit.Committer.When.UtcDateTime;
			fileInfo.CreationTime = firstCommit.Committer.When.UtcDateTime;

			Log.Log(Default, LogExit);
			return DokanResult.Success;
		}

		public override NtStatus ReadFile(ref Queue<string> filePath, byte[] buffer, ref int bytesRead, long offset, DokanFileInfo info, params object[] args)
		{
			Log.Log(Default, LogEnter);

			if (!args.RequiredArgs(typeof(Repository)))
			{
				Log.Error("Missing required args.");
				return NtStatus.Error;
			}

			var repository = args.GetArg<Repository>();

			var currentPath = filePath.Dequeue();
			if (!IsSubfolder(ref repository, ref currentPath))
			{
				return NtStatus.Error;
			}

			if (filePath.Count == 0)
			{
				return NtStatus.Error;
			}

			var currentBranch = repository.Branches.Single(b => b.FriendlyName.Equals(currentPath));

			SubFolderHandler handler;
			if (!m_Handlers.TryGetValue("*", out handler))
			{
				return NtStatus.Error;
			}


			var status = handler.ReadFile(ref filePath, buffer, ref bytesRead, offset, info, currentBranch.Tip.Tree, currentBranch.Tip);
			Log.Log(Default, LogExit);
			return status;
		}


		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, params object[] args)
		{
			Log.Log(Default, LogEnter);

			if (!args.RequiredArgs(typeof(Repository)))
			{
				Log.Error("Missing required args.");
				return NtStatus.Error;
			}

			var repository = args.GetArg<Repository>();

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
			var status = handler.FindFiles(ref filePath, ref files, info, currentBranch.Tip.Tree, currentBranch.Tip);
			Log.Log(Default, LogExit);
			return status;
		}
	}

	public class BranchesHandler : SubFolderHandler
	{
		public BranchesHandler(string Subfolder) : base(Subfolder) { }
		public BranchesHandler(string Subfolder, SubFolderHandler Handler) : base(Subfolder, Handler) { }
		public BranchesHandler(string Subfolder, IEnumerable<SubFolderHandler> Handlers) : base(Subfolder, Handlers) { }

		public override NtStatus GetFileInformation(ref Queue<string> filePath, ref FileInformation fileInfo, DokanFileInfo info, params object[] args)
		{
			Log.Log(Default, LogEnter);

			if (!args.RequiredArgs(typeof(Repository)))
			{
				Log.Error("Missing required args.");
				return NtStatus.Error;
			}

			var repository = args.GetArg<Repository>();

			var currentPath = filePath.Dequeue();
			if (!IsSubfolder(ref currentPath))
			{
				return NtStatus.Error;
			}


			if (filePath.Count != 0)
			{
				SubFolderHandler handler = GetRedirection(filePath.Peek());
				if (null == handler)
				{
					return NtStatus.Error;
				}

				return handler.GetFileInformation(ref filePath, ref fileInfo, info, args);
			}



			fileInfo.FileName = currentPath;

			var firstCommit = repository.Commits.QueryBy(new CommitFilter() { SortBy = CommitSortStrategies.Reverse | CommitSortStrategies.Time }).GetEnumerator().Current;

			fileInfo.Attributes = FileAttributes.Directory;
			fileInfo.LastAccessTime = firstCommit.Committer.When.UtcDateTime; // @TODO: Add more sense
			fileInfo.LastWriteTime = firstCommit.Committer.When.UtcDateTime;
			fileInfo.CreationTime = firstCommit.Committer.When.UtcDateTime;

			Log.Log(Default, LogExit);
			return DokanResult.Success;
		}

		public override NtStatus ReadFile(ref Queue<string> filePath, byte[] buffer, ref int bytesRead, long offset, DokanFileInfo info, params object[] args)
		{
			Log.Log(Default, LogEnter);
			if (!args.RequiredArgs(typeof(Repository)))
			{
				Log.Error("Missing required args.");
				return NtStatus.Error;
			}

			var repository = args.GetArg<Repository>();

			var currentPath = filePath.Dequeue();
			if (!IsSubfolder(ref currentPath))
			{
				return NtStatus.Error;
			}

			if (filePath.Count == 0)
			{
				return NtStatus.Error;
			}

			SubFolderHandler handler;
			if (!m_Handlers.TryGetValue(filePath.Peek(), out handler))
			{
				if (!m_Handlers.TryGetValue("*", out handler))
				{
					return NtStatus.Error;
				}
			}

			var status = handler.ReadFile(ref filePath, buffer, ref bytesRead, offset, info, args);
			Log.Log(Default, LogExit);
			return status;
		}

		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, params object[] args)
		{
			Log.Log(Default, LogEnter);

			if (!args.RequiredArgs(typeof(Repository)))
			{
				Log.Error("Missing required args.");
				return NtStatus.Error;
			}

			var repository = args.GetArg<Repository>();

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

				return handler.FindFiles(ref filePath, ref files, info, args[0]);
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

		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, params object[] args)
		{
			Log.Log(Default, LogEnter);

			if (args.Length < 1)
			{
				Log.Error("Invalid length {0}.", args.Length);
				return NtStatus.Error;
			}

			if (!(args[0] is Repository))
			{
				return NtStatus.Error;
			}

			var repository = args[0] as Repository;

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
}
