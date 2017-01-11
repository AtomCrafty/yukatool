using System;
using System.IO;
using System.Text;

namespace Yuka.Tasks {
	class SplitTask : Task {
		public override Task NewTask() {
			return new SplitTask();
		}

		public override void DefaultFlags(FlagCollection flags) {
			flags.Add('v', "verbose", "Enables logging of additional information", false);
		}

		protected override void Execute() {
			if(arguments.Length == 0) Fail("No source file specified");

			string sourcePath = Helpers.AbsolutePath(arguments[0]);
			string targetPath = arguments.Length > 1 ? Helpers.AbsolutePath(arguments[1]) : sourcePath;

			if(!System.IO.File.Exists(sourcePath)) Fail("File does not exist");

			FileStream fs = new FileStream(sourcePath, FileMode.Open);
			FileStream os;
			BinaryReader br = new BinaryReader(fs);
			{
				fs.Seek(0x10, SeekOrigin.Begin);
				int codeoffset = br.ReadInt32();
				int codecount = br.ReadInt32();
				int indexoffset = br.ReadInt32();
				int indexcount = br.ReadInt32();
				int dataoffset = br.ReadInt32();
				int datalength = br.ReadInt32();

				// code binary (+ header)
				os = new FileStream(Path.ChangeExtension(targetPath, ".code.dat"), FileMode.Create);
				fs.Seek(0, SeekOrigin.Begin);
				Helpers.CopyStream(fs, os, codecount * 4);

				// code ascii
				os = new FileStream(Path.ChangeExtension(targetPath, ".code.txt"), FileMode.Create);
				fs.Seek(codeoffset, SeekOrigin.Begin);
				os.WriteByte((byte)'\n');
				for(int i = 0; i < codecount; i++) {
					int commandID = br.ReadInt32();
					StringBuilder sb = new StringBuilder(commandID.ToString("X8"));

					// fetch cmd type from index
					long cpos = fs.Position;
					fs.Seek(indexoffset + commandID * 16, SeekOrigin.Begin);
					int cmdType = br.ReadInt32();
					fs.Seek(cpos, SeekOrigin.Begin);

					// parameters for functions
					if(cmdType == 0) {
						i++;
						int paramCount = br.ReadInt32();
						sb.Append(' ').Append(paramCount.ToString("X8"));
						for(int j = 0; j < paramCount; j++) {
							i++;
							sb.Append(' ').Append(br.ReadInt32().ToString("X8"));
						}
					}
					byte[] data = Encoding.ASCII.GetBytes(sb.Append('\n').ToString());
					os.Write(data, 0, data.Length);
					os.Flush();
				}

				// index binay
				os = new FileStream(Path.ChangeExtension(targetPath, ".index.dat"), FileMode.Create);
				fs.Seek(indexoffset, SeekOrigin.Begin);
				Helpers.CopyStream(fs, os, indexcount * 16);

				// index ascii
				os = new FileStream(Path.ChangeExtension(targetPath, ".index.txt"), FileMode.Create);
				fs.Seek(indexoffset, SeekOrigin.Begin);
				for(int i = 0; i < indexcount; i++) {
					byte[] data = Encoding.ASCII.GetBytes(i.ToString("X8") + ": " + br.ReadInt32().ToString("X8") + " " + br.ReadInt32().ToString("X8") + " " + br.ReadInt32().ToString("X8") + " " + br.ReadInt32().ToString("X8") + "\n");
					os.Write(data, 0, data.Length);
				}

				// data binary
				os = new FileStream(Path.ChangeExtension(targetPath, ".data.dat"), FileMode.Create);
				for(int i = 0; i < datalength; i++) {
					os.WriteByte((byte)(fs.ReadByte() ^ 0xAA));
				}
			}
		}
	}
}
