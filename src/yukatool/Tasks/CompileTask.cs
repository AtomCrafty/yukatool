using System;
using System.IO;
using Yuka.Data;
using Yuka.Script;

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
				files = Directory.GetFiles(sourceBasePath, "*." + Constants.decompiledScriptExtension, SearchOption.AllDirectories);
			}
			else if(System.IO.File.Exists(sourceBasePath)) {
				files = new string[] { sourceBasePath };
				sourceBasePath = targetBasePath = "";
			}
			else {
				Fail("The specified source file does not exist");
			}

			for(int i = 0; i < files.Length; i++) {
				string sourcePath = files[i];
				string localPath = sourcePath.Substring(sourceBasePath.Length).TrimStart('\\').ToLower();
				string targetPath = Path.ChangeExtension(Path.Combine(targetBasePath, localPath), Constants.compiledScriptExtension);
				string metaPath = Path.ChangeExtension(Path.Combine(sourceBasePath, localPath), Constants.stringMetaExtension);

				currentFile = localPath;

				if(flags.Has('v')) {
					Console.WriteLine("SourceBase: " + sourceBasePath);
					Console.WriteLine("TargetBase: " + targetBasePath);
					Console.WriteLine("Source:     " + sourcePath);
					Console.WriteLine("Target:     " + targetPath);
					Console.WriteLine("Local:      " + localPath);
					Console.WriteLine();
				}

				Compiler comp = new Compiler();

				YukaScript script = comp.FromSource(sourcePath, metaPath);

				Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
				FileStream fs = new FileStream(targetPath, FileMode.Create);

				comp.ToBinary(script, fs);

				fs.Close();
			}
			currentFile = "";
			if(flags.Has('w')) {
				Console.ReadLine();
			}
		}
	}
}
