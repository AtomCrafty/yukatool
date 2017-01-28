using System.Drawing;

namespace Yuka.Data {
	class YukaGraphics : YukaFile {
		public Bitmap bitmap;
		public byte[] metaData;

		public YukaGraphics(Bitmap bitmap) : base(DataType.Graphics) {
			this.bitmap = bitmap;
		}

		public YukaGraphics(Bitmap bitmap, byte[] metaData) : base(DataType.Graphics) {
			this.bitmap = bitmap;
			this.metaData = metaData;
		}
	}
}
