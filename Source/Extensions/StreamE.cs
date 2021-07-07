using System.IO;
using System.Text;

namespace RavUtilities {
	public static class StreamE {
		public static byte[] ToByteArray(this Stream instream) {
			if (instream is MemoryStream) {
				return ((MemoryStream)instream).ToArray();
			}

			using (MemoryStream? memoryStream = new MemoryStream()) {
				instream.CopyTo(memoryStream);
				return memoryStream.ToArray();
			}
		}

		public static string ToASCIIString(this Stream instream) {
			byte[] bytes = instream.ToByteArray();
			return Encoding.ASCII.GetString(bytes, 0, bytes.Length);
		}

		public static string ToUTF8String(this Stream instream) {
			byte[] bytes = instream.ToByteArray();
			return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}
	}
}
