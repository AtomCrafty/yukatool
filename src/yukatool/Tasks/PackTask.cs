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
				string localPath = sourcePath.Substring(sourceBasePath.Length).TrimStart('\\').ToLower();

				currentFile = localPath;

				if(flags.Has('v')) {
					Console.WriteLine();
					Console.WriteLine("SourceBase: " + sourceBasePath);
					Console.WriteLine("TargetBase: " + targetBasePath);
					Console.WriteLine("Source:     " + sourcePath);
					Console.WriteLine("Local:      " + localPath);
				}

				FileStream fs = new FileStream(sourcePath, FileMode.Open);
				MemoryStream ms = archive.GetOutputStream(localPath);
				fs.CopyTo(ms);
				fs.Close();
			}
			currentFile = "";

			ArchiveFactory.Instance.ToSource(archive, targetBasePath);

			if(flags.Has('w')) {
				Console.ReadLine();
			}
		}
	}
}
