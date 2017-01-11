using System;
using System.Collections.Generic;
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
			
			Task.SetDefault("help");
		}

		static void Run(string[] args) {
			try {
				Task task = Task.Create(args);
				task.Run();
			}
			catch(Exception e) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(e.Message);
				Console.ResetColor();
			}
		}

		static void Main(string[] args) {
			Setup();
			Run(args);
		}
	}
}
