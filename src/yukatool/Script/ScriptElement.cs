using System;
using System.Collections.Generic;
using System.Text;

namespace Yuka.Script {
	abstract class ScriptElement {

	}

	class FuncCallScriptElement : ScriptElement {
		public string name;
		public ScriptElement[] parameters;

		public FuncCallScriptElement(string name, ScriptElement[] parameters) {
			this.name = name;
			this.parameters = parameters;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder(name).Append('(');
			bool flag = false;
			foreach(ScriptElement param in parameters) {
				if(flag) {
					sb.Append(", ");
				}
				sb.Append(param.ToString());
				flag = true;
			}
			return sb.Append(')').ToString();
		}
	}

	class DataScriptElement : ScriptElement {
		public DataElement elem;

		public DataScriptElement(DataElement elem) {
			this.elem = elem;
		}

		public override string ToString() {
			return elem.ToString();
		}
	}

	class AssignmentScriptElement : ScriptElement {
		public DataElement var;
		public ScriptElement expression;

		public AssignmentScriptElement(DataElement var, ScriptElement expression) {
			this.var = var;
			this.expression = expression;
		}

		public override string ToString() {
			if(var is IntDataElement) {
				return "$ = " + expression;
			}
			return var + " = " + expression;
		}
	}

	class ExpressionScriptElement : ScriptElement {
		public string[] operators;
		public ScriptElement[] parameters;

		public ExpressionScriptElement(ScriptElement[] parameters) {
			//this.parameters = parameters;
			List<string> ops = new List<string>();
			List<ScriptElement> elements = new List<ScriptElement>();
			foreach(ScriptElement param in parameters) {
				if(param is DataScriptElement && (param as DataScriptElement).elem is ControlDataElement) {
					string name = ((param as DataScriptElement).elem as ControlDataElement).name;
					if("=".Equals(name)) {
						name = "==";
					}
					ops.Add(name);
				}
				else {
					elements.Add(param);
				}
			}
			if(ops.Count != elements.Count - 1) {
				if(FlagCollection.current.Has("strict")) {
					throw new Exception("Operator / parameter count incompatible");
				}
				else {
					Console.Error.WriteLine("Warning: incompatible operator / parameter count in " + Task.currentTask.currentFile);
				}
			}
			this.operators = ops.ToArray();
			this.parameters = elements.ToArray();
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder().Append('(').Append(parameters[0].ToString());
			for(int i = 1; i < parameters.Length; i++) {
				if(i <= operators.Length) {
					sb.Append(' ').Append(operators[i - 1]);
				}
				sb.Append(' ').Append(parameters[i]);
			}
			return sb.Append(')').ToString();
		}
	}

	class BranchScriptElement : ScriptElement {
		public ScriptElement condition;
		public List<ScriptElement> truebody;
		public List<ScriptElement> falsebody;

		public BranchScriptElement(ScriptElement condition, List<ScriptElement> truebody, List<ScriptElement> falsebody) {
			this.condition = condition;
			this.truebody = truebody;
			this.falsebody = falsebody;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder("if(").Append(condition).Append(") {\n");
			foreach(ScriptElement elem in truebody) {
				if(elem != null) {
					sb.Append("  ").Append(elem.ToString().Replace("\n", "\n  ")).Append('\n');
				}
			}
			if(falsebody != null) {
				sb.Append("}\nelse {\n");
				foreach(ScriptElement elem in falsebody) {
					if(elem != null) {
						sb.Append("  ").Append(elem.ToString().Replace("\n", "\n  ")).Append('\n');
					}
				}
			}
			return sb.Append('}').ToString();
		}
	}

	class SwitchFunctionScriptElement : ScriptElement {
		public string name;
		public ScriptElement[] parameters;
		public List<ScriptElement> body;

		public SwitchFunctionScriptElement(string name, ScriptElement[] parameters, List<ScriptElement> body) {
			this.name = name;
			this.parameters = parameters;
			this.body = body;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder(name).Append('(');
			bool flag = false;
			foreach(ScriptElement param in parameters) {
				if(flag) {
					sb.Append(", ");
				}
				sb.Append(param.ToString());
				flag = true;
			}
			sb.Append(") {\n");
			foreach(ScriptElement elem in body) {
				if(elem != null) {
					sb.Append("  ").Append(elem.ToString().Replace("\n", "\n  ")).Append('\n');
				}
			}
			return sb.Append('}').ToString();
		}
	}

	class JumpLabelScriptElement : ScriptElement {
		public string name;

		public JumpLabelScriptElement(string name) {
			this.name = name;
		}

		public override string ToString() {
			return "\n:" + name;
		}
	}

	class DummyScriptElement : ScriptElement {
		public override string ToString() {
			return "# Dummy";
		}
	}

	class EndScriptElement : ScriptElement {
		public override string ToString() {
			return "}";
		}
	}
}
