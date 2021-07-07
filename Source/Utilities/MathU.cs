using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace RavUtilities {
	public static class MathU {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Mod(int x, int m) {
			if (m == 0) {
				return m;
			}

			return (int)((uint)x % m);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Mod(float x, float m) {
			if (m == 0) {
				return m;
			}

			float r = x % m;
			return r < 0 ? r + m : r;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ModRange(int x, int min, int max) {
			return ((x - min) % (max - min) + (max - min)) % (max - min) + min;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ModRange(float x, float min, float max) {
			return ((x - min) % (max - min) + (max - min)) % (max - min) + min;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long NextPowerOfTwo(long n) {
			return (long)Math.Pow(2.0, Math.Ceiling(Math.Log(n, 2.0)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int NextPowerOfTwo(int n) {
			return (int)Math.Pow(2.0, Math.Ceiling(Math.Log(n, 2.0)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float NextPowerOfTwo(float n) {
			return MathF.Pow(2f, MathF.Ceiling(MathF.Log(n, 2f)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double NextPowerOfTwo(double n) {
			return Math.Pow(2.0, Math.Ceiling(Math.Log(n, 2.0)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Factorial(int n) {
			long num = 1;
			for (; n > 1; --n) {
				num *= n;
			}

			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DegreesToRadians(float degrees) {
			return degrees * (MathF.PI / 180f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float RadiansToDegrees(float radians) {
			return radians * 57.29578f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double DegreesToRadians(double degrees) {
			return degrees * (Math.PI / 180.0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double RadiansToDegrees(double radians) {
			return radians * (180.0 / Math.PI);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static float InverseSqrtFast(float x) {
			unsafe {
				float xhalf = 0.5f * x;
				int i = *(int*)&x; // Read bits as integer.
				i = 0x5f375a86 - (i >> 1); // Make an initial guess for Newton-Raphson approximation
				x = *(float*)&i; // Convert bits back to float
				x = x * (1.5f - xhalf * x * x); // Perform left single Newton-Raphson step.
				return x;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static double InverseSqrtFast(double x) {
			unsafe {
				double xhalf = 0.5 * x;
				long i = *(long*)&x; // Read bits as long.
				i = 0x5fe6eb50c7b537a9 - (i >> 1); // Make an initial guess for Newton-Raphson approximation
				x = *(double*)&i; // Convert bits back to double
				x = x * (1.5 - xhalf * x * x); // Perform left single Newton-Raphson step.
				return x;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
