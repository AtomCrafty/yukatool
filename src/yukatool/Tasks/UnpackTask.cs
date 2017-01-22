using System;
using System.IO;
using Yuka.Data;
using Yuka.Script;

namespace Yuka.Tasks {
	class UnpackTask : Task {
		public override Task NewTask() {
			return new UnpackTask();
		}

		public override void DefaultFlags(FlagCollection flags) {
			flags.Add('v', "verbose", "Outputs additional information", false);
			flags.Add('w', "wait", "Waits for enter before closing the console", false);
			flags.Add('p', "progress", "Display a progress bar", false);
		}

		protected override void Execute() {
			if(arguments.Length == 0) Fail("No source file specified");

			string sourceBasePath = Helpers.AbsolutePath(arguments[0]);
			string targetBasePath = arguments.Length > 1 ? Helpers.AbsolutePath(arguments[1]) : Path.ChangeExtension(sourceBasePath, "");

			if(!File.Exists(sourceBasePath)) {
				Fail("The specified source file does not exist");
			}

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
					Console.WriteLine("SourceBase: " + sourceBasePath);
					Console.WriteLine("TargetBase: " + targetBasePath);
					Console.WriteLine("Target:     " + targetPath);
					Console.WriteLine("Local:      " + localPath);
				}

				Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

				FileStream fs = new FileStream(targetPath, FileMode.Create);
				archive.GetInputStream(file.Key).CopyTo(fs);
				fs.Close();

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
