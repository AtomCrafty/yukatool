using System;
using System.IO;

namespace Yuka.Data.Factory {
	abstract class FileFactory {
		public static FileFactory ForType(Type type) {
			if(type == typeof(YukaScript)) return ScriptFactory.Instance;
			if(type == typeof(YukaArchive)) return ArchiveFactory.Instance;
			if(type == typeof(YukaGraphics)) return GraphicsFactory.Instance;
			return RawFactory.Instance;
		}

		// TODO
		public static FileFactory ForExtension(string extension) {
			throw new NotImplementedException();
		}
	}

	abstract class FileFactory<T> : FileFactory where T : YukaFile {
		public abstract T FromBinary(Stream s);
		public abstract long ToBinary(T data, Stream s);
		public abstract T FromSource(string filename);
		public abstract void ToSource(T data, string filename);
	}
}
