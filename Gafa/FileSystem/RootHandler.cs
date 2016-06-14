using DokanNet;
using Gafa.Patterns;
using LibGit2Sharp;
using System.Collections.Generic;
using System.IO;

namespace Gafa.FileSystem
{
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

			var currentPath = filePath.Dequeue();
			if (!IsSubfolder(ref currentPath))
			{
				return NtStatus.Error;
			}

			if (filePath.Count == 0)
			{
				return NtStatus.Error;
			}

			SubFolderHandler handler = GetRedirection(filePath.Peek());
			if (null == handler)
			{
				return NtStatus.Error;
			}

			var status = handler.ReadFile(ref filePath, buffer, ref bytesRead, offset, info, args);

			Log.Log(Default, LogExit);
			return status;
		}

		public override NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, params object[] args)
		{
			Log.Log(Default, LogEnter);

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

				return handler.FindFiles(ref filePath, ref files, info, args);
			}

			AddHandlersAsSubfolders(ref files);
			Log.Log(Default, LogExit);
			return DokanResult.Success;
		}
	}
}
