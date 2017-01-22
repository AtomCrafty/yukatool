using System;
using System.IO;
using Yuka.Script;
using static Yuka.Constants;

namespace Yuka.Data.Factory {
	class ScriptFactory : FileFactory<YukaScript> {
		public static readonly ScriptFactory Instance = new ScriptFactory();

		public override YukaScript FromBinary(Stream s) {
			return new Decompiler().FromBinary(s);
		}

		public override YukaScript FromSource(string filename) {
			return new Compiler().FromSource(filename, Path.ChangeExtension(filename, stringMetaExtension));
		}

		public override long ToBinary(YukaScript data, Stream s) {
			return new Compiler().ToBinary(data, s);
		}

		public override void ToSource(YukaScript data, string filename) {
			File.WriteAllText(filename, data.Source());
		}
	}
}
