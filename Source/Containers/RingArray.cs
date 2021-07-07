using System;
using RavUtilities;

namespace RavContainers {
	public class RingArray<T> {
		private T[] data;

		private int index;
		public int Index {
			get => this.index;
			set => this.index = MathU.Mod(value, this.Size);
		}

		public int Size {get; private set; }

		public RingArray(int initialLength) {
			this.Size = initialLength;
			this.data = new T[this.Size];
			this.Index = 0;
		}

		public T GetData() {
			return this.data[this.index];
		}

		public T[] GetAllData() {
			return this.data;
		}

		public Span<T> GetSpan() {
			return new Span<T>(this.data);
		}

		public Span<T> GetSpanRange(int start, int end) {
			return new Span<T>(this.data, start, end - start);
		}

		public void PushData(T data) {
			this.data[this.index] = data;
			this.index = MathU.Mod(this.index + 1, this.Size);
		}

		public void PushData(T[] data) {
			for (int i = 0; i < data.Length; i++) {
				this.PushData(data[i]);
			}
		}

		public T PopData() {
			this.index = MathU.Mod(this.index - 1, this.Size);
			T data = this.data[this.index];
			return data;
		}

		public Span<T> PopData(int count) {
			int end = this.Index;
			this.Index -= count;
			return this.data.AsSpan(new Range(new Index(this.index), new Index(end)));
		}
	}
}
