using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gafa.Patterns
{
	public static class StucturesExtensions
	{
		public static Stack<T> ToStack<T>(this IEnumerable<T> enumerable)
		{
			return new Stack<T>(enumerable);
		}

		public static Queue<T> ToQueue<T>(this IEnumerable<T> enumerable)
		{
			return new Queue<T>(enumerable);
		}

		public static string ToStringArray(this object[] args)
		{
			if(args.Length == 0)
			{
				return "[]";
			}
			var stringBuilder = new StringBuilder();
			stringBuilder.Append('[');
			for(int i=0; i<args.Length; ++i)
			{
				stringBuilder.AppendFormat("{{{0} : {1}}} ", args[i].ToString());
			}
			stringBuilder.Append(']');
			return stringBuilder.ToString();
		}

		public static string ToStringIEnumerable<T>(this IEnumerable<T> enumerable)
		{
			if(!enumerable.Any())
			{
				return "[]";
			}
			var stringBuilder = new StringBuilder();
			stringBuilder.Append('[');
			int i = 0;
			foreach(var element in enumerable)
			{
				stringBuilder.AppendFormat("{{{0} : {1}}} ", i++, element.ToString());
			}
			stringBuilder.Append(']');
			return stringBuilder.ToString();
		}

		public static string ToStringDokanFileInfo(this DokanNet.DokanFileInfo info)
		{
			return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}", info.Context, info.DeleteOnClose, info.IsDirectory, info.NoCache, info.PagingIo, info.ProcessId, info.SynchronousIo, info.WriteToEndOfFile, info.GetRequestor());
		}

		public static Queue<string> PathToQueue(this string path)
		{
			return path.Split('\\').ToQueue();
		}
	}

	public static class ParamsParsing
	{
		/// <summary>
		/// Returns arg.
		/// </summary>
		/// <typeparam name="T">type of arg</typeparam>
		/// <param name="idx">index of arg</param>
		/// <param name="args">args array</param>
		/// <returns>null if T doesn't match, T otherwise</returns>
		public static T GetArgAt<T>(this object[] args, int idx) where T : class
		{
			if (idx < args.Length)
			{
				if (args[idx] is T)
				{
					return args[idx] as T;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns first argument matching type.
		/// </summary>
		/// <typeparam name="T">type of arg</typeparam>
		/// <param name="args">args array</param>
		/// <returns>null if no arg matches T, T otherwise</returns>
		public static T GetArg<T>(this object[] args) where T : class
		{
			if (!args.Any())
				return null;

			foreach (var arg in args)
			{
				if (arg is T)
				{
					return (arg as T);
				}
			}

			return null;
		}

		public static bool RequiredArgsCount(this object[] args, int length)
		{
			return args.Length == length;
		}

		public static bool RequiredArgs(this object[] args, params Type[] types)
		{
			bool[] matches = new bool[types.Length];
			for(int i=0; i<args.Length; ++i)
			{
				for(int t=0; t<types.Length; ++t)
				{
					if (matches[t]) continue;
					if(args[i].GetType() == types[t])
					{
						matches[t] = true;
					}
				}
			}
			bool success = true;
			for(int m=0; m<matches.Length; ++m)
			{
				success &= matches[m];
			}
			return success;
		}
	}
}
