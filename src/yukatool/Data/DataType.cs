using System;

namespace Yuka.Data {
	[Flags]
	enum DataType {
		None = 0,
		Archive = 1,
		Graphics = 2,
		Script = 4,
		Raw = 8
	}
}
