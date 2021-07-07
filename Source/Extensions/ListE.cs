using System.Collections.Generic;
using System.Linq;
using RavUtilities;

namespace RavEngine {
	public static class ListE {
		public static T GetRandomElement<T>(this List<T> list) {
			return list[RandomU.Get(0, list.Count - 1)];
		}

		public static void Resize<T>(this List<T> list, int size, T element = default) {
			int count = list.Count;

			if (size < count) {
				list.RemoveRange(size, count - size);
			} else if (size > count) {
				if (size > list.Capacity) {
					list.Capacity = size;
				}

				list.AddRange(Enumerable.Repeat(element, size - count));
			}
		}
	}
}
