using Yuka.Tasks;

namespace Yuka {
	class Program {
		static void Setup() {
			// Register tasks
			Task.Register("help", new HelpTask());
			Task.Register("split", new SplitTask());
			Task.Register("decompile", new DecompileTask());
			Task.Register("compile", new CompileTask());
			Task.Register("pack", new PackTask());
			Task.Register("unpack", new UnpackTask());

			Task.SetDefault("help");
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
	}
}
