using System;
using System.Diagnostics;
using System.Threading;
using RavUtilities;
using Silk.NET.OpenAL;

namespace RavEngine {
	public class AudioE : EngineCore {
		private int bufferSize;
		[ConsoleCommand("BUFFER_SIZE")]
		public int BufferSize {
			get => this.bufferSize;
			private set {
				Engine.Settings.Set("BUFFER_SIZE", value);
				this.bufferSize = value;
			}
		}
		[ConsoleCommand("SAMPLE_RATE")]
		public int SampleRate { get; private set; }
		[ConsoleCommand("CHANNELS")]
		public int Channels { get; private set; }
		[ConsoleCommand("BUFFER_SIZE_SECONDS")]
		public double BufferSizeInSeconds {
			get => this.BufferSize / (double) this.SampleRate;
			private set => this.BufferSize = (int) (this.SampleRate * value);
		}
		[ConsoleCommand("BUFFER_SIZE_HZ")]
		public double BufferSizeInHz {
			get => 1.0 / this.BufferSizeInSeconds;
			set {
				if (value >= 50) {
					Engine.Editor.Console.WriteLine(ConsoleEntry.Debug("Max 50 Hz"));
					return;
				} // Max 50 Hertz
				this.BufferSize = (int) (1.0 / value * this.SampleRate);
			}
		}

		public delegate void AudioCallback(ref Audio audio);
		public event AudioCallback Callback;

		private unsafe Device* device;
		private unsafe Context* context;

		public ALContext ALC { get; }
		public AL AL { get; }

		private Thread thread;
		private bool threadRunning;

		public unsafe AudioE() {
			this.ALC = ALContext.GetApi(true);
			this.AL = AL.GetApi(true);
			this.device = this.ALC.OpenDevice("");
			this.context = this.ALC.CreateContext(this.device, null);
			this.ALC.MakeContextCurrent(this.context);
			this.ALC.ProcessContext(this.context);

			int attributeSize = 0;
			this.ALC.GetContextProperty(this.device, GetContextInteger.AttributesSize, 1, &attributeSize);
			int[] allContextAttributes = new int[attributeSize];
			fixed (int* ctx = allContextAttributes) {
				this.ALC.GetContextProperty(this.device, GetContextInteger.AllAttributes, 32 * 32, ctx);

				for (int i = 0; i < allContextAttributes.Length - 1; i++) {
					switch (allContextAttributes[i]) {
						case (int) ContextAttributes.Frequency:
							this.SampleRate = allContextAttributes[i + 1];
							break;
					}
				}
			}

			this.Channels = 2;
			if (Engine.Settings.TryGet("BUFFER_SIZE", out int bufferSize)) {
				this.bufferSize = bufferSize;
			} else {
				this.BufferSizeInHz = 35;
			}

			this.CheckError();

			this.thread = new Thread(this.AudioThread);
			this.thread.Name = "Audio Thread";
			this.thread.Priority = ThreadPriority.Highest;
		}

		internal override void Start() => this.StartThread();

		internal override void Stop() => this.StopThread();

		internal override void Update() { }

		internal override void Reset() {
			lock (this.thread) {
				this.Callback = null;
			}
		}

		[Conditional("DEBUG")]
		internal unsafe void CheckError() {
			AudioError audioError = this.AL.GetError();
			if (audioError != AudioError.NoError) {
				Console.WriteLine($"AudioError : {audioError}");
				Engine.Editor?.Console.WriteLine(ConsoleEntry.Error($"AudioError : {audioError}"));
			}

			ContextError contextError = this.ALC.GetError(this.device);
			if (contextError != ContextError.NoError) {
				Console.WriteLine($"ContextError : {contextError}");
				Engine.Editor?.Console.WriteLine(ConsoleEntry.Error($"ContextError : {contextError}"));
			}
		}

		internal void StartThread() => this.thread.Start();

		internal void StopThread() => this.threadRunning = false;

		internal unsafe void AudioThread() {
			uint source = this.AL.GenSource();
			this.AL.SetSourceProperty(source, SourceInteger.SourceType, (int) SourceType.Streaming);
			uint[] buffer = { 0 };
			Audio audioBuffer = new Audio(Engine.Audio.SampleRate, Engine.Audio.BufferSize, (ChannelMode) Engine.Audio.Channels);
			short[] audioBufferShorts = new short[Engine.Audio.BufferSize * Engine.Audio.Channels];
			Stopwatch stopwatch = new Stopwatch();

			this.threadRunning = true;
			while (this.threadRunning) {
				this.AL.SourceUnqueueBuffers(source, buffer);
				this.AL.GetSourceProperty(source, GetSourceInteger.BuffersQueued, out int buffersQueued);

				if (buffersQueued < 2) {
					if (buffersQueued == 0) {
						Console.WriteLine("Audio Underrun: No Buffers Queued");
					}
					stopwatch.Restart();

					lock (this.thread) {
						this.BufferResizeOrZero(ref audioBuffer);
						this.Callback?.Invoke(ref audioBuffer);
					}

					if (Engine.Audio.BufferSize * Engine.Audio.Channels != audioBufferShorts.Length) {
						audioBufferShorts = new short[Engine.Audio.BufferSize * Engine.Audio.Channels];
					} else {
						audioBufferShorts.FillVectorized((short) 0);
					}
					for (int i = 0; i < audioBufferShorts.Length; i += Engine.Audio.Channels) {
						for (int j = 0; j < audioBuffer.ChannelCount; j++) {
							audioBufferShorts[i + j] = (short) (audioBuffer[j][i / Engine.Audio.Channels] * short.MaxValue);
						}
					}

					this.AL.DeleteBuffer(buffer[0]);
					buffer[0] = this.AL.GenBuffer();
					this.AL.BufferData(buffer[0], BufferFormat.Stereo16, audioBufferShorts, Engine.Audio.SampleRate);
					this.AL.SourceQueueBuffers(source, buffer);

					this.AL.GetSourceProperty(source, GetSourceInteger.SourceState, out int sourceState);
					if (sourceState != (int) SourceState.Playing) {
						this.AL.SourcePlay(source);
						Console.WriteLine("Audio Underrun: Source Not Playing");
					}

					double msToSleep = Engine.Audio.BufferSizeInSeconds - stopwatch.Elapsed.TotalSeconds;
					Thread.Sleep((int) Math.Clamp(msToSleep, 1, (Engine.Audio.BufferSizeInSeconds * 1000) - 1));
				}
			}

			this.AL.DeleteSource(source);
			this.ALC.MakeContextCurrent(null);
			this.ALC.CloseDevice(this.device);
			this.ALC.DestroyContext(this.context);
		}

		public void BufferResizeOrZero(ref Audio audio) {
			for (int i = 0; i < audio.ChannelCount; i++) {
				if (Engine.Audio.BufferSize != audio[i].SamplesLength) {
					audio[i] = new Channel(new float[Engine.Audio.BufferSize]);
				} else {
					audio[i].Samples.FillVectorized(0f);
				}
			}
		}
	}
}
