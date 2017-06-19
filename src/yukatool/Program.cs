using System;
using System.IO;
using System.Reflection;
using Yuka.Tasks;

namespace Yuka {
	class Program {

		static void Setup() {
			// Register tasks
			Task.Register("help", new HelpTask());
			Task.Register("split", new SplitTask());
			Task.Register("decompile", new DecompileTask());
			Task.Register("compile", new CompileTask());
			Task.Register("unpack", new UnpackTask());
			Task.Register("patch", new PatchTask());
			Task.Register("pack", new PackTask());
			Task.Register("unwrap", new UnwrapTask());
			Task.Register("wrap", new WrapTask());
			Task.Register("deploy", new DeployTask());
			Task.Register("analyze", new AnalyzeTask());

			Task.Register("auto", new AutoTask());

			Task.SetDefault("auto");
		}

		static void Run(string[] args) {
#if !DEBUG
			try {
#endif
				Task task = Task.Create(args);
				task.Run();
#if !DEBUG
			}
			catch(Exception e) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(e.Message);
				Console.ResetColor();
				Console.ReadLine();
			}
#endif
		}

		static void Main(string[] args) {
			Setup();
			Run(args);
		}

		static Program() {
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Resolver);
		}

		static Assembly Resolver(object sender, ResolveEventArgs args) {
			string resourceName = "Yuka." + args.Name.Split(',')[0] + ".dll";

			Assembly ea = Assembly.GetExecutingAssembly();
			Stream s = ea.GetManifestResourceStream(resourceName);
			byte[] block = new byte[s.Length];
			s.Read(block, 0, block.Length);
			Assembly la = Assembly.Load(block);
			return la;
		}
	}
}
