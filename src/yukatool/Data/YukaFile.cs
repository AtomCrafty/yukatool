using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuka.Data {
	class YukaFile {
		public DataType Type { get; }

		protected YukaFile(DataType type) {
			Type = type;
		}
	}
}
