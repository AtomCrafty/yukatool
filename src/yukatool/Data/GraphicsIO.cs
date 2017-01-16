using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static Yuka.Constants;

namespace Yuka.Data {
	class GraphicsIO {
		public static YukaGraphics FromSource(string filename) {
			byte[] metaData = null;

			Bitmap bitmap = new Bitmap(filename);

			if(File.Exists(Path.ChangeExtension(filename, "meta"))) {
				metaData = File.ReadAllBytes(Path.ChangeExtension(filename, "meta"));
			}

			return new YukaGraphics(bitmap, metaData);
		}

		public static void ToSource(YukaGraphics graphics, string filename) {
			Directory.CreateDirectory(Path.GetDirectoryName(filename));

			graphics.bitmap.Save(filename);

			if(graphics.metaData != null) {
				File.WriteAllBytes(Path.ChangeExtension(filename, "meta"), graphics.metaData);
			}
		}

		public static YukaGraphics FromBinary(Stream s) {
			byte[] colorData = null, alphaData = null, metaData = null;
			long offset = s.Position;

			BinaryReader br = new BinaryReader(s, Encoding.ASCII, true /* don't close the stream! */);
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
				colorData[1] = (byte)'P';
				colorData[3] = (byte)'G';
			}
			else {
				if(FlagCollection.current.Has('v')) {
					Console.WriteLine("Warning: missing color layer");
				}
				// throw new Exception("No color layer found");
			}
			if(alphaoffset != 0) {
				s.Seek(offset + alphaoffset, SeekOrigin.Begin);
				alphaData = br.ReadBytes(alphalength);
				// PNG header hack
				alphaData[1] = (byte)'P';
				alphaData[3] = (byte)'G';
			}
			if(metaoffset != 0) {
				s.Seek(offset + metaoffset, SeekOrigin.Begin);
				metaData = br.ReadBytes(metalength);
			}

			br.Close();

			Bitmap colorLayer = colorData != null ? (Image.FromStream(new MemoryStream(colorData)) as Bitmap) : null;
			if(alphaData != null) {
				Bitmap alphaLayer = Image.FromStream(new MemoryStream(alphaData)) as Bitmap;

				Rectangle rect = new Rectangle(0, 0, alphaLayer.Width, alphaLayer.Height);

				colorLayer = colorLayer != null ? colorLayer.Clone(rect, PixelFormat.Format32bppArgb) : new Bitmap(alphaLayer.Width, alphaLayer.Height, PixelFormat.Format32bppArgb);

				BitmapData colorBits = colorLayer.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				BitmapData alphaBits = alphaLayer.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

				int colorBytes = Math.Abs(colorBits.Stride) * colorLayer.Height;
				int alphaBytes = Math.Abs(alphaBits.Stride) * alphaLayer.Height;

				byte[] colorValues = new byte[colorBytes];
				byte[] alphaValues = new byte[alphaBytes];

				Marshal.Copy(colorBits.Scan0, colorValues, 0, colorBytes);
				Marshal.Copy(alphaBits.Scan0, alphaValues, 0, alphaBytes);

				for(int counter = 0; counter < colorValues.Length; counter += 4) {
					// set the alpha channel of colorValue to the inverted red channel of alphaValues
					colorValues[counter + 3] = (byte)(255 - alphaValues[counter]);
				}

				Marshal.Copy(colorValues, 0, colorBits.Scan0, colorBytes);
				// Marshal.Copy(alphaValues, 0, colorBits.Scan0, alphaBytes);

				colorLayer.UnlockBits(colorBits);
				//alphaLayer.UnlockBits(alphaBits);
			}

			return new YukaGraphics(colorLayer, metaData);
		}

		public static int ToBinary(YukaGraphics graphics, Stream s) {
			long offset = s.Position;
			BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true);
			bw.Write(YKG_HEADER);

			Bitmap colorLayer = graphics.bitmap;
			/*
			Bitmap alphaLayer = null;
			if(graphics.bitmap.PixelFormat == PixelFormat.Format32bppArgb) {
				alphaLayer = new Bitmap(colorLayer.Width, colorLayer.Height, PixelFormat.Format24bppRgb);

				Rectangle rect = new Rectangle(0, 0, colorLayer.Width, colorLayer.Height);

				BitmapData colorBits = colorLayer.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				BitmapData alphaBits = alphaLayer.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

				int colorBytes = Math.Abs(colorBits.Stride) * colorLayer.Height;
				int alphaBytes = Math.Abs(alphaBits.Stride) * alphaLayer.Height;

				byte[] colorValues = new byte[colorBytes];
				byte[] alphaValues = new byte[alphaBytes];

				Marshal.Copy(colorBits.Scan0, colorValues, 0, colorBytes);
				Marshal.Copy(alphaBits.Scan0, alphaValues, 0, alphaBytes);

				for(int counter = 0; counter * 3 < alphaValues.Length; counter++) {
					// set the alpha channel of colorValue to the inverted red channel of alphaValues
					byte opacity = (byte)(255 - colorValues[counter * 4 + 3]);
					alphaValues[counter * 3 + 0] = opacity;
					alphaValues[counter * 3 + 1] = opacity;
					alphaValues[counter * 3 + 2] = opacity;
				}

				Marshal.Copy(colorValues, 0, colorBits.Scan0, colorBytes);
				Marshal.Copy(alphaValues, 0, colorBits.Scan0, alphaBytes);

				colorLayer.UnlockBits(colorBits);
				alphaLayer.UnlockBits(alphaBits);

				colorLayer = colorLayer.Clone(rect, PixelFormat.Format24bppRgb);
			}
			*/
			MemoryStream colorStream = new MemoryStream();
			//MemoryStream alphaStream = new MemoryStream();

			colorLayer.Save(colorStream, ImageFormat.Png);

			int curoffset = 0x40; // header length

			bw.Write(curoffset);
			bw.Write((int)colorStream.Length);

			curoffset += (int)colorStream.Length;
			/*
			if(alphaLayer != null) {
				alphaLayer.Save(alphaStream, ImageFormat.Png);
				bw.Write(curoffset);
				bw.Write((int)alphaStream.Length);
				curoffset += (int)alphaStream.Length;
			}
			else {
				bw.Write((long)0);
			}
			*/
			if(graphics.metaData != null) {
				bw.Write(curoffset);
				bw.Write(graphics.metaData.Length);
			}
			else {
				bw.Write((long)0);
			}

			colorStream.WriteTo(s);
			colorStream.Close();
			/*
			if(alphaLayer != null) {
				alphaStream.WriteTo(s);
			}
			alphaStream.Close();
			*/
			if(graphics.metaData != null) {
				bw.Write(graphics.metaData);
			}

			bw.Close();
			return (int)(s.Position - offset);
		}
	}
}
