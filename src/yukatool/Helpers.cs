using System;
using System.IO;
using System.Security.Cryptography;

namespace Yuka {
	static class Helpers {
		public static string AbsolutePath(string path) {
			return Path.Combine(Directory.GetCurrentDirectory(), path);
		}

		public static BinaryReader Reader(string path) {
			return new BinaryReader(new FileStream(path, FileMode.Open));
		}

		public static BinaryWriter Writer(string path) {
			return new BinaryWriter(new FileStream(path, FileMode.Create));
		}

		public static void CopyStream(Stream source, Stream target, int length) {
			byte[] buffer = new byte[32768];
			int read = 0;
			while(length > 0 && (read = source.Read(buffer, 0, Math.Min(buffer.Length, length))) > 0) {
				target.Write(buffer, 0, read);
				length -= read;
			}
		}

		public static long FileSize(string path) {
			return new FileInfo(path).Length;
		}

		public static string FileHash(string path) {
			using(HashAlgorithm hasher = new MD5CryptoServiceProvider())
			using(FileStream fs = new FileStream(path, FileMode.Open)) {
				return Convert.ToBase64String(hasher.ComputeHash(fs));
			}
		}
	}
}
