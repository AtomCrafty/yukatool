using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuka.Data.Factory {
	class RawFactory : FileFactory<YukaRaw> {
		public static readonly RawFactory Instance = new RawFactory();

		public override YukaRaw FromBinary(Stream s) {
			using(MemoryStream ms = new MemoryStream()) {
				s.CopyTo(ms);
				return new YukaRaw(ms.ToArray());
			}
		}

		public override long ToBinary(YukaRaw data, Stream s) {
			using(MemoryStream ms = new MemoryStream(data.data)) {
				ms.WriteTo(s);
				return ms.Position;
			}
		}

		public override YukaRaw FromSource(string filename) {
			return new YukaRaw(File.ReadAllBytes(filename));
		}

		public override void ToSource(YukaRaw data, string filename) {
			File.WriteAllBytes(filename, data.data);
		}
	}
}
