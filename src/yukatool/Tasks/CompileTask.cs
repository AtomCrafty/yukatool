using System;
using System.IO;
using Yuka.Data;
using Yuka.Data.Factory;

namespace Yuka.Tasks {
	class CompileTask : Task {
		public override Task NewTask() {
			return new CompileTask();
		}

		public override void DefaultFlags(FlagCollection flags) {
			flags.Add('v', "verbose", "Outputs additional information", false);
			flags.Add('w', "wait", "Waits for enter before closing the console", false);
		}

		protected override void Execute() {
			if(arguments.Length == 0) Fail("No source file specified");

			string sourceBasePath = Helpers.AbsolutePath(arguments[0]);
			string targetBasePath = arguments.Length > 1 ? Helpers.AbsolutePath(arguments[1]) : sourceBasePath;

			string[] files = null;
			if(Directory.Exists(sourceBasePath)) {
				files = Directory.GetFiles(sourceBasePath, "*." + Constants.ykd, SearchOption.AllDirectories);
			}
			else if(File.Exists(sourceBasePath)) {
				files = new string[] { sourceBasePath };
				sourceBasePath = targetBasePath = "";
			}
			else {
				Fail("The specified source file does not exist");
			}

			for(int i = 0; i < files.Length; i++) {
				string sourcePath = files[i];
				string localPath = Helpers.RelativePath(sourcePath, sourceBasePath);
				string targetPath = Path.ChangeExtension(Path.Combine(targetBasePath, localPath), Constants.yks);

				currentFile = localPath;

				if(flags.Has('v')) {
					Console.WriteLine();
					Console.WriteLine("SourceBase: " + sourceBasePath);
					Console.WriteLine("TargetBase: " + targetBasePath);
					Console.WriteLine("Source:     " + sourcePath);
					Console.WriteLine("Target:     " + targetPath);
					Console.WriteLine("Local:      " + localPath);
				}
				
				Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

				YukaScript script = ScriptFactory.Instance.FromSource(sourcePath);
				ScriptFactory.Instance.ToBinary(script, new FileStream(targetPath, FileMode.Create));
			}
			currentFile = "";
			if(flags.Has('w')) {
				Console.ReadLine();
			}
		}
	}
}
