using System.IO;

namespace RavUtilities {
	public static class FileU {
		public static bool CreateFileIfNotExists(in string filePath) {
			if (!File.Exists(filePath)) {
				using (FileStream fileStream = File.Create(filePath)) { }

				return true;
			}

			return false;
		}

		public static void DeleteFileToRecycleBin(string file) {
			Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
				file,
				Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
				Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
		}

		public static void DeleteDirectoryToRecycleBin(string path) {
			Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(
				path,
				Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
				Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
		}

		public static void CopyAll(DirectoryInfo source, DirectoryInfo target) {
			Directory.CreateDirectory(target.FullName);

			foreach (FileInfo fi in source.GetFiles()) {
				fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
			}

			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
				DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
				CopyAll(diSourceSubDir, nextTargetSubDir);
			}
		}

		public static void RenameAll(DirectoryInfo target, string from, string to) {
			foreach (DirectoryInfo di in target.GetDirectories()) {
				if (di.FullName.Contains(from)) {
					di.MoveTo(di.FullName.Replace(from, to));
				}
			}

			foreach (FileInfo fi in target.GetFiles()) {
				if (fi.FullName.Contains(from)) {
					fi.MoveTo(fi.FullName.Replace(from, to), true);
				}
			}

			foreach (DirectoryInfo diSourceSubDir in target.GetDirectories()) {
				RenameAll(diSourceSubDir, from, to);
			}
		}
	}
}
