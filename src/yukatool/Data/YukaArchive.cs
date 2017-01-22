using System.Collections.Generic;
using System.IO;

namespace Yuka.Data {
	class YukaArchive : YukaFile {
		public Dictionary<string, MemoryStream> files;

		public YukaArchive() {
			files = new Dictionary<string, MemoryStream>();
		}

		public YukaArchive(Dictionary<string, MemoryStream> files) {
			this.files = files;
		}

		public MemoryStream GetInputStream(string name) {
			MemoryStream ms = files[name];
			ms.Seek(0, SeekOrigin.Begin);
			return ms;
		}

		public MemoryStream GetOutputStream(string name) {
			if(!files.ContainsKey(name)) {
				files[name] = new MemoryStream();
			}
			MemoryStream ms = files[name];
			ms.Seek(0, SeekOrigin.Begin);
			ms.SetLength(0);
			return ms;
		}
	}
}
