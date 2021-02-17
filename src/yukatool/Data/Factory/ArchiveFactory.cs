using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yuka.Data.Factory {
	class ArchiveFactory : FileFactory<YukaArchive> {
		public static readonly ArchiveFactory Instance = new ArchiveFactory();
		private static readonly Encoding ShiftJis = Encoding.GetEncoding("Shift-JIS");

		public ArchiveFactory() : base(DataType.Archive) { }

		public override YukaArchive FromBinary(Stream s) {
			BinaryReader br = new BinaryReader(s);
			Dictionary<string, MemoryStream> files = new Dictionary<string, MemoryStream>();

			s.Seek(0x08, SeekOrigin.Begin);
			uint headerlength = br.ReadUInt32();
			s.Seek(0x04, SeekOrigin.Current);
			uint indexoffset = br.ReadUInt32();
			uint indexlength = br.ReadUInt32();

			for(int i = 0; i < indexlength / 0x14; i++) {
				uint curoffset = (uint)(indexoffset + i * 0x14);
				s.Seek(curoffset, SeekOrigin.Begin);
				uint nameoffset = br.ReadUInt32();
				uint namelength = br.ReadUInt32();
				uint dataoffset = br.ReadUInt32();
				uint datalength = br.ReadUInt32();

				s.Seek(nameoffset, SeekOrigin.Begin);
				string name = ShiftJis.GetString(br.ReadBytes((int)namelength - 1)).ToLower();
				s.Seek(dataoffset, SeekOrigin.Begin);
				byte[] data = br.ReadBytes((int)datalength);

				files[name] = new MemoryStream(data);
			}

			return new YukaArchive(files);
		}

		public override long ToBinary(YukaArchive data, Stream s) {
			long offset = s.Position;
			BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true);

			s.Write(Encoding.ASCII.GetBytes("YKC001\0\0"), 0x00, 0x08);
			bw.Write((uint)0x18);
			s.Write(new byte[0x0C], 0x00, 0x0C);

			// dataoffset, datalength, nameoffset, namelength
			Dictionary<string, uint[]> offsets = new Dictionary<string, uint[]>();

			// Write data sector
			foreach(var file in data.files) {
				uint dataoffset = (uint)s.Position;
				/*
				if(FlagCollection.current.Has('v')) {
					Console.WriteLine("Packing file: " + file.Key);
				}//*/
				MemoryStream ms = data.GetInputStream(file.Key);
				ms.CopyTo(s);
				//s.Write(data, 0, data.Length);
				offsets[file.Key] = (new uint[] { dataoffset, (uint)file.Value.Length, 0, (uint)ShiftJis.GetByteCount(file.Key) + 1 });
			}

			// Write name table
			foreach(var entry in offsets) {
				uint nameoffset = (uint)s.Position;
				var bytes = ShiftJis.GetBytes(entry.Key);
				s.Write(bytes, 0, bytes.Length);
				s.WriteByte(0);
				entry.Value[2] = nameoffset;
			}

			uint indexoffset = (uint)s.Position;

			// Write index
			foreach(var entry in offsets) {
				bw.Write(entry.Value[2]);
				bw.Write(entry.Value[3]);
				bw.Write(entry.Value[0]);
				bw.Write(entry.Value[1]);
				bw.Write((uint)0x00);
			}

			// Update header
			uint indexlength = (uint)(s.Position - indexoffset);
			bw.Seek(0x10, SeekOrigin.Begin);
			bw.Write(indexoffset);
			bw.Write(indexlength);

			bw.Close();
			return s.Position - offset;
		}

		public override YukaArchive FromSource(string filename) {
			using(FileStream fs = new FileStream(filename, FileMode.Open)) {
				return FromBinary(fs);
			}
		}

		public override void ToSource(YukaArchive data, string filename) {
			Directory.CreateDirectory(Path.GetDirectoryName(filename));
			using(FileStream fs = new FileStream(filename, FileMode.Create)) {
				ToBinary(data, fs);
			}
		}
	}
}
