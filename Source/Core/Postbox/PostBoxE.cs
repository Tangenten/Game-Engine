using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RavEngine {
	public class PostBoxE : EngineCore {
		private Dictionary<Type, Postbox> postBoxes;

		public PostBoxE() { this.postBoxes = new Dictionary<Type, Postbox>(); }

		internal override void Start() { }

		internal override void Stop() { }

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal override void Update() {
			foreach (KeyValuePair<Type, Postbox> keyValuePair in this.postBoxes) {
				keyValuePair.Value.Update();
			}
		}

		internal override void Reset() { this.postBoxes.Clear(); }

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void RegisterPostbox<A, B>() where B : Message {
			if (!this.postBoxes.ContainsKey(typeof(B))) {
				this.postBoxes[typeof(B)] = new Postbox(typeof(B));
			}

			this.postBoxes[typeof(B)].Register<A>();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void DeRegisterPostbox<A, B>() where B : Message {
			this.postBoxes[typeof(B)].DeRegister<A>();

			if (this.postBoxes[typeof(B)].Registrants() == 0) {
				this.postBoxes.Remove(typeof(B));
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void PostMessage<T>(T data) where T : Message { this.postBoxes[typeof(T)].PostMessage(data); }

		[MethodImpl(MethodImplOptions.Synchronized)]
		public List<B> GetMessages<A, B>() where B : Message { return this.postBoxes[typeof(B)].GetMessages<A, B>(); }

		[ConsoleCommand("LIST_POSTBOXES")]
		internal void ListPostboxes() {
			foreach (KeyValuePair<Type, Postbox> keyValuePair in this.postBoxes) {
				Console.WriteLine(keyValuePair.Key.ToString());
				Engine.Editor.Console.WriteLine(ConsoleEntry.Debug(keyValuePair.Key.ToString()));
			}
		}
	}

	internal struct Postbox {
		private List<Letter> letters;
		private List<Type> registrants;

		public Postbox(Type t) {
			this.letters = new List<Letter>();
			this.registrants = new List<Type>();
		}

		public void Update() {
			for (int i = this.letters.Count - 1; i >= 0; i--) {
				if (this.letters[i].ReadCount() == this.Registrants()) {
					this.letters.Remove(this.letters[i]);
				}
			}
		}

		public int Registrants() { return this.registrants.Count; }

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

		public void PostMessage(object data) { this.letters.Add(new Letter(data)); }

		public List<B> GetMessages<A, B>() where B : Message {
			List<B> messages = new List<B>();
			for (int i = 0; i < this.letters.Count; i++) {
				if (!this.letters[i].HasRead<A>()) {
					messages.Add(this.letters[i].GetData<B>());
					this.letters[i].SetRead<A>();
				}
			}

			return messages;
		}

		public List<B> GetMessages<A, B>(int messagesToRead) where B : Message {
			List<B> messages = new List<B>();
			int count = 0;
			for (int i = 0; i < this.letters.Count; i++) {
				if (!this.letters[i].HasRead<A>()) {
					messages.Add(this.letters[i].GetData<B>());
					this.letters[i].SetRead<A>();
					count++;
				}
				if (messagesToRead == count) {
					break;
				}
			}

			return messages;
		}
	}

	internal struct Letter {
		private object message;
		private List<Type> hasRead;
		private TimeStamp timeStamp;

		public Letter(object message) {
			this.hasRead = new List<Type>();
			this.message = message;
			this.timeStamp = Engine.Time.GetTimeStamp();
		}

		public bool HasRead<T>() { return this.hasRead.Contains(typeof(T)); }

		public void SetRead<T>() { this.hasRead.Add(typeof(T)); }

		public T GetData<T>() where T : Message {
			((T) this.message).TimeStamp = this.timeStamp;
			return (T) this.message;
		}

		public int ReadCount() { return this.hasRead.Count; }
	}

	public abstract class Message {
		public abstract TimeStamp TimeStamp { get; set; }
	}
}