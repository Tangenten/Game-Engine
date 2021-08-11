using System;
using RavUtilities;

namespace RavContainers {
	public class RingArray<T> {
		private T[] data;

		private int index;
		public int Index {
			get => this.index;
			set => this.index = MathU.Mod(value, this.Length);
		}

		public int Length { get; }

		public RingArray(int initialLength) {
			this.Length = initialLength;
			this.data = new T[this.Length];
			this.Index = 0;
		}

		public RingArray(int initialLength, T fillWith) {
			this.Length = initialLength;
			this.data = new T[this.Length];
			this.Index = 0;

			for (int i = 0; i < this.Length; i++) {
				this.data[i] = fillWith;
			}
		}

		public ref T this[int index] => ref this.data[index];

		public T GetData() { return this.data[this.index]; }

		public T[] GetAllData() { return this.data; }

		public Span<T> GetSpan() { return new Span<T>(this.data); }

		public Span<T> GetSpanRange(int start, int end) { return new Span<T>(this.data, MathU.Mod(start, this.Length), MathU.Mod(end - start, this.Length)); }

		public void PushData(T data) {
			this.data[this.index] = data;
			this.index = MathU.Mod(this.index + 1, this.Length);
		}

		public void PushData(T[] data) {
			for (int i = 0; i < data.Length; i++) {
				this.PushData(data[i]);
			}
		}

		public T PopData() {
			this.index = MathU.Mod(this.index - 1, this.Length);
			return this.data[this.index];
		}

		public T[] PopData(int count) {
			T[] list = new T[count];
			for (int i = 0; i < count; i++) {
				list[i] = this.PopData();
			}

			return list;
		}

		public T PeekData() {
			int index = MathU.Mod(this.index - 1, this.Length);
			return this.data[index];
		}

		public T[] PeekData(int count) {
			T[] list = new T[count];

			int index = MathU.Mod(this.index - 1, this.Length);
			for (int i = 0; i < count; i++) {
				list[i] = this.data[index];
				index = MathU.Mod(index - 1, this.Length);
			}

			return list;
		}
	}
}