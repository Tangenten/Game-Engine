using System.Numerics;
using RavUtilities;

namespace RavEngine {
	public static class ArrayE {
		public static void FillVectorized<T>(this T[] arrayToFill, T value) where T : struct {
			var fillVector = new Vector<T>(value);
			int numFullVectorsIndex = (arrayToFill.Length / Vector<T>.Count) * Vector<T>.Count;
			int i;
			for (i = 0; i < numFullVectorsIndex; i += Vector<T>.Count)
				fillVector.CopyTo(arrayToFill, i);
			for (; i < arrayToFill.Length; i++)
				arrayToFill[i] = value;
		}

		public static void FillVectorized<T>(this T[] arrayToFill, T value, int startIndex, int length) where T : struct {
			var fillVector = new Vector<T>(value);
			int numFullVectorsIndex = (length / Vector<T>.Count) * Vector<T>.Count;
			int i;
			for (i = startIndex; i < numFullVectorsIndex; i += Vector<T>.Count)
				fillVector.CopyTo(arrayToFill, i);
			for (; i < arrayToFill.Length; i++)
				arrayToFill[i] = value;
		}

		public static T GetRandomElement<T>(this T[] array) {
			return array[RandomU.Get(0, array.Length - 1)];
		}
	}
}
