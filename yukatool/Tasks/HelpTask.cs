using System;

namespace yuka.Tasks {
	class HelpTask : Task {
		public override Task NewTask() {
			return new HelpTask();
		}

		public override void DefaultFlags(FlagCollection flags) {
		}

		protected override void Execute() {
			if(arguments.Length == 0) {
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Usage: yuka.exe <task> [flags] <args>");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Available tasks:");
				Console.ForegroundColor = ConsoleColor.Cyan;
				foreach(var entry in Task.registeredTasks) {
					Console.WriteLine("  " + entry.Key);
				}
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("For a description of the individual tasks run \"yuka.exe help <task>\"");
				Console.ResetColor();
			}
			else {
				Console.WriteLine("Usage: yuka.exe <task> [flags] <args>");
				Console.WriteLine("Task help is still WIP");
			}
			Console.ReadLine();
		}
	}
}
