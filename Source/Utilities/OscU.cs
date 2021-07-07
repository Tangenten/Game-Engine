using System;
using System.Runtime.CompilerServices;

namespace RavUtilities {
	public static class OscU {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SinOsc(float minAmp, float maxAmp, float frequency, float phase, float increment) {
			return (MathF.Sin((increment + phase * MathF.PI) * frequency) + 1f) / 2f * (maxAmp - minAmp) + minAmp;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float PulseOsc(float minAmp, float maxAmp, float frequency, float phase, float increment, float pulse = 0.5f) {
			float val = SinOsc(-1f, 1f, frequency, phase, increment);
			if (val < -1f + pulse * 2f) {
				val = -1f;
			} else {
				val = 1f;
			}

			return val * (maxAmp - minAmp) + minAmp;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float TriangleOsc(float minAmp, float maxAmp, float frequency, float phase, float increment) {
			phase = MathU.Mod(phase + increment, (MathF.PI * 2f));
			float value = -1f + (2f * phase / (MathF.PI * 2f));

			return (2f * (MathF.Abs(value) - 0.5f)) * (maxAmp - minAmp) + minAmp;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SawOsc(float minAmp, float maxAmp, float frequency, float phase, float increment) {
			phase = MathU.Mod(phase + increment, (MathF.PI * 2f));
			float value = -1f + (2f * phase / (MathF.PI * 2f));

			return value * (maxAmp - minAmp) + minAmp;
		}
	}
}
