using System.Collections.Generic;
using System.Text;
using Yuka.Script;

namespace Yuka.Data {
	class YukaScript : YukaFile {
		public List<ScriptElement> commands;
		public Dictionary<string, string> stringTable;

		public YukaScript(List<ScriptElement> commands, Dictionary<string, string> stringTable) : base(DataType.Script) {
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
