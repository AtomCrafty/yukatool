using System.Collections.Generic;
using System.IO;

namespace yuka.Script {
	class DataManager {
		Dictionary<int, byte[]> dataTable = new Dictionary<int, byte[]>();
		public int offset = 0;

		public int Put(byte[] data) {
			int index = IndexOf(data);
			if(index != -1) {
				return index;
			}
			dataTable.Add(offset, data);
			int cur = offset;
			offset += data.Length;
			return cur;
		}

		public int IndexOf(byte[] data) {
			if(data == null) return -1;
			foreach(var entry in dataTable) {
				if(entry.Value == null || entry.Value.Length != data.Length) continue;
				bool mismatch = false;
				for(int i = 0; i < data.Length; i++) {
					if(entry.Value[i] != data[i]) {
						mismatch = true;
						break;
					}
				}
				if(!mismatch) return entry.Key;
			}
			return -1;
		}

		public byte[] Resolve(int offset) {
			return dataTable[offset];
		}

		public void WriteTo(Stream s) {
			foreach(byte[] data in dataTable.Values) {
				foreach(byte b in data) {
					s.WriteByte((byte)(b ^ 0xAA));
				}
			}
		}
	}
}
