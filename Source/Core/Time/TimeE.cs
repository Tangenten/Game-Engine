using System;
using System.Diagnostics;
using System.Linq;
using RavContainers;
using RavUtilities;

namespace RavEngine {
	public class TimeE : EngineCore {
		private Stopwatch stopWatch;

		private int avgFpsFrameCounter;
		private double avgFpsTimeCounter;
		private double maxFps;
		private double minFps;

		[ConsoleCommand("FPS")]
		public double Fps { get; private set; }
		[ConsoleCommand("AVERAGE_FPS")]
		public double AverageFps { get; private set; }
		[ConsoleCommand("PRINT_FPS")]
		internal bool PrintAverageFps { get; set; }

		public double DeltaGameTime { get; private set; }
		[ConsoleCommand("GAME_SCALAR")]
		public double DeltaGameTimeScalar { get; private set; }
		[ConsoleCommand("GAME_ELAPSED")]
		public double ElapsedGameTime { get; private set; }
		[ConsoleCommand("DELTA_TIME_MAX")]
		public double DeltaGameTimeMax {
			get => this.deltaGameTimeMax;
			set {
				this.deltaGameTimeMax = value;
				Engine.Settings.Set("DELTA_TIME_MAX", value);
			}
		}
		private double deltaGameTimeMax;
		private RingArray<double> deltaGameTimeSmooth;

		public double DeltaRealTime { get; private set; }
		[ConsoleCommand("REAL_ELAPSED")]
		public double ElapsedRealTime { get; private set; }
		[ConsoleCommand("FRAMES_ELAPSED")]
		public int ElapsedFrames { get; private set; }

		public double FixedTimeStep { get; private set; }
		public double FixedTimeStepAccumulate { get; private set; }
		public double FixedTimeStepInterpolate { get; private set; }
		public int FixedTimeStepUpdates { get; private set; }

		[ConsoleCommand("FIXED_UPDATES_PER_SECOND")]
		public int FixedUpdatesPerSecond {
			get => this.fixedUpdatesPerSecond;
			set {
				this.fixedUpdatesPerSecond = value;
				this.FixedTimeStep = 1.0 / this.FixedUpdatesPerSecond;
				Engine.Settings.Set("FIXED_UPDATES_PER_SECOND", value);
			}
		}
		private int fixedUpdatesPerSecond;

		public TimeE() {
			this.stopWatch = new Stopwatch();
			this.stopWatch.Start();
			this.DeltaGameTimeScalar = 1;

			if (Engine.Settings.TryGet("DELTA_TIME_MAX", out double max)) {
				this.deltaGameTimeMax = max;
			} else {
				this.DeltaGameTimeMax = 0.05;
			}

			this.PrintAverageFps = false;
			this.minFps = int.MaxValue;
			this.maxFps = 0;

			this.deltaGameTimeSmooth = new RingArray<double>(8);

			if (Engine.Settings.TryGet("FIXED_UPDATES_PER_SECOND", out int tps)) {
				this.fixedUpdatesPerSecond = tps;
				this.FixedTimeStep = 1.0 / this.FixedUpdatesPerSecond;
			} else {
				this.FixedUpdatesPerSecond = 64;
			}
		}

		internal override void Start() { }

		internal override void Stop() { }

		internal override void Update() {
			this.DeltaRealTime = this.stopWatch.Elapsed.TotalSeconds;
			this.stopWatch.Restart();

			this.ElapsedRealTime += this.DeltaRealTime;

			this.Fps = 1.0 / this.DeltaRealTime;

			this.DeltaGameTime = this.DeltaRealTime * this.DeltaGameTimeScalar;
			this.DeltaGameTime = Math.Clamp(this.DeltaGameTime, double.MinValue, this.DeltaGameTimeMax);
			this.deltaGameTimeSmooth.PushData(this.DeltaGameTime);

			double[] deltaGameTimeArray = this.deltaGameTimeSmooth.GetAllData();
			this.DeltaGameTime = deltaGameTimeArray.Sum() / deltaGameTimeArray.Length;

			this.ElapsedGameTime += this.DeltaGameTime;
			this.ElapsedFrames += 1;

			this.FixedTimeStepAccumulate += Engine.Time.DeltaGameTime;
			this.FixedTimeStepUpdates = 0;

			while (this.FixedTimeStepAccumulate >= this.FixedTimeStep) {
				this.FixedTimeStepAccumulate -= this.FixedTimeStep;
				this.FixedTimeStepUpdates++;
			}

			this.FixedTimeStepInterpolate = TweenU.Linear(this.FixedTimeStepAccumulate, 0.0, this.FixedTimeStep, 0.0, 1.0);

			this.CalculateAverageFPS();
		}

		internal override void Reset() {
			this.ElapsedFrames = 0;
			this.ElapsedRealTime = 0;
			this.DeltaGameTimeScalar = 1;
			this.ElapsedGameTime = 0;
		}

		[Conditional("DEBUG")]
		private void CalculateAverageFPS() {
			if (this.Fps < this.minFps) {
				this.minFps = this.Fps;
			}
			if (this.Fps > this.maxFps) {
				this.maxFps = this.Fps;
			}

			this.avgFpsTimeCounter += this.DeltaRealTime;
			this.avgFpsFrameCounter++;
			if (this.avgFpsTimeCounter >= 1) {
				this.AverageFps = this.avgFpsFrameCounter / this.avgFpsTimeCounter;

				if (this.PrintAverageFps) {
					Engine.Editor.Console.WriteLine(ConsoleEntry.Debug($"- AVG FPS: {this.AverageFps:#.00} - MIN FPS: {this.minFps:#.00} - MAX FPS: {this.maxFps:#.00} -"));
				}

				this.avgFpsTimeCounter = 0;
				this.avgFpsFrameCounter = 0;
				this.minFps = int.MaxValue;
				this.maxFps = 0;
			}
		}

		public TimeStamp GetTimeStamp() {
			return new TimeStamp(this.ElapsedFrames, this.ElapsedGameTime, this.ElapsedRealTime);
		}
	}

	public class TimeStamp {
		private int elapsedFrames;
		private double elapsedGameTime;
		private double elapsedRealTime;

		public TimeStamp() {
			this.elapsedFrames = 0;
			this.elapsedGameTime = 0;
			this.elapsedRealTime = 0;
		}

		public TimeStamp(int elapsedFrames, double elapsedGameTime, double elapsedRealTime) {
			this.elapsedFrames = elapsedFrames;
			this.elapsedGameTime = elapsedGameTime;
			this.elapsedRealTime = elapsedRealTime;
		}

		public int FrameStamp() { return this.elapsedFrames; }

		public double GameTimeStamp() { return this.elapsedGameTime; }

		public double RealTimeStamp() { return this.elapsedRealTime; }

		public int FramesSinceStamped() { return Engine.Time.ElapsedFrames - this.elapsedFrames; }

		public double GameTimeSinceStamped() { return Engine.Time.ElapsedGameTime - this.elapsedGameTime; }

		public double RealTimeSinceStamp() { return Engine.Time.ElapsedRealTime - this.elapsedRealTime; }

		public double RealTimeBetweenStamps(TimeStamp stamp) { return stamp.elapsedRealTime - this.elapsedRealTime; }
	}
}
