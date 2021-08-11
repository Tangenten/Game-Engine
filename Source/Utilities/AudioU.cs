using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace RavUtilities {
	public static class AudioU {
		private delegate float PCMToFloat(Span<byte> bytes);

		// http://soundfile.sapp.org/doc/WaveFormat/
		public static Audio OpenWavFile(Stream stream, int channels = -1, int sampleRate = -1) {
			BinaryReader binaryReader = new BinaryReader(stream);
			binaryReader.BaseStream.Position = 0;

			// RIFF Header
			string ChunkID = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
			if (ChunkID != "RIFF") {
				throw new Exception("Corrupted File");
			}
			int ChunkSize = binaryReader.ReadInt32();
			string Format = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));

			// FMT Chunk
			string Subchunk1ID = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
			if (Subchunk1ID != "fmt ") {
				throw new Exception("Corrupted File");
			}
			int Subchunk1Size = binaryReader.ReadInt32();
			short AudioFormat = binaryReader.ReadInt16();
			if (AudioFormat != 1) {
				throw new Exception("Not PCM Format");
			}
			short NumChannels = binaryReader.ReadInt16();
			int SampleRate = binaryReader.ReadInt32();
			int ByteRate = binaryReader.ReadInt32();
			short BlockAlign = binaryReader.ReadInt16();
			short BitsPerSample = binaryReader.ReadInt16();
			int BytesPerSample = BitsPerSample / 8;

			// DATA Chunk
			string Subchunk2ID = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
			if (Subchunk2ID != "data") {
				throw new Exception("Corrupted File");
			}
			int Subchunk2Size = binaryReader.ReadInt32();

			int SamplesPerChannel = Subchunk2Size / BytesPerSample / NumChannels / NumChannels;

			if (channels == -1) {
				channels = NumChannels;
			}

			if (sampleRate == -1) {
				sampleRate = SampleRate;
			}

			Audio audio = new Audio(sampleRate, SamplesPerChannel, (ChannelMode) channels);

			PCMToFloat PCMConvert = null;
			if (BitsPerSample == 16) {
				PCMConvert = PCM16ToFloat;
			} else if (BitsPerSample == 24) {
				PCMConvert = PCM24ToFloat;
			} else if (BitsPerSample == 32) {
				PCMConvert = PCM32ToFloat;
			} else {
				throw new Exception("Unsupported Bit Depth");
			}

			Span<byte> Data = binaryReader.ReadBytes(Subchunk2Size);

			int iter = 0;
			if (NumChannels == 2 && audio.ChannelCount == 2) {
				for (int i = 0; i < audio.SamplesLength; i++) {
					for (int j = 0; j < audio.ChannelCount; j++) {
						audio[j][i] = PCMConvert(Data.Slice(iter, iter + BytesPerSample));
						iter += BytesPerSample;
					}
				}
			} else if (NumChannels == 2 && audio.ChannelCount == 1) {
				for (int i = 0; i < audio.SamplesLength; i++) {
					for (int j = 0; j < audio.ChannelCount; j++) {
						float sample = PCMConvert(Data.Slice(iter, iter + BytesPerSample));
						iter += BytesPerSample;
						float sample2 = PCMConvert(Data.Slice(iter, iter + BytesPerSample));
						iter += BytesPerSample;
						audio[j][i] = (sample + sample2) / 2f;
					}
				}
			} else if (NumChannels == 1 && audio.ChannelCount == 2) {
				for (int i = 0; i < audio.SamplesLength; i++) {
					float sample = PCMConvert(Data.Slice(iter, iter + BytesPerSample));
					iter += BytesPerSample;
					for (int j = 0; j < audio.ChannelCount; j++) {
						audio[j][i] = sample;
					}
				}
			} else {
				throw new Exception("Unsupported Channel Configuration");
			}

			if (SampleRate != audio.SampleRate) {
				Resample(ref audio, SampleRate, audio.SampleRate);
			}

			return audio;
		}

		public static void Resample(ref Audio audio, int sampleRateFrom, int sampleRateTo) {
			if (sampleRateFrom == sampleRateTo) {
				return;
			}

			uint GCD = MathU.GCD((uint) sampleRateFrom, (uint) sampleRateTo);
			int L = (int) (sampleRateTo / GCD);
			int M = (int) (sampleRateFrom / GCD);

			if (GCD == sampleRateFrom || GCD == sampleRateTo) {
				//LowPassFIRFilter(ref audio, Math.Min(sampleRateFrom, sampleRateTo) / 2, 4000, sampleRateFrom);
				CosineInterpolation(ref audio, sampleRateFrom, sampleRateTo);
			} else {
				//LowPassFIRFilter(ref audio, Math.Min(sampleRateFrom, sampleRateTo) / 2, 4000, sampleRateFrom);
				CosineInterpolation(ref audio, sampleRateFrom, sampleRateTo);

				//Upsample(ref audio, L, sampleRateFrom, sampleRateTo);
				//LowPassFIRFilter(ref audio, Math.Max(L, M), 2500, sampleRateFrom);
				//Downsample(ref audio, M, sampleRateFrom, sampleRateTo);
			}

			audio.SampleRate = sampleRateTo;
		}

		public static void Downsample(ref Audio audio, int factor, int sampleRateFrom, int sampleRateTo) {
			if (sampleRateFrom == sampleRateTo) {
				return;
			}

			for (int i = 0; i < audio.ChannelCount; i++) {
				ref Channel channel = ref audio[i];
				int size = (int) (channel.SamplesLength / (float) factor);
				float[] downsampled = new float[size];

				for (int j = 0; j < downsampled.Length; j++) {
					downsampled[j] = channel[j * factor];
				}

				channel = new Channel(downsampled);
			}

			audio.SampleRate = sampleRateTo;
		}

		public static void Upsample(ref Audio audio, int factor, int sampleRateFrom, int sampleRateTo) {
			if (sampleRateFrom == sampleRateTo) {
				return;
			}

			for (int i = 0; i < audio.ChannelCount; i++) {
				ref Channel channel = ref audio[i];
				int size = channel.SamplesLength * factor;
				float[] upsampled = new float[size];

				for (int j = 0; j < upsampled.Length; j += factor) {
					upsampled[j] = channel[j / factor];
					for (int k = j + 1; k < j + 1 + (factor - 1); k++) {
						upsampled[k] = 0f;
					}
				}

				channel = new Channel(upsampled);
			}

			audio.SampleRate = sampleRateTo;
		}

		public static void LinearInterpolation(ref Audio audio, int sampleRateFrom, int sampleRateTo) {
			if (sampleRateFrom == sampleRateTo) {
				return;
			}

			double resampleRatio = sampleRateTo / (double) sampleRateFrom;
			double resampleRatio2 = sampleRateFrom / (double) sampleRateTo;

			for (int i = 0; i < audio.ChannelCount; i++) {
				ref Channel channel = ref audio[i];
				int size = (int) (channel.SamplesLength * resampleRatio);
				float[] resampledSamples = new float[size];

				double iter = 0;
				for (int j = 0; j < resampledSamples.Length - 1; j++) {
					int floor = (int) iter;
					int ceiling = (int) iter + 1;
					double interpolate = iter - (int) iter;
					iter += resampleRatio2;

					resampledSamples[j] = TweenU.Linear(channel[floor], channel[ceiling], (float) interpolate);
				}
				resampledSamples[^1] = channel[channel.SamplesLength - 1];

				channel = new Channel(resampledSamples);
			}

			audio.SampleRate = sampleRateTo;
		}

		public static void CosineInterpolation(ref Audio audio, int sampleRateFrom, int sampleRateTo) {
			if (sampleRateFrom == sampleRateTo) {
				return;
			}

			double resampleRatio = sampleRateTo / (double) sampleRateFrom;
			double resampleRatio2 = sampleRateFrom / (double) sampleRateTo;

			for (int i = 0; i < audio.ChannelCount; i++) {
				ref Channel channel = ref audio[i];
				int size = (int) (channel.SamplesLength * resampleRatio);
				float[] resampledSamples = new float[size];

				double iter = 0;
				for (int j = 0; j < resampledSamples.Length - 1; j++) {
					int floor = (int) iter;
					int ceiling = (int) iter + 1;
					double interpolate = iter - (int) iter;
					iter += resampleRatio2;

					resampledSamples[j] = TweenU.Cosine(channel[floor], channel[ceiling], (float) interpolate);
				}
				resampledSamples[^1] = channel[channel.SamplesLength - 1];

				channel = new Channel(resampledSamples);
			}

			audio.SampleRate = sampleRateTo;
		}

		public static void CubicInterpolation(ref Audio audio, int sampleRateFrom, int sampleRateTo) {
			if (sampleRateFrom == sampleRateTo) {
				return;
			}

			double resampleRatio = sampleRateTo / (double) sampleRateFrom;
			double resampleRatio2 = sampleRateFrom / (double) sampleRateTo;

			for (int i = 0; i < audio.ChannelCount; i++) {
				ref Channel channel = ref audio[i];
				int size = (int) (channel.SamplesLength * resampleRatio);
				float[] resampledSamples = new float[size];

				for (int j = 0; j < 2; j++) {
					resampledSamples[j] = channel[j];
					resampledSamples[resampledSamples.Length - 1 - j] = channel[channel.SamplesLength - 1 - j];
				}

				double iter = 2;
				for (int j = 2; j < resampledSamples.Length - 2; j++) {
					int left2 = (int) iter - 2;
					int left1 = (int) iter - 1;
					int right1 = (int) iter + 1;
					int right2 = (int) iter + 2;
					double interpolate = iter - (int) iter;
					iter += resampleRatio2;

					resampledSamples[j] = TweenU.Cubic(channel[left2], channel[left1], channel[right1], channel[right2], (float) interpolate);
				}

				channel = new Channel(resampledSamples);
			}

			audio.SampleRate = sampleRateTo;
		}

		public static void CubicHermiteInterpolation(ref Audio audio, int sampleRateFrom, int sampleRateTo) {
			if (sampleRateFrom == sampleRateTo) {
				return;
			}

			double resampleRatio = sampleRateTo / (double) sampleRateFrom;
			double resampleRatio2 = sampleRateFrom / (double) sampleRateTo;

			for (int i = 0; i < audio.ChannelCount; i++) {
				ref Channel channel = ref audio[i];
				int size = (int) (channel.SamplesLength * resampleRatio);
				float[] resampledSamples = new float[size];

				for (int j = 0; j < 2; j++) {
					resampledSamples[j] = channel[j];
					resampledSamples[resampledSamples.Length - 1 - j] = channel[channel.SamplesLength - 1 - j];
				}

				double iter = 2;
				for (int j = 2; j < resampledSamples.Length - 2; j++) {
					int left2 = (int) iter - 2;
					int left1 = (int) iter - 1;
					int right1 = (int) iter + 1;
					int right2 = (int) iter + 2;
					double interpolate = iter - (int) iter;
					iter += resampleRatio2;

					resampledSamples[j] = TweenU.CubicHermite(channel[left2], channel[left1], channel[right1], channel[right2], (float) interpolate);
				}

				channel = new Channel(resampledSamples);
			}

			audio.SampleRate = sampleRateTo;
		}

		public static void LowPassFIRFilter(ref Audio audio, float cutoffFrequency, float bandwidth, int sampleRate) {
			float[] impulseResponse = GenerateLowPassImpulseResponse(cutoffFrequency, bandwidth, sampleRate);
			Convolve(ref audio, impulseResponse);
		}

		public static float[] GenerateLowPassImpulseResponse(float cutoffFrequency, float bandwidth, int sampleRate) {
			float cutoffFraction = cutoffFrequency / sampleRate;
			float bandwidthFraction = bandwidth / sampleRate;
			int impulseResponseLength = (int) (4f / bandwidthFraction);
			if (impulseResponseLength % 2 == 0) {
				impulseResponseLength++;
			}
			float[] window = GenerateBlackmanWindow(impulseResponseLength);
			float[] impulseResponse = new float[impulseResponseLength];

			float sum = 0f;
			for (int i = 0; i < impulseResponseLength; i++) {
				impulseResponse[i] = Sinc(2f * cutoffFraction * (i - (impulseResponseLength - 1) / 2)) * window[i];
				sum += impulseResponse[i];
			}

			for (int i = 0; i < impulseResponseLength; i++) {
				impulseResponse[i] /= sum;
			}

			return impulseResponse;
		}

		public static void Convolve(ref Audio audio, in float[] impulseResponse, bool fillEdges = true) {
			if (impulseResponse.Length >= audio.SamplesLength) {
				throw new Exception("Impulse Response Larger Than Samples");
			}

			int impulseLength = impulseResponse.Length;
			int impulseLengthHalf = impulseResponse.Length / 2;

			int start = impulseLengthHalf;
			int end = audio.SamplesLength - impulseLengthHalf;

			for (int i = 0; i < audio.ChannelCount; i++) {
				ref Channel channel = ref audio[i];
				float[] convoledSamples = new float[channel.SamplesLength];

				if (fillEdges) {
					for (int j = 0; j < impulseLengthHalf; j++) {
						convoledSamples[j] = channel[j];
						convoledSamples[convoledSamples.Length - 1 - j] = channel[channel.SamplesLength - 1 - j];
					}
				}

				for (int j = start; j < end; j++) {
					float sum = 0f;

					int k = 0;
					int simdLength = Vector<float>.Count;
					for (; k <= impulseLength - simdLength; k += simdLength) {
						Vector<float> vectorSection = new Vector<float>(channel.Samples, j + k - impulseLengthHalf);
						Vector<float> vectorSection2 = new Vector<float>(impulseResponse, k);

						sum += Vector.Dot(vectorSection * vectorSection2, Vector<float>.One);
					}

					for (; k < impulseLength; k++) {
						sum += channel[j + k - impulseLengthHalf] * impulseResponse[k];
					}

					convoledSamples[j] = sum;
				}

				channel = new Channel(convoledSamples);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float[] GenerateBlackmanWindow(int size) {
			float[] blackmanWindow = new float[size];
			for (int i = 0; i < size; i++) {
				blackmanWindow[i] = 0.42f - 0.5f * MathF.Cos(2f * MathF.PI * i / (size - 1)) + 0.08f * MathF.Cos(4 * MathF.PI * i / (size - 1));
			}
			return blackmanWindow;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float[] GenerateHanningWindow(int size) {
			float[] hanningWindow = new float[size];
			for (int i = 0; i < size; i++) {
				hanningWindow[i] = 0.5f * (1f * MathF.Cos(2f * MathF.PI * i / (size - 1)));
			}
			return hanningWindow;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sinc(float x) {
			if (x == 0f) {
				return 1f;
			}
			return MathF.Sin(MathF.PI * x) / (MathF.PI * x);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float PCM16ToFloat(Span<byte> bytes) { return BitConverter.ToInt16(bytes) / 32767f; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float PCM24ToFloat(Span<byte> bytes) { return ((bytes[0] << 8) | (bytes[1] << 16) | (bytes[2] << 24)) / 2147483648f; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float PCM32ToFloat(Span<byte> bytes) { return BitConverter.ToInt32(bytes) / 2147483648f; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte[] FloatToPCM16(float sample) { return BitConverter.GetBytes((short) (Math.Clamp(sample, -1f, 1f) * 32767f)); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte[] FloatToPCM24(float sample) {
			byte[] intBytes = BitConverter.GetBytes((int) (Math.Clamp(sample, -1f, 1f) * 2147483648f));
			return new[] { intBytes[0], intBytes[2], intBytes[3] };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte[] FloatToPCM32(float sample) { return BitConverter.GetBytes((int) (Math.Clamp(sample, -1f, 1f) * 2147483648f)); }

		public static short[] FloatsToShorts(in float[] samples) {
			short[] shorts = new short[samples.Length];

			for (int i = 0; i < samples.Length; i++) {
				shorts[i] = (short) (Math.Clamp(samples[i], -1f, 1f) * 32767f);
			}

			return shorts;
		}

		public static float[] ShortsToFloats(in short[] samples) {
			float[] floats = new float[samples.Length];

			for (int i = 0; i < samples.Length; i++) {
				floats[i] = samples[i] / 32767f;
			}

			return floats;
		}

		public static float[] MonoToStereo(in float[] monoSamples) {
			float[] stereoSamples = new float[monoSamples.Length * 2];

			for (int i = 0; i < stereoSamples.Length; i += 2) {
				float stereoSample = monoSamples[i / 2];
				stereoSamples[i + 0] = stereoSample;
				stereoSamples[i + 1] = stereoSample;
			}

			return stereoSamples;
		}

		public static float[] StereoToMono(in float[] stereoSamples) {
			float[] monoSamples = new float[stereoSamples.Length / 2];

			for (int i = 0; i < stereoSamples.Length; i += 2) {
				float monoSample = (stereoSamples[i] + stereoSamples[i + 1]) / 2;
				monoSamples[i / 2] = monoSample;
			}

			return monoSamples;
		}
	}

	public class Audio {
		public int SampleRate { get; set; }
		public ChannelMode ChannelMode { get; }
		public Channel[] Channels { get; }

		public int TotalSamplesLength => this.SamplesLength * this.ChannelCount;
		public int ChannelCount => (int) this.ChannelMode;
		public int SamplesLength => this[0].SamplesLength;

		public ref Channel this[int index] => ref this.Channels[index];

		public Audio(int sampleRate, int samplesLength, ChannelMode channelMode) {
			this.SampleRate = sampleRate;
			this.ChannelMode = channelMode;
			this.Channels = new Channel[(int) channelMode];
			for (int i = 0; i < this.ChannelCount; i++) {
				this.Channels[i] = new Channel(samplesLength);
			}
		}

		public Audio(int sampleRate, Channel[] channels) {
			this.SampleRate = sampleRate;
			this.ChannelMode = (ChannelMode) channels.Length;
			this.Channels = channels;
		}

		public Audio CreateMono() {
			Audio monoAudio = null;
			if (this.ChannelMode == ChannelMode.MONO) {
				monoAudio = new Audio(this.SampleRate, new[] { this.Channels[0] });
			} else {
				monoAudio = new Audio(this.SampleRate, new[] { new Channel(this.GetInterleaved()) });
			}

			return monoAudio;
		}

		public Audio CreateStereo() {
			Audio stereoAudio = null;
			if (this.ChannelMode == ChannelMode.MONO) {
				stereoAudio = new Audio(this.SampleRate, new[] { this.Channels[0], this.Channels[0] });
			} else {
				stereoAudio = new Audio(this.SampleRate, new[] { this.Channels[0], this.Channels[1] });
			}

			return stereoAudio;
		}

		public float[] GetInterleaved() {
			float[] samples = new float[this.TotalSamplesLength];

			switch (this.ChannelMode) {
				case ChannelMode.MONO:
					for (int i = 0; i < samples.Length; i += 2) {
						samples[i + 0] = this.Channels[0][i / 2];
						samples[i + 1] = this.Channels[0][i / 2];
					}
					break;
				case ChannelMode.STEREO:
					for (int i = 0; i < samples.Length; i += 2) {
						samples[i + 0] = this.Channels[0][i / 2];
						samples[i + 1] = this.Channels[1][i / 2];
					}
					break;
				default: throw new ArgumentOutOfRangeException();
			}

			return samples;
		}
	}

	public struct Channel {
		public float[] Samples { get; }
		public int SamplesLength => this.Samples.Length;

		public ref float this[int index] => ref this.Samples[index];

		public Channel(int samplesLength) { this.Samples = new float[samplesLength]; }

		public Channel(float[] samples) { this.Samples = samples; }
	}

	public enum ChannelMode {
		MONO = 1,
		STEREO = 2
	}
}