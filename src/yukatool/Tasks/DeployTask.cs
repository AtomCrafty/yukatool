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
			flags.Add('i', "init", "Initialize a new deployment resource");
		}

		protected override void Execute() {
			// init patch directory
			if(flags.Has("init")) {
				string deployDirectory = Helpers.AbsolutePath(arguments.Length > 0 ? arguments[0] : "autopatch");
				string deployFilePath = Path.ChangeExtension(deployDirectory, Constants.ydr);
				Dictionary<string, string> targets = new Dictionary<string, string>();
				for(int i = 1; i < arguments.Length; i++) {
					// parse targets in format name:path or just path
					string[] tmp = arguments[i].Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
					string name = tmp.Length > 1 && tmp[0].Length > 1 ? tmp[0] : Path.GetFileName(tmp[0]);
					string path = Helpers.AbsolutePath(tmp.Length > 1 && tmp[0].Length > 1 ? tmp[1] : tmp[0]);
					targets.Add(name, path);
				}
				if(targets.Count == 0) Fail("No target directories specified");

				Directory.CreateDirectory(deployDirectory);

				int fileCount = 0;

				dynamic info = new JObject();
				info.build = 0;
				info.nameschema = "inc.{buildno}.{target}.patch.ykc";
				info.deploydir = deployDirectory;
				info.targets = new JArray();
				foreach(var targetInfo in targets) {
					string targetName = targetInfo.Key;
					string targetPath = targetInfo.Value;
					string[] files = Directory.GetFiles(targetPath, "*", SearchOption.AllDirectories);

					dynamic target = new JObject();
					{
						target.name = targetName;
						target.path = targetPath;
						target.files = new JArray();
						foreach(string file in files) {
							// ignore unknown file types and hidden files
							if(!DataTypes.ForExtension(DataTypes.BaseExtension(Path.GetExtension(file))).IncludeInArchive()) continue;
							if(new FileInfo(file).Attributes.HasFlag(FileAttributes.Hidden)) continue;

							dynamic entry = new JObject();
							{
								entry.name = Helpers.RelativePath(file, targetPath);
								entry.size = Helpers.FileSize(file);
								entry.hash = Helpers.FileHash(file);
								entry.version = 0;
							}
							target.files.Add(entry);

							fileCount++;
						}
					}
					info.targets.Add(target);
					Log("Added target '" + targetName + "' (" + target.files.Count + " files) [" + targetPath + "]", ConsoleColor.Yellow);
				}
				Log("Successfully added " + fileCount + " file in " + info.targets.Count + " targets", ConsoleColor.Green);

				File.WriteAllText(deployFilePath, JsonConvert.SerializeObject(info, Formatting.Indented));
			}

			else {
				if(arguments.Length == 0) Fail("No target file specified");
				string deployFilePath = Helpers.AbsolutePath(arguments[0]);

				int changedFiles = 0, newFiles = 0;

				if(deployFilePath.EndsWith(Constants.ydr, StringComparison.OrdinalIgnoreCase)) {
					dynamic info = JsonConvert.DeserializeObject(File.ReadAllText(deployFilePath));
					info.build = (int)info.build + 1;

					foreach(var target in info.targets) {
						List<string> includedFiles = new List<string>();
						string[] files = Directory.GetFiles((string)target.path, "*", SearchOption.AllDirectories);
						foreach(string file in files) {
							string localName = Helpers.RelativePath(file, (string)target.path);

							// ignore unknown file types and hidden files
							if(!DataTypes.ForExtension(DataTypes.BaseExtension(Path.GetExtension(localName))).IncludeInArchive()) continue;
							if(new FileInfo(file).Attributes.HasFlag(FileAttributes.Hidden)) continue;

							bool include = true, exists = false;
							foreach(var localFile in target.files) {
								if(localName.Equals((string)localFile.name, StringComparison.OrdinalIgnoreCase)) {
									exists = true;
									string hash = Helpers.FileHash(file);
									long size = Helpers.FileSize(file);

									if(size != (long)localFile.size || !hash.Equals((string)localFile.hash)) {
										// update file entry if files differ
										localFile.size = size;
										localFile.hash = hash;
										localFile.version = (int)localFile.version + 1;
									}
									else include = false;
									break;
								}
							}
							if(!exists) {
								Log("[New file]   " + localName, ConsoleColor.Green);
								dynamic entry = new JObject();
								{
									entry.name = localName;
									entry.size = Helpers.FileSize(file);
									entry.hash = Helpers.FileHash(file);
									entry.version = 1;
								}
								target.files.Add(entry);
								newFiles++;
							}
							if(include) {
								string mainFile = Path.ChangeExtension(localName, DataTypes.BaseExtension(Path.GetExtension(localName)));
								if(!includedFiles.Contains(mainFile)) {
									includedFiles.Add(mainFile);
									if(exists) Log("[Changed]    " + localName, ConsoleColor.Yellow);
									if(exists) changedFiles++;
								}
							}
							else {
								Log("[Up to date] " + localName, ConsoleColor.Red);
							}
							// Log(include ? exists ? "Changed" : "New file" : "Up to date", ConsoleColor.Cyan);
						}

						if(includedFiles.Count > 0) {
							bool verbose = flags.Has('v');
							flags.Unset('v');
							YukaArchive archive = new YukaArchive();
							foreach(string file in includedFiles) {
								dynamic factory = FileFactory.ForExtension(Path.GetExtension(file));
								if(factory != null) {
									string realname = Path.ChangeExtension(file, DataTypes.ForExtension(Path.GetExtension(file)).BinaryExtension());
									Console.WriteLine(realname);
									dynamic data = factory.FromSource(Path.Combine((string)target.path, file));
									MemoryStream ms = new MemoryStream();
									factory.ToBinary(data, ms);
									archive.files[realname] = ms;
								}
							}
							string name = (string)info.nameschema;

							name = name.Replace("{buildno}", ((int)info.build).ToString("D3"));
							name = name.Replace("{target}", ((string)target.name));
							name = name.Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd"));
							name = name.Replace("{time}", DateTime.Now.ToString("HH-mm-ss"));

							ArchiveFactory.Instance.ToSource(archive, Path.Combine((string)info.deploydir, name));
							if(verbose) flags.Set('v');
						}
					}

					Log("Deployed " + newFiles + " new files and " + changedFiles + " updates", ConsoleColor.Green);

					File.WriteAllText(deployFilePath, JsonConvert.SerializeObject(info, Formatting.Indented));
				}
			}

			if(flags.Has('w')) {
				Console.ReadLine();
			}
		}
	}
}
