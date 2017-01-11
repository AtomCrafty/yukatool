using System;
using System.Collections.Generic;
using yuka.Tasks;

namespace yuka {
	class Program {

		static void Setup() {
			// Register tasks
			Task.Register("help", new HelpTask());
			Task.Register("split", new SplitTask());
			Task.Register("decompile", new DecompileTask());
			Task.Register("compile", new CompileTask());
			Task.Register("pack", new PackTask());

			// Default task: help
			Task.SetDefault("help");
		}

		static void Run(string[] args) {
			if(DateTime.Now > new DateTime(2017, 3, 1)) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("This copy of YukaTool has beed deactivated.");
				Console.WriteLine("Please contact atomcrafty@frucost.net to retrieve the current version.");
				Console.ResetColor();
				Console.ReadLine();
				return;
			}

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
