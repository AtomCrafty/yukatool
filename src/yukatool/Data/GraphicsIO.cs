using System;
using System.Collections.Generic;
using System.IO;
using static Yuka.Constants;

namespace Yuka.Data {
	class GraphicsIO {
		public static YukaGraphics FromSource(string filename) {
			byte[] colorData = null, alphaData = null, metaData = null;

			if(File.Exists(Path.ChangeExtension(filename, "layer0.png"))) {
				colorData = File.ReadAllBytes(Path.ChangeExtension(filename, "layer0.png"));
			}
			if(File.Exists(Path.ChangeExtension(filename, "layer1.png"))) {
				alphaData = File.ReadAllBytes(Path.ChangeExtension(filename, "layer1.png"));
			}
			if(File.Exists(Path.ChangeExtension(filename, "meta"))) {
				metaData = File.ReadAllBytes(Path.ChangeExtension(filename, "meta"));
			}

			return new YukaGraphics(colorData, alphaData, metaData);
		}

		public static void ToSource(YukaGraphics graphics, string filename) {
			Directory.CreateDirectory(Path.GetDirectoryName(filename));

			if(graphics.colorData != null) {
				File.WriteAllBytes(Path.ChangeExtension(filename, "layer0.png"), graphics.colorData);
			}
			if(graphics.alphaData != null) {
				File.WriteAllBytes(Path.ChangeExtension(filename, "layer0.png"), graphics.alphaData);
			}
			if(graphics.metaData != null) {
				File.WriteAllBytes(Path.ChangeExtension(filename, "meta"), graphics.metaData);
			}
		}

		public static YukaGraphics FromBinary(Stream s) {
			byte[] colorData = null, alphaData = null, metaData = null;
			long offset = s.Position;

			BinaryReader br = new BinaryReader(s);
			s.Seek(0x28, SeekOrigin.Current);
			int coloroffset = br.ReadInt32();
			int colorlength = br.ReadInt32();
			int alphaoffset = br.ReadInt32();
			int alphalength = br.ReadInt32();
			int metaoffset = br.ReadInt32();
			int metalength = br.ReadInt32();

			if(coloroffset != 0) {
				s.Seek(offset + coloroffset, SeekOrigin.Begin);
				colorData = br.ReadBytes(colorlength);
				// PNG header hack
				colorData[1] = (byte)'G';
				colorData[3] = (byte)'P';
			}
			if(alphaoffset != 0) {
				s.Seek(offset + alphaoffset, SeekOrigin.Begin);
				alphaData = br.ReadBytes(alphalength);
				// PNG header hack
				alphaData[1] = (byte)'G';
				alphaData[3] = (byte)'P';
			}
			if(metaoffset != 0) {
				s.Seek(offset + metaoffset, SeekOrigin.Begin);
				metaData = br.ReadBytes(metalength);
			}

			br.Close();
			return new YukaGraphics(colorData, alphaData, metaData);
		}

		public static int ToBinary(YukaGraphics graphics, Stream s) {
			long offset = s.Position;
			BinaryWriter bw = new BinaryWriter(s);
			bw.Write(YKG_HEADER);

			int curoffset = YKG_HEADER.Length;

			if(graphics.colorData != null) {
				bw.Write(curoffset);
				bw.Write(graphics.colorData.Length);
				curoffset += graphics.colorData.Length;
			}
			else {
				bw.Write((long)0);
			}
			if(graphics.alphaData != null) {
				bw.Write(curoffset);
				bw.Write(graphics.alphaData.Length);
				curoffset += graphics.alphaData.Length;
			}
			else {
				bw.Write((long)0);
			}
			if(graphics.metaData != null) {
				bw.Write(curoffset);
				bw.Write(graphics.metaData.Length);
			}
			else {
				bw.Write((long)0);
			}

			if(graphics.colorData != null) {
				bw.Write(graphics.colorData);
			}
			if(graphics.alphaData != null) {
				bw.Write(graphics.alphaData);
			}
			if(graphics.metaData != null) {
				bw.Write(graphics.metaData);
			}

			bw.Close();
			return (int)(s.Position - offset);
		}
	}
}
