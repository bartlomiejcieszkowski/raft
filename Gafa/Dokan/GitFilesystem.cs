using DokanNet;
using Gafa.FileSystem;
using Gafa.Logging;
using System;
using System.IO;
using System.Threading;

namespace Gafa.Dokan
{
	public partial class GitFilesystem : Logger, IFileSystem, IDokanOperations
	{
		private SubFolderHandler m_RootHandler;
		private FilesystemInformation m_FilesystemInformation;
		private string m_RepositoryPath;

		public GitFilesystem(string mountPoint, string repositoryPath, SubFolderHandler Handler) : base()
		{
			Log.Log(Default, LogEnter);
			m_FilesystemInformation = new FilesystemInformation()
			{
				m_VolumeLabel = Path.GetDirectoryName(repositoryPath),
				m_Mountpoint = mountPoint,
				m_OpenTime = DateTime.UtcNow
			};
			m_RepositoryPath = repositoryPath;
			m_RootHandler = Handler;
			Log.Log(Default, LogExit);
		}

		public void Mount()
		{
			Log.Log(Default, LogEnter);
			new Thread(() =>
			{
				DokanNet.Dokan.Mount(this, m_FilesystemInformation.m_Mountpoint);
			}).Start();
			Log.Log(Default, LogExit);
		}

		public void Unmount()
		{
			Log.Log(Default, LogEnter);
			new Thread(() =>
			{
				DokanNet.Dokan.RemoveMountPoint(m_FilesystemInformation.m_Mountpoint);
			}).Start();
			Log.Log(Default, LogExit);
		}
	}

}
