using System;
using System.IO;
using Yuka.Data;
using Yuka.Script;
using Yuka.Data.Factory;

namespace Yuka.Tasks {
	class UnpackTask : Task {
		public override Task NewTask() {
			return new UnpackTask();
		}

		public override void DefaultFlags(FlagCollection flags) {
			flags.Add('v', "verbose", "Outputs additional information", false);
			flags.Add('w', "wait", "Waits for enter before closing the console", false);
			flags.Add('p', "progress", "Display a progress bar", false);
			flags.Add('r', "raw", "Disables auto-conversion of binary files", false);
			flags.Add('e', "excel", "Uses ; instead of , as csv delimiter", false);
		}

		protected override void Execute() {
			if(arguments.Length == 0) Fail("No source file specified");

			if(flags.Has('v')) flags.Unset('p');

			string sourceBasePath = Helpers.AbsolutePath(arguments[0]);
			string targetBasePath = arguments.Length > 1 ? Helpers.AbsolutePath(arguments[1]) : Path.ChangeExtension(sourceBasePath, "");

			if(!File.Exists(sourceBasePath)) {
				Fail("The specified source file does not exist");
			}

			if(flags.Has('e')) {
				Decompiler.CSVDelimiter = ';';
				Console.WriteLine("Switching to Excel compatibility mode");
			}

			// read archive
			YukaArchive archive = ArchiveFactory.Instance.FromSource(sourceBasePath);

			if(flags.Has('p')) {
				Console.Write("\n\nUnpacking {0}\n", Path.GetFileName(sourceBasePath));
			}

			double count = 0;
			foreach(var file in archive.files) {
				string localPath = file.Key.ToLower();
				string targetPath = Path.Combine(targetBasePath, localPath);

				currentFile = localPath;

				if(flags.Has('v')) {
					Console.WriteLine();
					Console.WriteLine("SourceBase: {0}", sourceBasePath);
					Console.WriteLine("TargetBase: {0}", targetBasePath);
					Console.WriteLine("Target:     {0}", targetPath);
					Console.WriteLine("Local:      {0}", localPath);
				}

				Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

				if(flags.Has('r') || !DataTypes.ConvertOnUnpack(Path.GetExtension(localPath))) {
					// output raw data
					FileStream fs = new FileStream(targetPath, FileMode.Create);
					archive.GetInputStream(file.Key).CopyTo(fs);
					fs.Close();
				}
				else {
					// convert data on extraction
					dynamic factory = FileFactory.ForExtension(Path.GetExtension(localPath));
					dynamic data = factory.FromBinary(archive.GetInputStream(file.Key));
					factory.ToSource(data, targetPath);
				}

				count++;
				if(flags.Has('p')) {
					Console.Write("\r" + TextUtils.ProgressBar(Console.WindowWidth, count / archive.files.Count));
				}
			}
			currentFile = "";

			if(flags.Has('w')) {
				Console.ReadLine();
			}
		}
	}
}
