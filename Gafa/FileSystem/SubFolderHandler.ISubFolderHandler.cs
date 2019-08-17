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
		public virtual void Cleanup(string fileName, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2}", fileName.ToStringIEnumerable(), info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
		}

		public virtual void CloseFile(string fileName, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2}", fileName.ToStringIEnumerable(), info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
		}

		public virtual NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4} {5} {6} {7}", fileName.ToStringIEnumerable(), access, share, mode, options, attributes, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus DeleteDirectory(string fileName, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2}", fileName.ToStringIEnumerable(), info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus DeleteFile(string fileName, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2}", fileName.ToStringIEnumerable(), info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus FindFiles(string fileName, ref IList<FileInformation> files, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", fileName.ToStringIEnumerable(), files, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus FindStreams(string fileName, ref IList<FileInformation> streams, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", fileName.ToStringIEnumerable(), streams, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus FlushFileBuffers(string fileName, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2}", fileName.ToStringIEnumerable(), info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus GetDiskFreeSpace(ref long freeBytesAvailable, ref long totalNumberOfBytes, ref long totalNumberOfFreeBytes, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4}", freeBytesAvailable, totalNumberOfBytes, totalNumberOfFreeBytes, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus GetFileInformation(string fileName, ref FileInformation fileInfo, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", fileName.ToStringIEnumerable(), fileInfo, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus GetFileSecurity(string fileName, ref FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4}", fileName.ToStringIEnumerable(), security, sections, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus GetVolumeInformation(ref string volumeLabel, ref FileSystemFeatures features, ref string fileSystemName, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4}", volumeLabel, features, fileSystemName, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4}", fileName.ToStringIEnumerable(), offset, length, info, args.ToStringArray());
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

		public virtual NtStatus ReadFile(string fileName, byte[] buffer, ref int bytesRead, long offset, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4} {5}", fileName.ToStringIEnumerable(), buffer, bytesRead, offset, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", fileName.ToStringIEnumerable(), length, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", fileName.ToStringIEnumerable(), length, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", fileName.ToStringIEnumerable(), attributes, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4}", fileName.ToStringIEnumerable(), security, sections, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3}", fileName.ToStringIEnumerable(), creationTime, lastAccessTime, lastWriteTime, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4}", fileName.ToStringIEnumerable(), offset, length, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus Unmounted(DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1}", info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}

		public virtual NtStatus WriteFile(string fileName, byte[] buffer, ref int bytesWritten, long offset, DokanFileInfo info, params object[] args)
		{
			Log.Trace("{0} {1} {2} {3} {4} {5}", fileName.ToStringIEnumerable(), buffer, bytesWritten, offset, info, args.ToStringArray());
			Log.Log(NotImplemented, LogNotImplemented);
			return NtStatus.NotImplemented;
		}
	}
}
