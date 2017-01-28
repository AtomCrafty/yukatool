using System;
using static Yuka.Constants;

namespace Yuka.Data {
	[Flags]
	internal enum DataType {
		None = 0,
		Archive = 1,
		Graphics = 2,
		Script = 4,
		Raw = 8,
		Internal = 16
	}

	internal static class DataTypes {
		public static DataType ForExtension(string ext) {
			switch(ext.Trim('.').ToLower()) {
				case ykc: return DataType.Archive;
				case ykg: return DataType.Graphics;
				case png: return DataType.Graphics;
				case yks: return DataType.Script;
				case ykd: return DataType.Script;
				case csv: return DataType.None;
				case meta: return DataType.None;
				case ypl: return DataType.Internal;
			}
			return DataType.None;
		}
	}
}
