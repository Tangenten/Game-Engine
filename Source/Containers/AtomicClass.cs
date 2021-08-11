using System;
using System.Threading;

namespace RavContainers {
	public class AtomicClass<T> where T : class, ICloneable {
		private long resource;
		private T value;

		public AtomicClass(T value) {
			this.value = value;
			this.resource = 0;
		}

		public AtomicClass(AtomicClass<T> atomic) {
			this.value = atomic.value;
			this.resource = 0;
		}

		public T Get() {
			this.Lock();
			T valueCopy = (T) this.value.Clone();
			this.Unlock();
			return valueCopy;
		}

		public void Set(T value) {
			this.Lock();
			this.value = value;
			this.Unlock();
		}

		public bool TryGet(out T value) {
			if (this.TryLock()) {
				value = (T) this.value.Clone();
				this.Unlock();
				return true;
			}

			value = default;
			return false;
		}

		public bool TrySet(T value) {
			if (this.TryLock()) {
				this.value = value;
				this.Unlock();
				return true;
			}

			return false;
		}

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