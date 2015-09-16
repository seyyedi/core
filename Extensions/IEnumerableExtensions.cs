using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Seyyedi
{
	public static class IEnumerableExtensions
	{
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T, int> method)
		{
			var i = 0;

			foreach (var item in items)
			{
				method(item, i);
				i++;
			}

			return items;
		}

		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T> method)
		{
			foreach (var item in items)
			{
				method(item);
			}

			return items;
		}

		public static async Task<IEnumerable<T>> ForEachAsync<T>(this IEnumerable<T> items, Func<T, Task> method)
		{
			foreach (var item in items)
			{
				await method(item);
			}

			return items;
		}

		public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, params T[] items)
		{
			return first.Concat((IEnumerable<T>)items);
		}

		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> items)
			where T : class
		{
			return items.Where(i => i != null);
		}

		public static string Join(this IEnumerable<string> items, string separator)
		{
			return string.Join(separator, items.ToArray());
		}

		public static List<T> AddTo<T>(this IEnumerable<T> items, List<T> target)
		{
			target.AddRange(items);

			return target;
		}

		public static void AddRange<T>(this List<T> list, params T[] items)
		{
			list.AddRange((IEnumerable<T>)items);
		}
	}
}