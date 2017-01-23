using System;
using System.IO;
using Yuka.Data;

namespace Yuka.Tasks {
	class PatchTask : Task {
		public override Task NewTask() {
			return new PatchTask();
		}

		public override void DefaultFlags(FlagCollection flags) {
			flags.Add('v', "verbose", "Outputs additional information", false);
			flags.Add('w', "wait", "Waits for enter before closing the console", false);
		}

		protected override void Execute() {
			if(arguments.Length == 0) Fail("No target file specified");
			if(arguments.Length == 1) Fail("No source file specified");

			string targetBasePath = Helpers.AbsolutePath(arguments[0]);
			string sourceBasePath = Helpers.AbsolutePath(arguments[1]);

			if(!File.Exists(targetBasePath)) Fail("The specified target archive does not exist");
			if(!File.Exists(sourceBasePath)) Fail("The specified source archive does not exist");

			YukaArchive target = ArchiveFactory.Instance.FromSource(targetBasePath);
			YukaArchive source = ArchiveFactory.Instance.FromSource(sourceBasePath);

			if(target == null) Fail("Failed to read target archive.");
			if(source == null) Fail("Failed to read source archive.");

			foreach(var entry in source.files) {
				string localPath = entry.Key;
				Task.currentTask.currentFile = localPath;

				target.files[entry.Key] = entry.Value;

				if(flags.Has('v')) {
					Console.WriteLine();
					Console.WriteLine("SourceBase: " + sourceBasePath);
					Console.WriteLine("TargetBase: " + targetBasePath);
					Console.WriteLine("Local:      " + localPath);
				}
			}
			currentFile = "";

			ArchiveFactory.Instance.ToSource(target, targetBasePath);

			if(flags.Has('w')) {
				Console.ReadLine();
			}
		}
	}
}
