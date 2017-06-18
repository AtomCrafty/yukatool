using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yuka.Data;
using Yuka.Data.Factory;
using Yuka.Script;

namespace Yuka.Tasks {
	class AutoTask : Task {
		public override Task NewTask() {
			return new AutoTask();
		}

		public override void DefaultFlags(FlagCollection flags) {
			flags.Add('v', "verbose", "Outputs additional information", false);
			flags.Add('w', "wait", "Waits for enter before closing the console", false);
		}

		protected override void Execute() {
			if(arguments.Length == 0) {
				Task task = new HelpTask();
				task.arguments = new string[0];
				task.flags = flags;
				task.Run();
			}
			else if(arguments[0].EndsWith(".patch.ykc")) {
				// patch mode
				Task task = null;
				for(int i = 1; i <= 5; i++) {
					if(Path.GetFileName(arguments[0]).Contains(".data0" + i + '.')) {
						if(File.Exists(Path.Combine(Path.GetDirectoryName(arguments[0]), "data0" + i + ".ykc"))) {
							flags.Set('v');
							task = new PatchTask(new[] { arguments[0], Path.Combine(Path.GetDirectoryName(arguments[0]), "data0" + i + ".ykc") }, flags);
						}
						else if(File.Exists(Helpers.AbsolutePath("data0" + i + ".ykc"))) {
							flags.Set('v');
							task = new PatchTask(new[] { arguments[0], Path.Combine(Directory.GetCurrentDirectory(), "data0" + i + ".ykc") }, flags);
						}
						break;
					}
				}
				if(task == null) {
					Fail("Unable to determine patch target");
				}
				else {
					task.Run();
				}
				Console.ReadLine();
			}
			else if(Path.GetExtension(arguments[0]) == '.' + Constants.ykg) {
				string path = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetFileName(arguments[0]), Constants.png));
				using(FileStream fs = new FileStream(Helpers.AbsolutePath(arguments[0]), FileMode.Open)) {
					YukaGraphics g = GraphicsFactory.Instance.FromBinary(fs);
					g.bitmap.Save(path);
					System.Diagnostics.Process.Start(path);
				}
			}
			else if(Directory.Exists(arguments[0])) {
				string patchLogPath = Path.Combine(Helpers.AbsolutePath(arguments[0]), "patch.ypl");
				if(File.Exists(patchLogPath)) {
					// TODO
				}
			}
			else if(Path.GetExtension(arguments[0]) == '.' + Constants.ypl) {
				flags.Set('v');
				flags.Set('w');
				new PatchTask(arguments, flags).Run();
			}
			else if(Path.GetExtension(arguments[0]) == '.' + Constants.ydr) {
				flags.Set('v');
				flags.Set('w');
				new DeployTask(arguments, flags).Run();
			}
			else {
				Fail("Unable to auto determine processing method");
			}
		}
	}
}
