using System;
using System.Collections;

namespace RavContainers {
	public class SwapArray<T> {
		private T[] data;
		public int Size {get; private set;}
		public int Length {get; private set;}

		public SwapArray(int initialSize) {
			this.Size = initialSize;
			this.Length = 0;
			this.data = new T[this.Size];
		}

		public ref T this[int index] => ref this.data[index];

		public void Push(T item) {
			this.data[this.Length++] = item;

			if (this.Length == this.Size) {
				this.Resize();
			}
		}

		public bool TryPop(out T val) {
			if (this.Length == 0) {
				val = default;
				return false;
			} else {
				val = this.data[--this.Length];
				return true;
			}
		}

		public void Remove(int index) {
			if (index < this.Length) {
				this.Swap(index, --this.Length);
			}
		}

		public void Clear() {
			Array.Clear(this.data, 0, this.Length);
			this.Length = 0;
		}

		private void Swap(int indexA, int indexB) {
			T temp = this.data[indexA];
			this.data[indexA] = this.data[indexB];
			this.data[indexB] = temp;
		}

		private void Resize() {
			this.Size *= 2;
			Array.Resize(ref this.data, this.Size);
		}

		public Span<T> GetSpan() {
			return new Span<T>(this.data);
		}

		public Span<T> GetSpanRange(int start, int end) {
			return new Span<T>(this.data, start, end - start);
		}

		public static void Copy(ref SwapArray<T> sourceArray, ref SwapArray<T> destinationArray, int start, int end) {
			Array.Copy(sourceArray.data, start, destinationArray.data, start, end - start);
		}

		public static void Copy(ref SwapArray<T> sourceArray, ref SwapArray<T> destinationArray, int sourceStart, int destinationStart, int length) {
			Array.Copy(sourceArray.data, sourceStart, destinationArray.data, destinationStart, length);
		}

		public void Fill(T element) {
			Array.Fill(this.data, element);
		}

		public void Fill(T element, int start, int end) {
			Array.Fill(this.data, element, start, end - start);
		}

		public void Sort(IComparer comparer) {
			Array.Sort(this.data, comparer);
		}

		public void Sort(IComparer comparer, int start, int end) {
			Array.Sort(this.data, start, end, comparer);
		}
	}
}
