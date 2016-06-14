using DokanNet;
using Gafa.Logging;
using System.Collections.Generic;
using System;
using System.IO;
using System.Security.AccessControl;
using Gafa.Patterns;

namespace Gafa.FileSystem
{
	public abstract partial class SubFolderHandler : Logger, ISubFolderHandler
	{
		public virtual void Cleanup(ref Queue<string> filePath, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2}", filePath.ToStringIEnumerable(), info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
		}

		public virtual void CloseFile(ref Queue<string> filePath, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2}", filePath.ToStringIEnumerable(), info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
		}

		public virtual NtStatus CreateFile(ref Queue<string> filePath, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4} {5} {6} {7}", filePath.ToStringIEnumerable(), access, share, mode, options, attributes, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus DeleteDirectory(ref Queue<string> filePath, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2}", filePath.ToStringIEnumerable(), info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus DeleteFile(ref Queue<string> filePath, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2}", filePath.ToStringIEnumerable(), info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus FindFiles(ref Queue<string> filePath, ref IList<FileInformation> files, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", filePath.ToStringIEnumerable(), files, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus FindStreams(ref Queue<string> filePath, ref IList<FileInformation> streams, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", filePath.ToStringIEnumerable(), streams, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus FlushFileBuffers(ref Queue<string> filePath, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2}", filePath.ToStringIEnumerable(), info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus GetDiskFreeSpace(ref long freeBytesAvailable, ref long totalNumberOfBytes, ref long totalNumberOfFreeBytes, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4}", freeBytesAvailable, totalNumberOfBytes, totalNumberOfFreeBytes, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus GetFileInformation(ref Queue<string> filePath, ref FileInformation fileInfo, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", filePath.ToStringIEnumerable(), fileInfo, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus GetFileSecurity(ref Queue<string> filePath, ref FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4}", filePath.ToStringIEnumerable(), security, sections, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus GetVolumeInformation(ref string volumeLabel, ref FileSystemFeatures features, ref string fileSystemName, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4}", volumeLabel, features, fileSystemName, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus LockFile(ref Queue<string> filePath, long offset, long length, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4}", filePath.ToStringIEnumerable(), offset, length, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus Mounted(DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1}", info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus MoveFile(ref Queue<string> oldFilePath, ref Queue<string> newFilePath, bool replace, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4}", oldFilePath, newFilePath, replace, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus ReadFile(ref Queue<string> filePath, byte[] buffer, ref int bytesRead, long offset, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4} {5}", filePath.ToStringIEnumerable(), buffer, bytesRead, offset, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus SetAllocationSize(ref Queue<string> filePath, long length, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", filePath.ToStringIEnumerable(), length, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus SetEndOfFile(ref Queue<string> filePath, long length, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", filePath.ToStringIEnumerable(), length, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus SetFileAttributes(ref Queue<string> filePath, FileAttributes attributes, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", filePath.ToStringIEnumerable(), attributes, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus SetFileSecurity(ref Queue<string> filePath, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4}", filePath.ToStringIEnumerable(), security, sections, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus SetFileTime(ref Queue<string> filePath, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", filePath.ToStringIEnumerable(), creationTime, lastAccessTime, lastWriteTime, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus UnlockFile(ref Queue<string> filePath, long offset, long length, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4}", filePath.ToStringIEnumerable(), offset, length, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus Unmounted(DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1}", info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus WriteFile(ref Queue<string> filePath, byte[] buffer, ref int bytesWritten, long offset, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4} {5}", filePath.ToStringIEnumerable(), buffer, bytesWritten, offset, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}
	}
}
