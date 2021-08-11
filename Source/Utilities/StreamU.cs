using System.IO;
using System.Text;

namespace RavUtilities {
	public static class StreamU {
		public static byte[] ToByteArray(this Stream stream) {
			if (stream is MemoryStream) {
				return ((MemoryStream) stream).ToArray();
			}

			using (MemoryStream? memoryStream = new MemoryStream()) {
				stream.CopyTo(memoryStream);
				return memoryStream.ToArray();
			}
		}

		public static string ToStringASCII(this Stream stream) {
			byte[] bytes = stream.ToByteArray();
			return Encoding.ASCII.GetString(bytes, 0, bytes.Length);
		}

		public static string ToStringUTF8(this Stream stream) {
			byte[] bytes = stream.ToByteArray();
			return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}

		public static string ReadText(this FileStream fileStream) {
			string output = "";

			fileStream.Seek(0, SeekOrigin.Begin);
			using (StreamReader streamReader = new StreamReader(fileStream, leaveOpen: true)) {
				output = streamReader.ReadToEnd();
				streamReader.Close();
			}
			fileStream.Seek(0, SeekOrigin.Begin);

			return output;
		}

		public static void WriteText(this FileStream fileStream, string text) {
			fileStream.Seek(0, SeekOrigin.Begin);
			fileStream.SetLength(0);
			fileStream.Flush(true);
			using (StreamWriter streamWriter = new StreamWriter(fileStream, leaveOpen: true)) {
				streamWriter.Write(text);
				streamWriter.Flush();
				streamWriter.Close();
			}
			fileStream.Seek(0, SeekOrigin.Begin);
		}
	}
}