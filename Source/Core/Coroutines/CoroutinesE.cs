using System.Collections.Generic;

namespace RavEngine {
	public class CoroutinesE : EngineCore {
		private List<Coroutine> coroutines;

		public CoroutinesE() {
		}

		internal override void Start() {
			this.coroutines = new List<Coroutine>();
		}

		internal override void Stop() {
		}

		internal override void Update() {
			this.TickCoroutines();
		}

		internal override void Reset() {
			this.coroutines.Clear();
		}

		public void AddCoroutine(Coroutine coroutine) {
			this.coroutines.Add(coroutine);
		}

		public void RemoveCoroutine(Coroutine coroutine) {
			this.coroutines.Remove(coroutine);
		}

		private void TickCoroutines() {
			for (int i = this.coroutines.Count - 1; i >= 0; i--) {
				if (this.coroutines[i].Finished) {
					this.RemoveCoroutine(this.coroutines[i]);
				} else {
					this.coroutines[i].Tick();
				}
			}
		}
	}
}
