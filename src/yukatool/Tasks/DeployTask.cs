using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Yuka.Data;
using Yuka.Data.Factory;

namespace Yuka.Tasks {
	class DeployTask : Task {
		public override Task NewTask() {
			return new DeployTask();
		}

		public DeployTask() { }
		public DeployTask(string[] arguments, FlagCollection flags) : base(arguments, flags) { }

		public override void DefaultFlags(FlagCollection flags) {
			flags.Add('v', "verbose", "Outputs additional information", false);
			flags.Add('w', "wait", "Waits for enter before closing the console", false);
			flags.Add('i', "incremental", "Only include files that have changed. Requires a deploy", false);
		}

		protected override void Execute() {
			// init patch directory
			if(flags.Has("init")) {
				string fetchUrl = arguments.Length > 0 ? arguments[0] : "";
				string gameDirectory = arguments.Length > 1 ? Helpers.AbsolutePath(arguments[1]) : Directory.GetCurrentDirectory();
				string patchDirectory = arguments.Length > 2 ? Helpers.AbsolutePath(arguments[2]) : Path.Combine(gameDirectory, "autopatch");
				string patchFilePath = Path.ChangeExtension(patchDirectory, Constants.ypl);

				Directory.CreateDirectory(patchDirectory);

				dynamic info = new JObject();
				info.gamedir = gameDirectory;
				info.patchdir = patchDirectory;
				info.fetchurl = fetchUrl;
				info.history = new JArray();

				File.WriteAllText(patchFilePath, JsonConvert.SerializeObject(info));

				Console.WriteLine("GameDir:   " + gameDirectory);
				Console.WriteLine("PatchDir:  " + patchDirectory);
				Console.WriteLine("PatchFile: " + patchFilePath);
			}

			else {
				if(arguments.Length == 0) Fail("No target file specified");
				string targetBasePath = Helpers.AbsolutePath(arguments[0]);

				// patch archive
				if(targetBasePath.EndsWith(Constants.ykc, StringComparison.OrdinalIgnoreCase)) {
					if(arguments.Length == 1) Fail("No patch file specified");
					string patchBasePath = Helpers.AbsolutePath(arguments[1]);

					if(!File.Exists(targetBasePath)) Fail("The specified target archive does not exist");
					if(!File.Exists(patchBasePath)) Fail("The specified patch archive does not exist");

					Patch(targetBasePath, patchBasePath);
				}

				// patch file
				else if(targetBasePath.EndsWith(Constants.ypl, StringComparison.OrdinalIgnoreCase)) {
					dynamic info = JsonConvert.DeserializeObject(File.ReadAllText(targetBasePath));
					string gameDirectory = info.gamedir;
					string patchDirectory = info.patchdir;
					string fetchUrl = info.fetchurl;

					List<string> history = new List<string>();
					foreach(string f in info.history) {
						history.Add(f);
					}

					if(!Directory.Exists(gameDirectory)) Fail("Game directory does not exist");
					if(!Directory.Exists(patchDirectory)) Fail("Patch directory does not exist");

					// download new patch files
					int finishedDownloads = 0, skippedDownloads = 0;
					if(fetchUrl != null && fetchUrl.Length > 2) {
						using(WebClient web = new WebClient()) {
							Log("Fetching patch list from " + fetchUrl, ConsoleColor.Yellow);
							string[] remoteFiles = web.DownloadString(fetchUrl).Split('\n');

							foreach(string fileUri in remoteFiles) {
								if(fileUri == "") continue;
								string fileName = Path.GetFileName(fileUri);
								string filePath = Path.Combine(patchDirectory, fileName);
								if(File.Exists(filePath)) {
									skippedDownloads++;
								}
								else {
									Log("Downloading " + fileName, ConsoleColor.Yellow);
									web.DownloadFile(fileUri, filePath);
									finishedDownloads++;
								}
							}
						}
					}
					if(skippedDownloads + finishedDownloads > 0) {
						Log("Downloaded " + finishedDownloads + " patches, skipped " + skippedDownloads, ConsoleColor.Green);
					}

					// apply patch files
					int appliedPatches = 0, skippedPatches = 0;
					string[] localFiles = Directory.GetFiles(patchDirectory, "*." + Constants.ykc);
					foreach(string filePath in localFiles) {
						string fileName = Path.GetFileName(filePath);
						if(history.Contains(fileName)) {
							skippedPatches++;
						}
						else {
							bool found = false;
							for(int i = 1; i <= 5; i++) {
								string archiveName = "data0" + i;
								string archivePath = Path.Combine(gameDirectory, archiveName + '.' + Constants.ykc);
								if(fileName.Contains(archiveName) && File.Exists(archivePath)) {
									if(flags.Has('v')) {
										Console.ForegroundColor = ConsoleColor.Yellow;
										Console.WriteLine("Applying patch " + fileName);
										Console.ResetColor();
									}
									found = true;
									Patch(archivePath, filePath);
									appliedPatches++;
									break;
								}
							}
							if(found) {
								history.Add(fileName);
							}
							else {
								Log("Could not determine target archive for patch '" + fileName + "'", ConsoleColor.Red);
							}
						}
					}
					if(skippedPatches + appliedPatches > 0) {
						Log("Applied " + appliedPatches + " patches, skipped " + skippedPatches, ConsoleColor.Green);
					}

					info.history = JArray.FromObject(history.ToArray());
					File.WriteAllText(targetBasePath, JsonConvert.SerializeObject(info));
				}
			}

			if(flags.Has('w')) {
				Console.ReadLine();
			}
		}

		void Patch(string targetBasePath, string patchBasePath) {
			YukaArchive target = ArchiveFactory.Instance.FromSource(targetBasePath);
			YukaArchive source = ArchiveFactory.Instance.FromSource(patchBasePath);

			if(target == null) Fail("Failed to read target archive.");
			if(source == null) Fail("Failed to read source archive.");

			foreach(var entry in source.files) {
				string localPath = entry.Key;
				currentFile = localPath;

				target.files[entry.Key] = entry.Value;

				/*if(flags.Has('v')) {
					Console.WriteLine();
					Console.WriteLine("TargetBase: " + targetBasePath);
					Console.WriteLine("PatchBase:  " + patchBasePath);
					Console.WriteLine("Local:      " + localPath);
				}*/
			}
			currentFile = "";

			ArchiveFactory.Instance.ToSource(target, targetBasePath);
		}
	}
}
