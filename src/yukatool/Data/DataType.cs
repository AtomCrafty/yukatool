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
				case ypl: return DataType.Internal;
				case ydr: return DataType.Internal;
				case csv: return DataType.None;
				case meta: return DataType.None;
			}
			return DataType.None;
		}

		public static string SourceExtension(this DataType type) {
			switch(type) {
				case DataType.Archive: return ykc;
				case DataType.Graphics: return png;
				case DataType.Script: return ykd;
			}
			return "";
		}

		public static string BaseExtension(string currentExtension) {
			switch(currentExtension.Trim('.').ToLower()) {
				case ykc: return ykc;
				case ykg: return ykg;
				case png: return png;
				case yks: return yks;
				case ykd: return ykd;
				case csv: return ykd;
				case meta: return png;
				case ypl: return ypl;
				case ydr: return ydr;
			}
			return "";
		}

		public static string BinaryExtension(this DataType type) {
			switch(type) {
				case DataType.Archive: return ykc;
				case DataType.Graphics: return ykg;
				case DataType.Script: return yks;
			}
			return "";
		}
	}
}
