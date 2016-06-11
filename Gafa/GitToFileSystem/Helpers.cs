using LibGit2Sharp;
using System.IO;

namespace Gafa.GitToFileSystem
{
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
}
