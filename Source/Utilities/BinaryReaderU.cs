using System;
using System.IO;
using System.Linq;

namespace RavUtilities {
	public static class BinaryReaderU {
		public static int ReadInt32BE(this BinaryReader binaryReader) {
			byte[] bytes = binaryReader.ReadBytes(4);
			if (BitConverter.IsLittleEndian) {
				return BitConverter.ToInt32(bytes.Reverse().ToArray());
			}
			return BitConverter.ToInt32(bytes);
		}

		public static uint ReadUInt32BE(this BinaryReader binaryReader) {
			byte[] bytes = binaryReader.ReadBytes(4);

			if (BitConverter.IsLittleEndian) {
				return BitConverter.ToUInt32(bytes.Reverse().ToArray());
			}
			return BitConverter.ToUInt32(bytes);
		}
	}
}