using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RavEngine {
	public class PostBoxE : EngineCore {
		private Dictionary<Type, Postbox> postBoxes;

		public PostBoxE() {
		}

		internal override void Start() {
			this.postBoxes = new Dictionary<Type, Postbox>();
		}

		internal override void Stop() { }

		internal override void Update() {
			foreach (KeyValuePair<Type, Postbox> keyValuePair in this.postBoxes) {
				keyValuePair.Value.Update();
			}
		}

		internal override void Reset() {
			this.postBoxes.Clear();
		}

		public void Test() {
			this.RegisterPostbox<TimeE, Data>();
			this.DeRegisterPostbox<TimeE, Data>();
			this.PostMessage(new Data());
			this.GetMessages<TimeE, Data>();
		}

		public struct Data : IMessage {
			public TimeStamp TimeStamp { get; set; }
		}

		public void RegisterPostbox<A, B>() where B : IMessage {
			if (!this.postBoxes.ContainsKey(typeof(B))) {
				this.postBoxes[typeof(B)] = new Postbox(typeof(B));
			}

			this.postBoxes[typeof(B)].Register<A>();
		}

		public void DeRegisterPostbox<A, B>() where B : IMessage {
			this.postBoxes[typeof(B)].DeRegister<A>();

			if (this.postBoxes[typeof(B)].Registrants() == 0) {
				this.postBoxes.Remove(typeof(B));
			}
		}

		public void PostMessage<T>(T data) where T : IMessage {
			this.postBoxes[typeof(T)].PostMessage(data);
		}

		public List<B> GetMessages<A, B>() where B : IMessage {
			return this.postBoxes[typeof(B)].GetMessages<A, B>();
		}
	}

	internal struct Postbox {
		private List<Message> messages;
		private List<Type> registrants;

		public Postbox(Type t) {
			this.messages = new List<Message>();
			this.registrants = new List<Type>();
		}

		public void Update() {
			for (int i = this.messages.Count - 1; i >= 0; i--) {
				if (this.messages[i].ReadCount() == this.registrants.Count) {
					this.messages.Remove(this.messages[i]);
				}
			}
		}

		public int Registrants() {
			return this.registrants.Count;
		}

		public void Register<T>() {
			if (!this.registrants.Contains(typeof(T))) {
				this.registrants.Add(typeof(T));
			}
		}

		public void DeRegister<T>() {
			if (this.registrants.Contains(typeof(T))) {
				this.registrants.Remove(typeof(T));
			}
		}

		public void PostMessage(object data) {
			this.messages.Add(new Message(data));
		}

		public List<B> GetMessages<A, B>() where B : IMessage {
			List<B> messages = new List<B>();
			for (int i = 0; i < this.messages.Count; i++) {
				if (!this.messages[i].HasRead<A>()) {
					messages.Add(this.messages[i].GetData<B>());
					this.messages[i].SetRead<A>();
				}
			}

			return messages;
		}

		public List<B> GetMessages<A, B>(int messagesToRead) where B : IMessage {
			List<B> messages = new List<B>();
			int count = 0;
			for (int i = 0; i < this.messages.Count; i++) {
				if (!this.messages[i].HasRead<A>()) {
					messages.Add(this.messages[i].GetData<B>());
					this.messages[i].SetRead<A>();
					count++;
				}
				if (messagesToRead == count) {
					break;
				}
			}

			return messages;
		}
	}

	internal struct Message {
		private List<Type> hasRead;
		private object data;
		private TimeStamp timeStamp;

		public Message(object data) {
			this.hasRead = new List<Type>();
			this.data = data;
			this.timeStamp = Engine.Time.GetTimeStamp();
		}

		public bool HasRead<T>() {
			return this.hasRead.Contains(typeof(T));
		}

		public void SetRead<T>() {
			this.hasRead.Add(typeof(T));
		}

		public T GetData<T>() where T : IMessage {
			((T) this.data).TimeStamp = this.timeStamp;
			return (T) this.data;
		}

		public int ReadCount() {
			return this.hasRead.Count;
		}
	}

	public interface IMessage {
		public TimeStamp TimeStamp { get; set; }
	}
}
