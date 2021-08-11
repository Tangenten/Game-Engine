using System.Threading;

namespace RavContainers {
	public class FastLock {
		private long resource;

		private bool TryLock() { return Interlocked.Exchange(ref this.resource, 1) == 0; }

		private void Unlock() { Interlocked.Exchange(ref this.resource, 0); }

		private bool Lock() {
			if (this.TryLock()) {
				return true;
			}
			SpinWait.SpinUntil(() => { return this.TryLock(); });
			return true;
		}
	}
}