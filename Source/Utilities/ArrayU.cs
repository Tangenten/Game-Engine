using System.Numerics;

namespace RavUtilities {
	public static class ArrayU {
		public static void FillVectorized<T>(this T[] arrayToFill, T value) where T : struct {
			Vector<T> fillVector = new Vector<T>(value);
			int numFullVectorsIndex = arrayToFill.Length / Vector<T>.Count * Vector<T>.Count;
			int i;
			for (i = 0; i < numFullVectorsIndex; i += Vector<T>.Count) fillVector.CopyTo(arrayToFill, i);
			for (; i < arrayToFill.Length; i++) arrayToFill[i] = value;
		}

		public static void FillVectorized<T>(this T[] arrayToFill, T value, int startIndex, int length) where T : struct {
			Vector<T> fillVector = new Vector<T>(value);
			int numFullVectorsIndex = length / Vector<T>.Count * Vector<T>.Count;
			int i;
			for (i = startIndex; i < numFullVectorsIndex; i += Vector<T>.Count) fillVector.CopyTo(arrayToFill, i);
			for (; i < arrayToFill.Length; i++) arrayToFill[i] = value;
		}

		public static T GetRandomElement<T>(this T[] array) { return array[RandomU.Get(0, array.Length - 1)]; }

		public static void AddArrays(ref float[] array1, ref float[] array2) {
			int i = 0;
			int simdLength = Vector<float>.Count;
			int array1Length = array1.Length;
			for (i = 0; i <= array1Length - simdLength; i += simdLength) {
				Vector<float> vectorSection = new Vector<float>(array1, i);
				Vector<float> vectorSection2 = new Vector<float>(array2, i);
				(vectorSection + vectorSection2).CopyTo(array1, i);
			}

			for (; i < array1Length; i++) {
				array1[i] += array2[i];
			}
		}
	}
}