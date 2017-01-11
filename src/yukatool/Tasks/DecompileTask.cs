using System;
using System.IO;
using Yuka.Script;

namespace Yuka.Tasks {
	class DecompileTask : Task {
		public override Task NewTask() {
			return new DecompileTask();
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
				files = Directory.GetFiles(sourceBasePath, "*." + Constants.compiledScriptExtension, SearchOption.AllDirectories);
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
				string targetPath = Path.ChangeExtension(Path.Combine(targetBasePath, localPath), Constants.decompiledScriptExtension);
				string metaPath = Path.ChangeExtension(Path.Combine(targetBasePath, localPath), Constants.stringMetaExtension);

				currentFile = localPath;

				if(flags.Has('v')) {
					Console.WriteLine("SourceBase: " + sourceBasePath);
					Console.WriteLine("TargetBase: " + targetBasePath);
					Console.WriteLine("Source:     " + sourcePath);
					Console.WriteLine("Target:     " + targetPath);
					Console.WriteLine("Local:      " + localPath);
					Console.WriteLine();
				}

				Decompiler decomp = new Decompiler();
				FileStream fs = new FileStream(sourcePath, FileMode.Open);
				ScriptInstance script = decomp.FromBinary(fs);
				fs.Close();

				string source = script.Source();

				/*if(flags.Has('v')) {
					Console.Write(source);
				}*/

				Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

				StreamWriter w = new StreamWriter(new FileStream(targetPath, FileMode.Create));
				w.Write(source);
				w.Close();

				if(script.stringTable.Count > 0) {
					w = new StreamWriter(new FileStream(metaPath, FileMode.Create));
					w.WriteLine("ID,Speaker,Original,Translation,TLC,Edit,QC,Comments,Generated");
					// write names
					bool flag = false;
					foreach(var entry in script.stringTable) {
						if(entry.Key.StartsWith("N")) {
							if(!flag) {
								w.WriteLine("\n#Names:");
								flag = true;
							}
							w.WriteLine(entry.Key + ",," + entry.Value);
						}
					}
					// write lines
					flag = false;
					foreach(var entry in script.stringTable) {
						if(!entry.Key.StartsWith("N")) {
							if(!flag) {
								w.WriteLine("\n#Lines:");
								flag = true;
							}

							string speaker = "";
							string line = entry.Value;

							int index = line.IndexOf('|');
							if(index >= 0) {
								speaker = line.Substring(0, index);
								line = line.Substring(index + 1);
							}

							line = "\"" + line.Replace("\"", "\"\"") + "\"";

							w.WriteLine(entry.Key + ',' + speaker + ',' + line);
						}
					}
					w.Close();
				}
			}
			currentFile = "";
			if(flags.Has('w')) {
				Console.ReadLine();
			}
		}
	}
}
