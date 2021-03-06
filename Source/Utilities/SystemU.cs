using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RavUtilities {
	public static class SystemU {
		private static Mutex globalLock;

		public static bool IsProcessUnique(in string guid) {
			globalLock = new Mutex(false, guid);
			return globalLock.WaitOne(0, false);
		}

		public static bool IsProcessUnique() {
			globalLock = new Mutex(false, Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName));
			return globalLock.WaitOne(0, false);
		}

		public static void LogExceptionsIntoFile() { AppDomain.CurrentDomain.UnhandledException += WriteExceptionIntoFile; }

		private static void WriteExceptionIntoFile(object sender, UnhandledExceptionEventArgs e) {
			Exception ex = e.ExceptionObject as Exception;
			Console.WriteLine(ex.Message);

			string filePath = $"{Directory.GetCurrentDirectory()}/ExceptionLog.txt";
			using (StreamWriter writer = new StreamWriter(filePath, true)) {
				writer.WriteLine("-----------------------------------------------------------------------------");
				writer.WriteLine($"Date : {DateTime.Now}");
				writer.WriteLine();

				while (ex != null) {
					writer.WriteLine(ex.GetType().FullName);
					writer.WriteLine($"Message : {ex.Message}");
					writer.WriteLine($"StackTrace : {ex.StackTrace}");

					ex = ex.InnerException;
				}
			}
		}
	}
}
