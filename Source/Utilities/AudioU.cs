using System;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Text;

namespace RavUtilities {
	public class AudioU {

		public struct AudioFile {
			public float[] samples;
			public int channels;
			public int sampleRate;
		}

		public AudioFile OpenWavFile(Stream stream) {
			BinaryReader binaryReader = new BinaryReader(stream, Encoding.Default, true);

			// RIFF Header
			string ChunkID = System.Text.Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
			if (ChunkID != "RIFF") {
				throw new Exception("Corrupted File");
			}

			int ChunkSize = binaryReader.ReadInt32();
			string Format = System.Text.Encoding.ASCII.GetString(binaryReader.ReadBytes(4));

			// FMT Chunk
			string Subchunk1ID = System.Text.Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
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

			// DATA Chunk
			string Subchunk2ID = System.Text.Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
			if (Subchunk2ID != "data") {
				throw new Exception("Corrupted File");
			}

			int Subchunk2Size = binaryReader.ReadInt32();
			byte[] Data = binaryReader.ReadBytes(Subchunk2Size);

			AudioFile audioFile = new AudioFile();
			audioFile.channels = NumChannels;
			audioFile.sampleRate = SampleRate;
			audioFile.samples = new float[Data.Length / 4];

			if (BitsPerSample == 16) {
				for(int i = 0; i < audioFile.samples.Length; i++) {
					short sampleInt16 = BitConverter.ToInt16(Data, i * 2);
					float sampleFP32 = TweenU.Linear(sampleInt16, short.MinValue, short.MaxValue, -1f, 1f);
					audioFile.samples[i] = sampleFP32;
				}
			}else if (BitsPerSample == 24) {
				int Int24ToInt32(byte b1, byte b2, byte b3) {
					int r = 0;
					byte b0 = 0xff;

					if ((b1 & 0x80) != 0) r |= b0 << 24;
					r |= b1 << 16;
					r |= b2 << 8;
					r |= b3;
					return r;
				}

				for(int i = 0; i < audioFile.samples.Length; i++) {
					int sampleInt24 = Int24ToInt32(Data[(i + 1) * 3], Data[(i + 1) * 3], Data[(i + 2) * 3]);
					float sampleFP32 = TweenU.Linear(sampleInt24, -8388608f, 8388607f, -1f, 1f);
					audioFile.samples[i] = sampleFP32;
				}
			} else if (BitsPerSample == 32) {
				for(int i = 0; i < audioFile.samples.Length; i++) {
					int sampleInt32 = BitConverter.ToInt32(Data, i * 4);
					float sampleFP32 = TweenU.Linear(sampleInt32, int.MinValue, int.MaxValue, -1f, 1f);
					audioFile.samples[i] = sampleFP32;
				}
			} else {
				throw new Exception("Unsupported Wav Bit Depth");
			}

			return audioFile;
		}

		public float[] MonoToStereo(in float[] monoSamples) {
			float[] stereoSamples = new float[monoSamples.Length * 2];

			for (int i = 0; i < stereoSamples.Length; i += 2) {
				float stereoSample = monoSamples[i / 2];
				stereoSamples[i + 0] = stereoSample;
				stereoSamples[i + 1] = stereoSample;
			}

			return stereoSamples;
		}

		public float[] StereoToMono(in float[] stereoSamples) {
			float[] monoSamples = new float[stereoSamples.Length / 2];

			for (int i = 0; i < stereoSamples.Length; i += 2) {
				float monoSample = (stereoSamples[i] + stereoSamples[i + 1]) / 2;
				monoSamples[i / 2] = monoSample;
			}

			return monoSamples;
		}

		public float[] ResampleMono(in float[] samples, int from, int to) {
			double resampleRatio = (double) to / (double) from;
			double resampleRatio2 = (double) from / (double) to;
			float[] resampledSamples = new float[(int) (samples.Length * resampleRatio)];

			int iter = 0;
			for (double i = 0; i < samples.Length; i += resampleRatio2) {
				int floor = (int) i;
				int ceiling = (int) i + 1;
				float interpolate = (float) (i - floor);

				resampledSamples[iter++] = TweenU.Linear(samples[floor], samples[ceiling], interpolate);
			}

			return resampledSamples;
		}

		public float[] ResampleStereo(in float[] samples, int from, int to) {
			double resampleRatio = (double) to / (double) from;
			double resampleRatio2 = (double) from / (double) to;

			// Bitflip to Force int to be even (stereo)
			int size = (int) (samples.Length * resampleRatio);
			size &= ~1;
			float[] resampledSamples = new float[size];

			int iter = 0;
			for (double i = 0; i < samples.Length; i += 2 + resampleRatio2) {
				int floor = (int) i;
				int ceiling = (int) i + 1;
				float interpolate = (float) (i - floor);

				resampledSamples[iter++] = TweenU.Linear(samples[floor + 0], samples[ceiling + 0], interpolate);
				resampledSamples[iter++] = TweenU.Linear(samples[floor + 1], samples[ceiling + 1], interpolate);
			}

			return resampledSamples;
		}
	}
}
