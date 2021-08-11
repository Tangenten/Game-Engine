using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RavUtilities {
	public static class RandomU {
		private static ThreadLocal<RandomNumberGenerator> r = new ThreadLocal<RandomNumberGenerator>(() => new RandomNumberGenerator());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Get(float min, float max) { return r.Value.Get(min, max); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Get(double min, double max) { return r.Value.Get(min, max); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Get(int min, int max) { return r.Value.Get(min, max); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Get(uint min, uint max) { return r.Value.Get(min, max); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static short Get(short min, short max) { return r.Value.Get(min, max); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte Get(byte min, byte max) { return r.Value.Get(min, max); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetBell(float min, float max, float mu = 0, float sigma = 1f) { return r.Value.GetBell(min, max, mu, sigma); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetTriangular(float min, float max, float mid) { return r.Value.GetTriangular(min, max, mid); }
	}

	public class RandomNumberGenerator {
		private Random r;

		public RandomNumberGenerator(int? seed = null) {
			if (seed != null) {
				this.r = new Random((int) seed);
			} else {
				this.r = new Random();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float Get(float min, float max) { return (float) (this.r.NextDouble() * (max - min)) + min; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Get(double min, double max) { return this.r.NextDouble() * (max - min) + min; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Get(int min, int max) { return this.r.Next(min, max + 1); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint Get(uint min, uint max) { return (uint) this.r.Next((int) min, (int) max + 1); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public short Get(short min, short max) { return (short) this.r.Next(min, max + 1); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte Get(byte min, byte max) { return (byte) this.r.Next(min, max + 1); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetBell(float min, float max, float mu = 0, float sigma = 1f) {
			float x1 = (float) (1 - this.r.NextDouble());
			float x2 = (float) (1 - this.r.NextDouble());

			float y1 = MathF.Sqrt(-2f * MathF.Log(x1)) * MathF.Sin(2f * MathF.PI * x2);
			float y2 = y1 * sigma + mu;
			y2 /= 3;
			y2 += 1;
			y2 /= 2;
			return y2 * (max - min) + min;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetTriangular(float min, float max, float mid) {
			float u = (float) this.r.NextDouble();

			return u < (mid - min) / (max - min)
				? min + MathF.Sqrt(u * (max - min) * (mid - min))
				: max - MathF.Sqrt((1 - u) * (max - min) * (max - mid));
		}
	}
}