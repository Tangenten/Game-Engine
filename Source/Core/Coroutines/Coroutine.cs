using System;
using System.Collections;

namespace RavEngine {
	public class Coroutine {
		public bool Finished { get; private set; }
		public uint TicksPerFrame { get; set; }
		private Action? onFinished;
		private IEnumerator routine;

		public string RoutineName => this.routine.ToString()!;

		public Coroutine(IEnumerator routine, Action onFinished = null, uint ticksPerFrame = 1) {
			this.routine = routine;
			this.Finished = false;
			this.TicksPerFrame = ticksPerFrame;
			this.onFinished = onFinished;
		}

		internal void Tick() {
			for (int i = 0; i < this.TicksPerFrame && !this.Finished; i++) {
				if (!this.routine.MoveNext()) this.Finish();
			}
		}

		private void Finish() {
			if (!this.Finished) {
				this.Finished = true;
				this.onFinished?.Invoke();
			}
		}

		public static IEnumerator WaitGameSeconds(float seconds) {
			double stopAt = Engine.Time.ElapsedGameTime + seconds;
			while (Engine.Time.ElapsedGameTime <= stopAt) {
				yield return null;
			}
		}

		public static IEnumerator WaitSeconds(float seconds) {
			double stopAt = Engine.Time.ElapsedRealTime + seconds;
			while (Engine.Time.ElapsedRealTime <= stopAt) {
				yield return null;
			}
		}

		public static IEnumerator WaitFrames(int frames) {
			double stopAt = Engine.Time.ElapsedFrames + frames;
			while (Engine.Time.ElapsedFrames <= stopAt) {
				yield return null;
			}
		}

		public static IEnumerator AfterCondition(Func<bool> after) {
			if (after.Invoke()) {
				yield return null;
			}
		}
	}
}