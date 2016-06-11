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

	}
}
