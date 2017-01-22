using System.IO;

namespace Yuka.Data.Factory {
	abstract class FileFactory<T> where T : YukaFile {
		public abstract T FromBinary(Stream s);
		public abstract long ToBinary(T data, Stream s);
		public abstract T FromSource(string filename);
		public abstract void ToSource(T data, string filename);
	}
}
