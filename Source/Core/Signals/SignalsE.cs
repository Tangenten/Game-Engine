using System;
using System.Collections.Generic;

namespace RavEngine {
	public class SignalsE : EngineCore {
		private Dictionary<Type, List<Delegate>> events;

		public SignalsE() {
		}

		internal override void Start() {
			this.events = new Dictionary<Type, List<Delegate>>();
		}

		internal override void Stop() {
		}

		internal override void Update() {
		}

		internal override void Reset() {
			this.events.Clear();
		}

		public void AddListener<T>(Action action) where T : ISignal {
			if (!this.events.ContainsKey(typeof(T))) {
				this.events[typeof(T)] = new List<Delegate>();
			}

			this.events[typeof(T)].Add(action);
		}

		public void AddListener<T>(Action<T> action) where T : ISignal {
			if (!this.events.ContainsKey(typeof(T))) {
				this.events[typeof(T)] = new List<Delegate>();
			}

			this.events[typeof(T)].Add(action);
		}

		public void RemoveListener<T>(Action<T> action) where T : ISignal {
			if (this.events.ContainsKey(typeof(T))) {
				this.events[typeof(T)].Remove(action);
			}
		}

		public void InvokeEvent<T>() where T : ISignal {
			List<Delegate> listeners = this.events[typeof(T)];

			for (int i = 0; i < listeners.Count; i++) {
				listeners[i].DynamicInvoke();
			}
		}

		public void InvokeEvent<T>(T eventData) where T : ISignal {
			List<Delegate> listeners = this.events[typeof(T)];

			for (int i = 0; i < listeners.Count; i++) {
				listeners[i].DynamicInvoke(eventData);
			}
		}
	}

	public interface ISignal {
	}
}
