using System;
using System.Diagnostics;
using System.Threading;

namespace RavUtilities {
	public static class TimerU {
		private static ThreadLocal<Stopwatch> stopwatch = new ThreadLocal<Stopwatch>(() => new Stopwatch());

		private static int invocationCount;
		private static double accumulatedTime;
		private static double minTime;
		private static double maxTime;

		public static void StartTimer() {
			invocationCount = 0;
			accumulatedTime = 0;
			minTime = double.MaxValue;
			maxTime = double.MinValue;

			stopwatch.Value.Start();
		}

		public static void Invoke() {
			invocationCount++;
			double deltaTime = stopwatch.Value.Elapsed.TotalSeconds - accumulatedTime;
			accumulatedTime += deltaTime;

			if (deltaTime < minTime) {
				minTime = deltaTime;
			}

			if (deltaTime > maxTime) {
				maxTime = deltaTime;
			}
		}

		public static double StopTimer() {
			double totalTime = 0;

			if (stopwatch.Value.IsRunning) {
				stopwatch.Value.Stop();
				totalTime = stopwatch.Value.Elapsed.TotalSeconds;

				stopwatch.Value.Reset();
			} else {
				Console.WriteLine("Stopwatch not running");
			}

			return totalTime;
		}

		public static double RestartTimer() {
			double totalTime = 0;

			if (stopwatch.Value.IsRunning) {
				stopwatch.Value.Stop();
				totalTime = stopwatch.Value.Elapsed.TotalSeconds;

				stopwatch.Value.Reset();

				invocationCount = 0;
				accumulatedTime = 0;
				minTime = double.MaxValue;
				maxTime = double.MinValue;

				stopwatch.Value.Start();
			} else {
				Console.WriteLine("Stopwatch not running");
			}

			return totalTime;
		}

		public static void PrintTimer(string prefix = "") {
			double totalTime = stopwatch.Value.Elapsed.TotalSeconds;

			if (stopwatch.Value.IsRunning) {
				if (invocationCount > 0) {
					double averageTime = totalTime / invocationCount;
					Console.WriteLine($"{prefix} - Min: {minTime:#.0000} Max: {maxTime:#.0000} Average: {averageTime:#.0000} Total: {totalTime:#.0000} - ");
				} else {
					Console.WriteLine($"{prefix} - Total: {totalTime:#.0000} - ");
				}
			} else {
				Console.WriteLine("Stopwatch not running");
			}
		}
	}
}
