using System;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;

namespace RavUtilities {
	public static class FileU {
		public static FileStream LoadStreamWaitLock(string filePath) {
			// Will try to load file over and over again, usually a file will get locked for a short while
			// as the file is being saved in another program

			while (true) {
				try {
					return new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
				} catch (Exception e) {
					Console.WriteLine(e.Message);
				}
			}
		}

		public static bool CreateFileIfNotExists(in string filePath) {
			if (!File.Exists(filePath)) {
				using (FileStream fileStream = File.Create(filePath)) { }

				return true;
			}

			return false;
		}

		public static void DeleteFileToRecycleBin(string file) {
			FileSystem.DeleteFile(
				file,
				UIOption.OnlyErrorDialogs,
				RecycleOption.SendToRecycleBin);
		}

		public static void DeleteDirectoryToRecycleBin(string path) {
			FileSystem.DeleteDirectory(
				path,
				UIOption.OnlyErrorDialogs,
				RecycleOption.SendToRecycleBin);
		}

		public static void CopyAllFilesInDirectory(DirectoryInfo source, DirectoryInfo target) {
			Directory.CreateDirectory(target.FullName);

			foreach (FileInfo fi in source.GetFiles()) {
				fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
			}

			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
				DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
				CopyAllFilesInDirectory(diSourceSubDir, nextTargetSubDir);
			}
		}

		public static void RenameAllFilesInDirectory(DirectoryInfo target, string from, string to) {
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
				RenameAllFilesInDirectory(diSourceSubDir, from, to);
			}
		}

		public static bool TryGetSolutionPath(out string path) {
			DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());
			while (directory != null && !directory.GetFiles("*.sln").Any()) {
				directory = directory.Parent;
			}

			if (directory == null) {
				path = "";
				return false;
			}
			path = directory.FullName;
			return true;
		}

		public static bool TryGetProjectPath(out string path) {
			DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());
			while (directory != null && !directory.GetFiles("*.csproj").Any()) {
				directory = directory.Parent;
			}

			if (directory == null) {
				path = "";
				return false;
			}
			path = directory.FullName;
			return true;
		}
	}
}