using Gafa.Logging;
using System.Collections.Generic;

namespace Gafa.FileSystem
{
	public class MountsList : Logger
	{
		public ICollection<IFileSystem> m_FileSystems;

		public MountsList(ICollection<IFileSystem> FileSystems) : base()
		{
			m_FileSystems = FileSystems;
		}

		public MountsList() : base()
		{
			m_FileSystems = new List<IFileSystem>();
		}

		public void Mount()
		{
			Log.Log(Default, LogEnter);
			foreach (var fileSystem in m_FileSystems)
			{
				fileSystem.Mount();
			}
			Log.Log(Default, LogExit);
		}

		public void Unmount()
		{
			Log.Log(Default, LogEnter);
			foreach (var fileSystem in m_FileSystems)
			{
				fileSystem.Unmount();
			}
			Log.Log(Default, LogExit);
		}
	}
}
