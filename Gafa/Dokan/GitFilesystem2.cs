using DokanNet;
using Gafa.Dokan;
using LibGit2Sharp;
using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace Gafa.Dokan
{
	public static class GitFilesystemExtensions2
	{
		/// <summary>
		/// Converts path subfolder to PathType
		/// </summary>
		/// <param name="path">subfolder</param>
		/// <returns>PathType</returns>
		public static PathType ToPathType(this string path)
		{
			if (path.Equals(""))
			{
				return PathType.root;
			}

			PathType retVal;
			if (!Enum.TryParse(path, out retVal))
			{
				retVal = PathType.unknown;
			}

			return retVal;
		}

		public static Branch Branch(this Repository repository, string branchName)
		{
			return repository.Branches.Single(b => branchName.Equals(b.FriendlyName));
		}

		public static Commit Commit(this Branch branch)
		{
			return branch.Tip;
		}

		public static Tag Tag(this Repository repository, string tagName)
		{
			return repository.Tags.Single(t => tagName.Equals(t.FriendlyName));
		}

		public static Commit Commit(this Tag tag)
		{
			return tag.PeeledTarget as Commit;
		}
		

		//public static object Descend(this Tree tree, st)
		//{
		//	return tree.
		//}
	}

	

	public class GitFileEntry
	{
		public Branch m_Branch;
		public GitObject m_GitObject;
	}

	public enum PathType
	{
		unknown,
		root,
		branches,
		tags,
		invalid,
	}


	public partial class GitFilesystem2 : Logging.Logger
	{
		private Repository m_Repository;
		
		public GitFilesystem2(string mountPoint, string repositoryPath) : base()
		{
			Log.Trace(LogEnter);
			m_FilesystemInformation = new FilesystemInformation()
			{
				m_VolumeLabel = Path.GetDirectoryName(repositoryPath),
				m_Mountpoint = mountPoint,
				m_OpenTime = DateTime.UtcNow
			};
			m_RepositoryPath = repositoryPath;
			m_Handler = Handler;
			m_Repository = new Repository(m_RepositoryPath);
			Log.Trace(LogExit);
		}

		public GitObjectWrapper GetGitObjectWrapper(string fileName)
		{
			var wrapper = GitObjectWrapper.GetWrapper(fileName, m_Repository);
			if(null == wrapper)
			{
				Log.Error("Unable to get wrapper.");
				return null;
			}

			Log.Info("IsFiles:{0} IsRoot:{1} IsBranches:{2} IsTags:{3}",
				wrapper.IsFiles(),
				wrapper.IsRoot(),
				wrapper.IsBranches(),
				wrapper.IsTags()
				);

			return wrapper;
		}

		public class GitObjectWrapper
		{
			//private static object safetyLock = new object();
			//private GitObject _GitObject = null;
			//public GitObject m_GitObject
			//{
			//	get
			//	{
			//		if (_GitObject == null)
			//		{
			//			lock(safetyLock)
			//			{
			//				if (_GitObject == null)
			//				{

			//				}
			//			}
			//		}
			//	}
			//}

			//public IEnumerable<>

			public Repository m_Repository;
			public string m_SubPath;
			public string m_MainPath;

			public GitObjectWrapper(Repository repository, string mainPath, string subPath)
			{
				m_Repository = repository;
				m_MainPath = mainPath;
				m_SubPath = subPath;
			}

			public static GitObjectWrapper GetWrapper(string fileName, Repository repository, params object[] args)
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
					return null;
				}

				return new GitObjectWrapper(repository, splittedPath[1], splittedPath.Length > 2 ? splittedPath[2] : null);
			}

			internal bool IsFiles()
			{
				return null != m_SubPath;
			}

			internal bool IsRoot()
			{
				return m_MainPath.Equals("");
			}

			internal bool IsBranches()
			{
				return m_MainPath.Equals("branches");
			}

			internal bool IsTags()
			{
				return m_MainPath.Equals("tags");
			}


		}

		//private GitObjectWrapper GetTags(string item2)
		//{
		//	return new GitObjectWrapper(m_Repository.Tags, item2);
		//}

		//private GitObjectWrapper GetBranches(string item2)
		//{
		//	return new GitObjectWrapper(m_Repository.Branches, item2);
		//}

		//private GitObjectWrapper GetRoot(string item2)
		//{
		//	return new GitObjectWrapper(m_Repository, item2);
		//}
	}
}
