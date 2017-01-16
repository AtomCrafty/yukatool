using System.Drawing;

namespace Yuka.Data {
	class YukaGraphics {
		public Bitmap bitmap;
		public byte[] metaData;

		public YukaGraphics(Bitmap bitmap) {
			this.bitmap = bitmap;
		}

		public YukaGraphics(Bitmap bitmap, byte[] metaData) {
			this.bitmap = bitmap;
			this.metaData = metaData;
		}
	}
}
