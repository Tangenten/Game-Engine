using System;
using RavUtilities;

namespace RavContainers {
	public class PaddedArray<T> where T : class {
		private static readonly int PaddingMultiplier = CpuU.GetCacheLineSize() / IntPtr.Size;

		private T[] data;
		public int Size { get; }
		public int Length { get; }

		public PaddedArray(int size) {
			this.Size = PaddingMultiplier * size;
			this.Length = this.Size / PaddingMultiplier;
			this.data = new T[this.Size];
		}

		public ref T this[int index] => ref this.data[PaddingMultiplier * index];
	}
}