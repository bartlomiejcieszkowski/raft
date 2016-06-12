using System;

namespace Gafa.FileSystem
{
	public interface IFileSystem
	{
		/// <summary>
		/// Mounts filesystem
		/// </summary>
		void Mount();
		/// <summary>
		/// Unmounts filesystem
		/// </summary>
		void Unmount();
	}

	public abstract class FilesystemStringOperations
	{
		/// <summary>
		/// Checks whether path is a root.
		/// </summary>
		/// <param name="Path">path</param>
		/// <returns></returns>
		public static bool IsRoot(string Path)
		{
			return Path == @"\";
		}
	}

	public class FilesystemInformation
	{
		public DateTime m_OpenTime;
		public string m_VolumeLabel;
		public string m_Mountpoint;
	}

}
