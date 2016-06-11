using DokanNet;
using Gafa.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gafa.FileSystem
{
	public abstract class SubFolderHandler : Logger
	{
		protected string m_Subfolder;

		protected virtual bool IsSubfolder(ref string subfolder) { return m_Subfolder.Equals(subfolder); }

		protected IDictionary<string, SubFolderHandler> m_Handlers;


		protected SubFolderHandler(string Subfolder) : base()
		{
			Log.Log(Default, LogEnter);
			m_Subfolder = Subfolder;
			m_Handlers = new Dictionary<string, SubFolderHandler>();
			Log.Log(Default, LogExit);
		}

		protected SubFolderHandler(string Subfolder, SubFolderHandler Handler) : base()
		{
			Log.Log(Default, LogEnter);
			m_Subfolder = Subfolder;
			m_Handlers = new Dictionary<string, SubFolderHandler>()
			{
				{Handler.m_Subfolder, Handler }
			};

			
			Log.Log(Default, LogExit);
		}

		protected SubFolderHandler(string Subfolder, IEnumerable<SubFolderHandler> Handlers) : base()
		{
			Log.Log(Default, LogEnter);
			m_Subfolder = Subfolder;
			m_Handlers = Handlers.ToDictionary(p => p.m_Subfolder);
			Log.Log(Default, LogExit);
		}

		public FileInformation GetFileInformation()
		{
			return new FileInformation()
			{
				Attributes = FileAttributes.Directory,
				CreationTime = DateTime.UtcNow,
				LastWriteTime = DateTime.UtcNow,
				LastAccessTime = DateTime.UtcNow,
				FileName = m_Subfolder
			};
		}

		protected void AddHandlersAsSubfolders(ref IList<FileInformation> files)
		{
			Log.Log(Default, LogEnter);
			foreach (var entry in m_Handlers)
			{
				if (entry.Value == this) continue;
				if (entry.Value.m_Subfolder == "*") continue;

				files.Add(entry.Value.GetFileInformation());
			}
			Log.Log(Default, LogExit);
		}

		public abstract NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, Object UnkObject, Object UnkObject2 = null);
	}

}
