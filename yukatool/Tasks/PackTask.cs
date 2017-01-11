using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using yuka.File;

namespace yuka.Tasks {
	class PackTask : Task {
		public override Task NewTask() {
			return new PackTask();
		}

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

			Archive archive = new Archive();

			for(int i = 0; i < files.Length; i++) {
				string sourcePath = files[i];
				string localPath = sourcePath.Substring(sourceBasePath.Length).TrimStart('\\').ToLower();

				currentFile = localPath;

				if(flags.Has('v')) {
					Console.WriteLine("SourceBase: " + sourceBasePath);
					Console.WriteLine("TargetBase: " + targetBasePath);
					Console.WriteLine("Source:     " + sourcePath);
					Console.WriteLine("Local:      " + localPath);
					Console.WriteLine();
				}

				FileStream fs = new FileStream(sourcePath, FileMode.Open);
				MemoryStream ms = archive.GetOutputStream(localPath);
				fs.CopyTo(ms);
				fs.Close();
			}
			currentFile = "";

			FileStream outstream = new FileStream(targetBasePath, FileMode.Create);
			ArchiveIO.Write(archive, outstream);
			outstream.Close();

			if(flags.Has('w')) {
				Console.ReadLine();
			}
		}
	}
}
