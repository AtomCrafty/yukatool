using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yuka.Script {
	class ScriptInstance {
		public List<ScriptElement> commands;
		public Dictionary<string, string> stringTable;

		public ScriptInstance(List<ScriptElement> commands, Dictionary<string, string> stringTable) {
			this.commands = commands;
			this.stringTable = stringTable;
		}

		public string Source() {
			StringBuilder sb = new StringBuilder();
			foreach(ScriptElement elem in commands) {
				sb.AppendLine(elem.ToString());
			}
			return sb.ToString();
		}
	}
}
