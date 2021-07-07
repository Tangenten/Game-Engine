using System;
using System.Diagnostics;
using System.Linq;
using RavContainers;

namespace RavEngine {
	public class TimeE : EngineCore {
		private Stopwatch stopWatch;

		private int avgFpsFrameCounter;
		private double avgFpsTimeCounter;
		private double maxFps;
		private double minFps;

		public double Fps { get; private set; }

		public double AverageFps { get; private set; }
		internal bool PrintAverageFps { get; set; }

		public double DeltaGameTime { get; private set; }
		public double DeltaGameTimeScalar { get; private set; }
		public double ElapsedGameTime { get; private set; }
		public double DeltaGameTimeMax { get; set; }
		public RingArray<double> deltaGameTimeSmooth;

		public int TicksPerSeconds { get; private set; }
		public double FixedTimeStep { get; private set; }
		public double FixedTimeStepAccumulate { get; private set; }
		public double FixedTimeStepInterpolate { get; private set; }

		public double DeltaRealTime { get; private set; }
		public double ElapsedRealTime { get; private set; }
		public int ElapsedFrames { get; private set; }

		public TimeE() {
		}

		internal override void Start() {
			this.stopWatch = new Stopwatch();
			this.stopWatch.Start();
			this.DeltaGameTimeScalar = 1;
			this.DeltaGameTimeMax = 0.05f;

			this.SetTicksPerSeconds(64);

			this.PrintAverageFps = false;
			this.minFps = int.MaxValue;
			this.maxFps = 0;

			this.deltaGameTimeSmooth = new RingArray<double>(8);
		}

		internal override void Stop() {
		}

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

			this.FixedTimeStepAccumulate += this.DeltaGameTime;

			while (this.FixedTimeStepAccumulate >= this.FixedTimeStep) {
				// Update(this.FixedTimeStep)
				this.FixedTimeStepAccumulate -= this.FixedTimeStep;
				// this.FixedTimeStepAccumulate += this.stopWatch.Restart();
			}

			this.FixedTimeStepInterpolate = RavUtilities.TweenU.Linear(this.FixedTimeStepAccumulate, 0.0, this.FixedTimeStep, 0.0, 1.0);
			// Render(this.FixedTimeStepInterpolate)

			this.CalculateAverageFPS();
		}

		internal override void Reset() {
			this.ElapsedFrames = 0;
			this.ElapsedRealTime = 0;
			this.DeltaGameTimeScalar = 1;
			this.ElapsedGameTime = 0;
		}

		public void SetTicksPerSeconds(int tps) {
			this.TicksPerSeconds = tps;
			this.FixedTimeStep = 1.0 / this.TicksPerSeconds;
		}

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
					Engine.Editor.Console.WriteToOutput(ConsoleEntry.Debug($"- AVG FPS: {this.AverageFps:#.00} - MIN FPS: {this.minFps:#.00} - MAX FPS: {this.maxFps:#.00} -"));
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

	public struct TimeStamp {
		private int elapsedFrames;
		private double elapsedGameTime;
		private double elapsedRealTime;

		public TimeStamp(int elapsedFrames, double elapsedGameTime, double elapsedRealTime) {
			this.elapsedFrames = elapsedFrames;
			this.elapsedGameTime = elapsedGameTime;
			this.elapsedRealTime = elapsedRealTime;
		}

		public int FramesSinceStamped() {
			return Engine.Time.ElapsedFrames - this.elapsedFrames;
		}

		public double GameTimeSinceStamped() {
			return Engine.Time.ElapsedGameTime - this.elapsedGameTime;
		}

		public double RealTimeSinceStamp() {
			return Engine.Time.ElapsedRealTime - this.elapsedRealTime;
		}
	}
}
