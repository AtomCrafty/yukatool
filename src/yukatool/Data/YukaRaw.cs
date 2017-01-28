
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuka.Data {
	class YukaRaw : YukaFile {
		public byte[] data;

		public YukaRaw(byte[] data) : base(DataType.Raw) {
			this.data = data;
		}
	}
}
