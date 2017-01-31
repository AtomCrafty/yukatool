using System;
using static Yuka.Constants;

namespace Yuka.Data {
	[Flags]
	internal enum DataType {
		None = 0,
		Archive = 1,
		Graphics = 2,
		Script = 4,
		Sound = 8,
		Other = 16,
		Raw = 32,
		Internal = 64
	}

	internal static class DataTypes {
		/**
		 * Determines whether a file needs to be converted before packing
		 */
		public static bool ConvertOnPack(string ext) {
			switch(ext.Trim('.').ToLower()) {
				case ykd: return true;
				case png: return true;
				default: return false;
			}
		}

		/**
		 * Determines whether a file needs to be converted before unpacking
		 */
		public static bool ConvertOnUnpack(string ext) {
			switch(ext.Trim('.').ToLower()) {
				case yks: return true;
				case ykg: return true;
				default: return false;
			}
		}

		public static DataType ForExtension(string ext) {
			switch(ext.Trim('.').ToLower()) {
				case yks: return DataType.Script;
				case ykd: return DataType.Script;
				case ykc: return DataType.Archive;
				case ykg: return DataType.Graphics;
				case png: return DataType.Graphics;
				case ypl: return DataType.Internal;
				case ydr: return DataType.Internal;
				case ogg: return DataType.Sound;
				case ini: return DataType.Other;
				case csv: return DataType.None;
				case meta: return DataType.None;
			}
			return DataType.None;
		}

		public static string SourceExtension(this DataType type) {
			switch(type) {
				case DataType.Script: return ykd;
				case DataType.Archive: return ykc;
				case DataType.Graphics: return png;
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
				case ogg: return ogg;
				case ini: return ini;
				case ypl: return ypl;
				case ydr: return ydr;
				case meta: return png;
			}
			return "";
		}

		public static string BinaryExtension(this DataType type) {
			switch(type) {
				case DataType.Script: return yks;
				case DataType.Archive: return ykc;
				case DataType.Graphics: return ykg;
			}
			return "";
		}

		public static bool IncludeInArchive(this DataType type) {
			switch(type) {
				case DataType.Script:
				case DataType.Archive:
				case DataType.Graphics:
				case DataType.Sound:
				case DataType.Other:
					return true;
			}
			return false;
		}
	}
}
