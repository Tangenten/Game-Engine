using System;
using System.Collections.Generic;
using RavUtilities;

namespace RavContainers {
	public struct BitSet {
		private readonly byte[] bytes;
		private readonly int indices;

		public BitSet(int size) {
			this.indices = size;
			this.bytes = new byte[(this.indices / 8) + 1];
		}

		public BitSet(BitSet bitSet) {
			this.indices = bitSet.indices;
			this.bytes = bitSet.bytes;
		}

		public BitSet(List<byte> bytes, int indices) {
			this.indices = indices;
			this.bytes = new byte[(this.indices / 8) + 1];
			this.bytes = bytes.ToArray();
		}

		// Test This
		public bool this[int index] {
			get {
				if (index >= indices) {
					throw new Exception("Range of BitSet is 0 - " + (this.indices - 1));
				}

				// OverFlows Perfectly? Test This
				byte indexBit = (byte) Math.Pow(2, index);
				int arrayIndex = (int) TweenU.Linear(index, 0, this.indices, 0, this.bytes.Length - 1);
				byte bitSetBit = this.bytes[arrayIndex];

				return (indexBit & bitSetBit) == indexBit;
			}
			set {
				if (index >= indices) {
					throw new Exception("Range of BitSet is 0 - " + (this.indices - 1));
				}

				// OverFlows Perfectly? Test This
				byte indexBit = (byte) Math.Pow(2, index);
				int arrayIndex = (int) TweenU.Linear(index, 0, this.indices, 0, this.bytes.Length - 1);
				byte bitSetBit = this.bytes[arrayIndex];

				this.bytes[arrayIndex] = (byte) (bitSetBit | indexBit);
			}
		}

		public BitSet And(BitSet bitset) {
			BitSet newBitSet = new BitSet(this);

			for (int i = 0; i < bytes.Length; i++) {
				newBitSet.bytes[i] &= bitset.bytes[i];
			}

			return newBitSet;
		}
	}
}
