namespace Yuka.Data {
	class YukaGraphics {
		public byte[] colorData, alphaData, metaData;

		public YukaGraphics(byte[] colorData) {
			this.colorData = colorData;
		}

		public YukaGraphics(byte[] colorData, byte[] alphaData, byte[] metaData) {
			this.colorData = colorData;
			this.alphaData = alphaData;
			this.metaData = metaData;
		}
	}
}
