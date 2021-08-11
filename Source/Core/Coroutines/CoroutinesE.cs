using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RavEngine {
	public class CoroutinesE : EngineCore {
		private List<Coroutine> coroutines;

		public CoroutinesE() {
			this.coroutines = new List<Coroutine>();
		}

		internal override void Start() { }

		internal override void Stop() { }

		internal override void Update() => this.TickCoroutines();

		internal override void Reset() => this.coroutines.Clear();

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddCoroutine(Coroutine coroutine) => this.coroutines.Add(coroutine);

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void RemoveCoroutine(Coroutine coroutine) => this.coroutines.Remove(coroutine);

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void TickCoroutines() {
			for (int i = this.coroutines.Count - 1; i >= 0; i--) {
				if (this.coroutines[i].Finished) {
					this.RemoveCoroutine(this.coroutines[i]);
				} else {
					this.coroutines[i].Tick();
				}
			}
		}

		[ConsoleCommand("LIST_COROUTINES")]
		internal void ListCoroutines() {
			for (int i = 0; i < this.coroutines.Count; i++) {
				Engine.Editor?.Console.WriteLine(ConsoleEntry.Debug(this.coroutines[i].RoutineName));
			}
		}
	}
}
