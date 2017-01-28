using System;
using System.IO;

namespace Yuka.Data.Factory {
	abstract class FileFactory {
		public DataType Type { get; private set; }

		public static FileFactory ForType(Type type) {
			if(type == typeof(YukaScript)) return ScriptFactory.Instance;
			if(type == typeof(YukaArchive)) return ArchiveFactory.Instance;
			if(type == typeof(YukaGraphics)) return GraphicsFactory.Instance;
			return RawFactory.Instance;
		}

		public static FileFactory ForDataType(DataType type) {
			if(type == DataType.None) return null;
			if(type == DataType.Script) return ScriptFactory.Instance;
			if(type == DataType.Archive) return ArchiveFactory.Instance;
			if(type == DataType.Graphics) return GraphicsFactory.Instance;
			return RawFactory.Instance;
		}

		// TODO
		public static FileFactory ForExtension(string extension) {
			throw new NotImplementedException();
		}

		public FileFactory(DataType Type) {
			this.Type = Type;
		}
	}

	abstract class FileFactory<T> : FileFactory where T : YukaFile {
		public abstract T FromBinary(Stream s);
		public abstract long ToBinary(T data, Stream s);
		public abstract T FromSource(string filename);
		public abstract void ToSource(T data, string filename);

		public FileFactory(DataType Type) : base(Type) { }
	}
}
