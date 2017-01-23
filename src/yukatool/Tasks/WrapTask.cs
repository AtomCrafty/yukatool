using System;
using System.IO;
using Yuka.Data;
using Yuka.Data.Factory;

namespace Yuka.Tasks {
	class WrapTask : Task {
		public override Task NewTask() {
			return new WrapTask();
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
				files = Directory.GetFiles(sourceBasePath, "*." + Constants.unwrappedGraphicsExtension, SearchOption.AllDirectories);
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
				string localPath = sourcePath.Substring(sourceBasePath.Length).TrimStart('\\').ToLower();
				string targetPath = Path.ChangeExtension(Path.Combine(targetBasePath, localPath), Constants.wrappedGraphicsExtension);

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

				using(FileStream fs = new FileStream(targetPath, FileMode.Create)) {

					YukaGraphics graphics = GraphicsFactory.Instance.FromSource(sourcePath);
					GraphicsFactory.Instance.ToBinary(graphics, fs);

				}
			}
			currentFile = "";

			if(flags.Has('w')) {
				Console.ReadLine();
			}
		}
	}
}
