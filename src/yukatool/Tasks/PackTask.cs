using System;
using System.IO;
using Yuka.Data;
using Yuka.Data.Factory;

namespace Yuka.Tasks {
	class PackTask : Task {
		public override Task NewTask() {
			return new PackTask();
		}

		public PackTask() { }
		public PackTask(string[] arguments, FlagCollection flags) : base(arguments, flags) { }

		public override void DefaultFlags(FlagCollection flags) {
			flags.Add('v', "verbose", "Outputs additional information", false);
			flags.Add('w', "wait", "Waits for enter before closing the console", false);
		}

		protected override void Execute() {
			if(arguments.Length == 0) Fail("No source directory specified");

			string sourceBasePath = Helpers.AbsolutePath(arguments[0]);
			string targetBasePath = arguments.Length > 1 ? Helpers.AbsolutePath(arguments[1]) : Path.ChangeExtension(sourceBasePath.TrimEnd('\\'), "ykc");

			string[] files = null;
			if(Directory.Exists(sourceBasePath)) {
				files = Directory.GetFiles(sourceBasePath, "*", SearchOption.AllDirectories);
			}
			else {
				Fail("The specified source directory does not exist");
			}

			YukaArchive archive = new YukaArchive();

			for(int i = 0; i < files.Length; i++) {
				string sourcePath = files[i];
				string localPath = Helpers.RelativePath(sourcePath, sourceBasePath);
				string extension = Path.GetExtension(localPath);
				DataType type = DataTypes.ForExtension(extension);

				// ignore unknown file types and hidden files
				if(!type.IncludeInArchive()) continue;
				if(new FileInfo(sourcePath).Attributes.HasFlag(FileAttributes.Hidden)) continue;

				currentFile = localPath;

				if(flags.Has('v')) {
					Console.WriteLine();
					Console.WriteLine("SourceBase: " + sourceBasePath);
					Console.WriteLine("TargetBase: " + targetBasePath);
					Console.WriteLine("Source:     " + sourcePath);
					Console.WriteLine("Local:      " + localPath);
				}

				if(DataTypes.ConvertOnPack(extension)) {
					// File needs to be converted
					string realPath = Path.ChangeExtension(localPath, type.BinaryExtension());
					MemoryStream ms = archive.GetOutputStream(realPath);
					dynamic factory = FileFactory.ForDataType(type);
					dynamic data = factory.FromSource(sourcePath);
					factory.ToBinary(data, ms);
				}
				else {
					MemoryStream ms = archive.GetOutputStream(localPath);
					FileStream fs = new FileStream(sourcePath, FileMode.Open);
					fs.CopyTo(ms);
					fs.Close();
				}
			}
			currentFile = "";

			ArchiveFactory.Instance.ToSource(archive, targetBasePath);

			if(flags.Has('w')) {
				Console.ReadLine();
			}
		}
	}
}
