using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RavUtilities {
	public static class TweenU {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Linear(float fromMin, float fromMax, float fraction) { return fromMin * (1f - fraction) + fromMax * fraction; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Linear(double fromMin, double fromMax, double fraction) { return fromMin * (1f - fraction) + fromMax * fraction; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Linear(float fromValue, float fromMin, float fromMax, float toMin, float toMax) { return Math.Clamp(Linear(toMin, toMax, (fromValue - fromMin) / (fromMax - fromMin)), MathF.Min(toMin, toMax), MathF.Max(toMin, toMax)); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Linear(double fromValue, double fromMin, double fromMax, double toMin, double toMax) { return Math.Clamp(Linear(toMin, toMax, (fromValue - fromMin) / (fromMax - fromMin)), Math.Min(toMin, toMax), Math.Max(toMin, toMax)); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Cosine(float fromMin, float fromMax, float fraction) {
			fraction = (1f - MathF.Cos(fraction * MathF.PI)) / 2f;
			return fromMin * (1f - fraction) + fromMax * fraction;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Cosine(float fromValue, float fromMin, float fromMax, float toMin, float toMax) { return Math.Clamp(Cosine(toMin, toMax, (fromValue - fromMin) / (fromMax - fromMin)), MathF.Min(toMin, toMax), MathF.Max(toMin, toMax)); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Linear(float fromValue, float fromMin, float fromMax, in float[] array) {
			float x = Linear(fromValue, fromMin, fromMax, 0f, array.Length - 1f);
			int xRoundDown = (int) x;
			int xRoundup = (int) (x + 1f);
			float xFraction = x - xRoundDown;
			return Linear(array[xRoundDown], array[xRoundup], xFraction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Linear(float fromValue, float fromMin, float fromMax, in List<float> list) {
			float x = Linear(fromValue, fromMin, fromMax, 0f, list.Count - 1f);
			int xRoundDown = (int) x;
			int xRoundup = (int) (x + 1f);
			float xFraction = x - xRoundDown;
			return Linear(list[xRoundDown], list[xRoundup], xFraction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Cosine(float fromValue, float fromMin, float fromMax, in float[] array) {
			float x = Cosine(fromValue, fromMin, fromMax, 0f, array.Length - 1f);
			int xRoundDown = (int) x;
			int xRoundup = (int) (x + 1f);
			float xFraction = x - xRoundDown;
			return Cosine(array[xRoundDown], array[xRoundup], xFraction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Cosine(float fromValue, float fromMin, float fromMax, in List<float> list) {
			float x = Cosine(fromValue, fromMin, fromMax, 0f, list.Count - 1f);
			int xRoundDown = (int) x;
			int xRoundup = (int) (x + 1f);
			float xFraction = x - xRoundDown;
			return Cosine(list[xRoundDown], list[xRoundup], xFraction);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Cubic(float y0, float y1, float y2, float y3, float mu) {
			float a0, a1, a2, a3, mu2;

			mu2 = mu * mu;
			a0 = y3 - y2 - y0 + y1;
			a1 = y0 - y1 - a0;
			a2 = y2 - y0;
			a3 = y1;

			return a0 * mu * mu2 + a1 * mu2 + a2 * mu + a3;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Cubic(double y0, double y1, double y2, double y3, double mu) {
			double a0, a1, a2, a3, mu2;

			mu2 = mu * mu;
			a0 = y3 - y2 - y0 + y1;
			a1 = y0 - y1 - a0;
			a2 = y2 - y0;
			a3 = y1;

			return a0 * mu * mu2 + a1 * mu2 + a2 * mu + a3;
		}

		public static float CubicHermite(float A, float B, float C, float D, float t) {
			float a = -A / 2.0f + 3.0f * B / 2.0f - 3.0f * C / 2.0f + D / 2.0f;
			float b = A - 5.0f * B / 2.0f + 2.0f * C - D / 2.0f;
			float c = -A / 2.0f + C / 2.0f;
			float d = B;

			return a * t * t * t + b * t * t + c * t + d;
		}

		public static double CubicHermite(double A, double B, double C, double D, double t) {
			double a = -A / 2.0 + 3.0 * B / 2.0 - 3.0 * C / 2.0 + D / 2.0;
			double b = A - 5.0 * B / 2.0 + 2.0 * C - D / 2.0;
			double c = -A / 2.0 + C / 2.0;
			double d = B;

			return a * t * t * t + b * t * t + c * t + d;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Accelerate(float fromMin, float fromMax, float fraction, float factor) { return MathF.Pow(fraction, 2f * factor) * (fromMax - fromMin) + fromMin; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Accelerate(float fromValue, float fromMin, float fromMax, float toMin, float toMax, float factor) { return Accelerate(toMin, toMax, (fromValue - fromMin) / (fromMax - fromMin), factor); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Decelerate(float fromMin, float fromMax, float fraction, float factor) { return MathF.Pow(fraction, 1f / (factor * 2f)) * (fromMax - fromMin) + fromMin; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Decelerate(float fromValue, float fromMin, float fromMax, float toMin, float toMax, float factor) { return Decelerate(toMin, toMax, (fromValue - fromMin) / (fromMax - fromMin), factor); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SmoothStep(float x, float x0, float x1) {
			x = x * x * (3f - 2f * x);
			return x * (x1 - x0) + x0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SmootherStep(float x, float x0, float x1) {
			x = x * x * x * (x * (x * 6 - 15) + 10);
			return x * (x1 - x0) + x0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SmoothToTarget(float current, float target, float scalar) { return current += (target - current) * scalar; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InLinear(float t) { return t; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float OutLinear(float t) { return 1f - t; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InCubic(float t) { return t * t * t; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float OutCubic(float t) {
			t -= 1f;
			return t * t * t + 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InOutCubic(float t) {
			t /= 2f;
			if (t < 1f) {
				return 0.5f * t * t * t;
			}
			t -= 2f;
			return 0.5f * (t * t * t + 2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InQuadratic(float t) { return t * t; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float OutQuadratic(float t) { return -1f * t * (t - 2); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InOutQuadratic(float t) {
			t /= 2f;
			if (t < 1f) {
				return 0.5f * t * t;
			}
			t -= 1f;
			return -0.5f * (t * (t - 2) - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InQuartic(float t) { return t * t * t * t; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float OutQuartic(float t) {
			t--;
			return -1f * (t * t * t * t - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InOutQuartic(float t) {
			t /= 2f;
			if (t < 1f) {
				return 0.5f * t * t * t * t;
			}
			return -0.5f * (t * t * t * t - 2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InQuintic(float t) { return t * t * t * t * t; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float OutQuintic(float t) {
			t -= 1f;
			return t * t * t * t + 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InOutQuintic(float t) {
			t /= 2f;
			if (t < 1) {
				return 0.5f * t * t * t * t * t;
			}
			t -= 2;
			return 0.5f * (t * t * t * t * t + 2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InSin(float t) { return -1f * (float) Math.Cos(t * (Math.PI / 2.0)) + 1f; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float OutSin(float t) { return +1f * (float) Math.Sin(t * (Math.PI / 2.0)); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InOutSin(float t) { return -0.5f * ((float) Math.Cos(Math.PI * t) - 1f); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InExponential(float t) { return (float) Math.Pow(2, 10 * (t - 1)); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float OutExponential(float t) { return (float) -Math.Pow(2, -10 * t) + 1f; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InOutExponential(float t) {
			t /= 2f;
			if (t < 1) {
				return 0.5f * (float) Math.Pow(2, 10 * (t - 1));
			}
			t -= 1f;
			return 0.5f * ((float) -Math.Pow(2, -10 * t) + 2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InCircular(float t) { return -1f * ((float) Math.Sqrt(1 - t * t) - 1); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float OutCircular(float t) {
			t -= 1f;
			return (float) Math.Sqrt(1 - t * t);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InOutCircular(float t) {
			t /= 2f;
			if (t < 1) {
				return -0.5f * ((float) Math.Sqrt(1 - t * t) - 1);
			}
			return 0.5f * ((float) Math.Sqrt(1 - t * t) + 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InBack(float t) {
			const float s = 1.70158f;
			return t * t * ((s + 1) * t - s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float OutBack(float t) {
			const float s = 1.70158f;
			t -= 1f;
			return t * t * ((s + 1) * t + s) + 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InOutBack(float t) {
			const float s = 1.70158f;
			const float s2 = s * 1.525f;
			t /= 2f;
			if (t < 1) {
				return 0.5f * (t * t * ((s2 + 1) * t - s));
			}
			t -= 2f;
			return 0.5f * t * t * ((s2 + 1) * t + s) + 2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InElastic(float t) {
			double s = 1.70158;
			if (t == 0) return 0f;
			if (t == 1) return 1f;
			s = 0.3 / (2.0 * Math.PI) * Math.Asin(1.0);
			t -= 1f;
			return -(float) (Math.Pow(2, 10 * t) * Math.Sin((t - s) * (2.0 * Math.PI) / 0.3));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float OutElastic(float t) {
			double s = 1.70158;
			if (t == 0) return 0f;
			if (t == 1) return 1f;
			s = 0.3 / (2.0 * Math.PI) * Math.Asin(1.0);
			return (float) (Math.Pow(2, -10 * t) * Math.Sin((t - s) * (2.0 * Math.PI) / 0.3) + 1.0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InOutElastic(float t) {
			double s = 1.70158;
			if (t == 0) return 0f;
			if (t == 1) return 1f;
			double p = 0.3 * 1.5;
			t *= 2f;
			s = p / (2.0 * Math.PI) * Math.Asin(1.0);
			if (t < 1) {
				t -= 1f;
				return -0.5f * (float) (Math.Pow(2, 10 * t) * Math.Sin((t - s) * (2.0 * Math.PI) / p));
			}
			t -= 1f;
			return (float) (Math.Pow(2, -10 * t) * Math.Sin((t - s) * (2.0 * Math.PI) / p) * 0.5) + 1f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InBounce(float t) { return 1f - OutBounce(1f - t); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float OutBounce(float t) {
			if (t < 1f / 2.75f) {
				return 7.5625f * t * t;
			}
			if (t < 2f / 2.75f) {
				t -= 1.5f / 2.75f;
				return 7.5625f * t * t + 0.75f;
			}
			if (t < 2.5f / 2.75f) {
				t -= 2.25f / 2.75f;
				return 7.5625f * t * t + 0.9375f;
			}
			t -= 2.625f / 2.75f;
			return 7.5625f * t * t + 0.984375f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InOutBounce(float t) {
			if (t < 0.5f) {
				return InBounce(t * 2f) * 0.5f;
			}
			return OutBounce(t * 2f - 1f) * 0.5f + 0.5f;
		}
	}
}