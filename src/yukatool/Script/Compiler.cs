using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Yuka.Data;

namespace Yuka.Script {
	class Compiler {
		/// <summary>
		/// The table of key - string associations
		/// </summary>
		Dictionary<string, string> stringTable = new Dictionary<string, string>();

		/// <summary>
		/// The table of jump label IDs and their target control element
		/// </summary>
		Dictionary<string, ControlDataElement> jumpLabels = new Dictionary<string, ControlDataElement>();

		/// <summary>
		/// Indicates how many temporary variables have been used so far
		/// </summary>
		int tempVarID = 0;

		/// <summary>
		/// Parses an entire script (.ykd) with optional attached metadata (.csv)
		/// </summary>
		/// <param name="scriptPath">Absolute path of the script file (.ykd)</param>
		/// <param name="stringPath">Absolute path of the meta attachment (.csv)</param>
		/// <returns>The parsed script</returns>
		public YukaScript FromSource(string scriptPath, string stringPath) {

			#region Import meta information
			if(File.Exists(stringPath)) {
				#region Parse CSV
				StreamReader sr;
				int offset = -1;

				bool quoted = false;
				bool justLeftQuote = false;

				List<List<string>> rows = new List<List<string>>();
				List<string> row = new List<string>();
				StringBuilder cell = new StringBuilder();

				sr = new StreamReader(new FileStream(stringPath, FileMode.Open));

				while(!sr.EndOfStream) {
					offset++;
					char ch = (char)sr.Read();
					switch(ch) {
						case '"': {
								if(quoted) {
									quoted = false;
									justLeftQuote = true;
									continue;
								}
								else if(justLeftQuote) {
									quoted = true;
									cell.Append('"');
								}
								else {
									quoted = true;
								}
								break;
							}
						case ',': {
								if(quoted) {
									cell.Append(',');
								}
								else {
									row.Add(cell.ToString());
									cell.Clear();
								}
								break;
							}
						case '\r': {
								// do nothing
								break;
							}
						case '\n': {
								if(quoted) {
									cell.Append('\n');
								}
								else {
									row.Add(cell.ToString());
									rows.Add(row);
									row = new List<string>();
									cell.Clear();
								}
								break;
							}
						default: {
								cell.Append(ch);
								break;
							}
					}
					justLeftQuote = false;
				}

				sr.Close();

				row.Add(cell.ToString());
				rows.Add(row);
				#endregion

				#region Populate string table
				//int metaColumnID = 7;
				int maxTextColumn = 6;
				int minTextColumn = 2;

				int defaultLineWidth = TextUtils.defaultLineWidth;
				bool removeQuotes = true;

				foreach(List<string> entry in rows) {
					#region Handle row
					int lineWidth = defaultLineWidth;
					if(entry[0].Length > 0 && entry[0][0] == '!') {
						entry[0] = entry[0].TrimStart('!');
						if(entry.Count > 1) {
							string[] metaData = entry[1].Split('\n');
							string speaker = "";
							foreach(string meta in metaData) {
								string[] data = meta.Split(new[] { ':' }, 2);
								if(data.Length == 2) {
									switch(data[0].ToLower()) {
										case "speaker":
											speaker = data[1];
											break;
										case "w":
										case "cpl":
										case "width":
											lineWidth = int.Parse(data[1]);
											break;
										case "dw":
										case "dcpl":
										case "dwidth":
										case "defaultwidth":
										case "defaultlinewidth":
											defaultLineWidth = int.Parse(data[1]);
											lineWidth = defaultLineWidth;
											break;
										case "rmq":
										case "rmquotes":
										case "removequotes":
											removeQuotes = bool.Parse(data[1]);
											break;
									}
								}
							}
							entry[1] = speaker;
						}
					}

					if(entry[0].Length > 0 && "LNS".Contains(entry[0][0].ToString())) {
						/*if(entry.Count > metaColumnID && entry[metaColumnID].Trim().Length > 0) {
							Console.WriteLine(entry[0] + ": " + entry[metaColumnID].Trim('\n'));
							// stringTable[entry[0]] = entry[metaColumnID].Trim('\n');
							// continue;
						}*/

						for(int i = maxTextColumn; i >= minTextColumn; i--) {
							if(i < entry.Count && !entry[i].Equals("") && !entry[i].Equals(".")) {
								string value = entry[i];

								if(entry[0][0] == 'L') {



									value = value.Trim('\n', '%', '#', '「', '」');
									if(value.Length > 1 && value[0] == '"' && value[value.Length - 1] == '"' && removeQuotes) {
										value = value.Substring(1, value.Length - 2);
									}

									// get rid of most japanese double width punctuation characters
									value = TextUtils.ReplaceSpecialChars(value);

									// replace 2 or more dots by exactly 3
									value = Regex.Replace(value, @"…|[…\.]{2,}", "...");
									// add a space after ellipses that don't already have one next to them
									value = Regex.Replace(value, @"(?!^|[\s""])\.\.\.(?=\w)", "... ");





									value = TextUtils.WrapWords(value, lineWidth);
								}

								//Console.WriteLine(entry[0] + ": " + value);
								stringTable[entry[0]] = value;
								break;
							}
						}
					}
					#endregion
				}
				#endregion
			}
			#endregion

			#region Parse script file
			BinaryReader br = new BinaryReader(new FileStream(scriptPath, FileMode.Open));
			List<ScriptElement> commands = new List<ScriptElement>();

			SkipWhitespace(br);

			ScriptElement elem;
			while((elem = NextScriptElement(br)) != null) {
				if(!(elem is DummyScriptElement)) {
					commands.Add(elem);
					if(FlagCollection.current.Has('v')) {
						Console.WriteLine(elem);
					}
				}
			}
			br.Close();
			#endregion

			return new YukaScript(commands, stringTable);
		}

		/// <summary>
		/// Parses a single script element.
		/// </summary>
		/// <param name="br">The BinaryReader used to read characters</param>
		/// <returns>The parsed script element</returns>
		ScriptElement NextScriptElement(BinaryReader br) {
			if(br.BaseStream.Position == br.BaseStream.Length) return null;

			ScriptElement elem = NextValue(br);
			SkipWhitespace(br);

			if(elem is DataScriptElement && (elem as DataScriptElement).elem is ControlDataElement) {
				return new JumpLabelScriptElement(((elem as DataScriptElement).elem as ControlDataElement).name);
			}

			switch(br.PeekChar()) {
				#region Assignment
				case '=':
					// assignment logic
					br.ReadChar();
					SkipWhitespace(br);
					ScriptElement value = NextValue(br);
					return new AssignmentScriptElement((elem as DataScriptElement).elem, value);
				#endregion
				#region Code block
				case '{':
					// nested commands logic
					br.ReadChar();
					SkipWhitespace(br);

					#region Read statements
					List<ScriptElement> body = new List<ScriptElement>();
					ScriptElement next;
					while((next = NextScriptElement(br)) != null && !(next is EndScriptElement)) {
						if(!(elem is DummyScriptElement)) {
							body.Add(next);
						}
					}
					#endregion

					#region If statement
					if((elem as FuncCallScriptElement).name.Equals("if")) {
						// check for else
						SkipWhitespace(br);
						long pos = br.BaseStream.Position;
						if(NextIdentifier(br).Equals("else")) {
							SkipWhitespace(br);
							// opening brace
							br.ReadChar();
							SkipWhitespace(br);
							List<ScriptElement> falsebody = new List<ScriptElement>();

							while((next = NextScriptElement(br)) != null && !(next is EndScriptElement)) {
								if(!(elem is DummyScriptElement)) {
									falsebody.Add(next);
								}
							}

							return new BranchScriptElement((elem as FuncCallScriptElement).parameters[0], body, falsebody);
						}
						else {
							br.BaseStream.Seek(pos, SeekOrigin.Begin);
							return new BranchScriptElement((elem as FuncCallScriptElement).parameters[0], body, null);
						}
					}
					#endregion
					#region Switch statement
					return new SwitchFunctionScriptElement((elem as FuncCallScriptElement).name, (elem as FuncCallScriptElement).parameters, body);
				#endregion
				#endregion
				default:
					return elem;
			}
		}

		/// <summary>
		/// Parses a single expression
		/// </summary>
		/// <param name="br">The BinaryReader used to read characters</param>
		/// <returns>The parsed script element</returns>
		ScriptElement NextValue(BinaryReader br) {
			SkipWhitespace(br);
			int ch = br.PeekChar();
			switch(ch) {
				#region End of file
				case -1:
					// end of stream
					return null;
				#endregion
				#region End of code block
				case '}':
					// end code block
					br.ReadChar();
					return new EndScriptElement();
				#endregion
				#region Comment
				case '#':
					// skip comment
					while(br.ReadChar() != '\n') ;
					return new DummyScriptElement();
				#endregion
				#region String literal
				case '"':
					// read string literal
					StringBuilder sb = new StringBuilder();
					br.ReadChar();
					while(true) {
						ch = br.ReadChar();
						if(ch == '"') {
							if(br.PeekChar() == '"') {
								sb.Append(br.ReadChar());
							}
							else {
								break;
							}
						}
						else {
							sb.Append((char)ch);
						}
					}
					return new DataScriptElement(new StringDataElement(sb.ToString()));
				#endregion
				#region Jump label
				case ':':
					// read jump label
					br.ReadChar();
					return new DataScriptElement(new ControlDataElement(NextIdentifier(br)));
				#endregion
				#region Externalized string
				case '@':
					// read externalized string
					br.ReadChar();
					return new DataScriptElement(new ExternalStringDataElement(NextIdentifier(br), stringTable));
				#endregion
				#region Expression
				case '(':
					// read expression
					br.ReadChar();
					List<ScriptElement> parameters = new List<ScriptElement>();

					ScriptElement next = NextValue(br);
					parameters.Add(next);
					SkipWhitespace(br);

					while(br.PeekChar() != ')') {
						string op = "";
						while(br.PeekChar() != ' ') {
							op += br.ReadChar();
						}
						parameters.Add(new DataScriptElement(new ControlDataElement(op)));
						SkipWhitespace(br);
						parameters.Add(NextValue(br));
						SkipWhitespace(br);
					}
					br.ReadChar();

					return new ExpressionScriptElement(parameters.ToArray());
				#endregion
				default:
					if(char.IsDigit((char)ch) || ch == '-') {
						#region Integer literal
						// read int literal
						string value = "";
						do {
							value += (char)ch;
							br.ReadChar();
						}
						while(char.IsDigit((char)(ch = br.PeekChar())));
						return new DataScriptElement(new IntDataElement(int.Parse(value)));
						#endregion
					}
					else {
						// read var name
						string name = NextIdentifier(br);
						#region Variable
						if(name.StartsWith("$")) {
							// look up known var name
							name = name.Substring(1);
							// special exception for the $ variable
							if(name.Length == 0) {
								return new DataScriptElement(new IntDataElement(0));
							}

							foreach(var entry in Constants.knownVars) {
								if(entry.Value.Equals(name)) {
									name = entry.Key;
									break;
								}
							}

							if(name.StartsWith("flag_")) {
								int id = int.Parse(name.Substring(5));
								return new DataScriptElement(new VarIntRefDataElement("Flag", id));
							}
							if(name.StartsWith("globalflag_")) {
								int id = int.Parse(name.Substring(11));
								return new DataScriptElement(new VarIntRefDataElement("GlobalFlag", id));
							}

							if(name.StartsWith("str_")) {
								int id = int.Parse(name.Substring(4));
								return new DataScriptElement(new VarStringRefDataElement("String", id));
							}
							if(name.StartsWith("globalstr_")) {
								int id = int.Parse(name.Substring(10));
								return new DataScriptElement(new VarStringRefDataElement("GlobalString", id));
							}
						}
						#endregion
						#region Function call
						else {
							// read method call
							ch = br.ReadChar();
							if(!(ch == '(')) {

							}
							List<ScriptElement> param = new List<ScriptElement>();
							if(br.PeekChar() != ')') {
								do {
									param.Add(NextValue(br));
								}
								while(br.ReadChar() == ',');
							}
							else {
								// read the closing parenthesis
								br.ReadChar();
							}
							return new FuncCallScriptElement(name, param.ToArray());
						}
						#endregion
					}
					break;
			}
			return null;
		}

		/// <summary>
		/// Reads a string of letters, digits, '_' or '$'
		/// </summary>
		/// <param name="br"></param>
		/// <returns></returns>
		string NextIdentifier(BinaryReader br) {
			StringBuilder sb = new StringBuilder();
			int ch = br.PeekChar();
			if(ch == '_' || ch == '$' || char.IsLetter((char)ch)) {
				do {
					sb.Append((char)ch);
					br.ReadChar();
					ch = br.PeekChar();
				}
				while(ch == '_' || ch == '$' || char.IsLetter((char)ch) || char.IsDigit((char)ch));
			}
			return sb.ToString();
		}

		/// <summary>
		/// Progresses the input stream to the next non-whitespace character
		/// </summary>
		/// <param name="br">The BinaryReader used to read characters</param>
		/// 
		[DebuggerStepThrough]
		void SkipWhitespace(BinaryReader br) {
			while(char.IsWhiteSpace((char)br.PeekChar())) {
				br.ReadChar();
			}
		}

		/// <summary>
		/// Compiles an entire script into a binary script file (.yks)
		/// </summary>
		/// <param name="script">The script instance to compile</param>
		/// <param name="s">The output stream</param>
		/// <returns>The number of bytes written</returns>
		public long ToBinary(YukaScript script, Stream s) {
			long offset = s.Position;

			List<ScriptElement> flattened = new List<ScriptElement>();

			Console.ForegroundColor = ConsoleColor.Cyan;

			foreach(ScriptElement elem in script.commands) {
				List<ScriptElement> cmds = Flatten(elem, false);
				flattened.AddRange(cmds);
				if(cmds.Count == 0 && FlagCollection.current.Has('v')) {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("?????????????");
					Console.ForegroundColor = ConsoleColor.Cyan;
				}
				foreach(ScriptElement cmd in cmds) {
					if(cmd is DataScriptElement && (cmd as DataScriptElement).elem is ControlDataElement) {
						string name = ((cmd as DataScriptElement).elem as ControlDataElement).name;
						if(!"else".Equals(name) && !"{".Equals(name) && !"}".Equals(name)) {
							jumpLabels.Add(name, (cmd as DataScriptElement).elem as ControlDataElement);
						}
					}
					//Console.WriteLine(cmd);
				}
			}

			List<DataElement> code = new List<DataElement>();
			Dictionary<string, FuncDataElement> functions = new Dictionary<string, FuncDataElement>();

			foreach(ScriptElement elem in flattened) {
				if(elem is FuncCallScriptElement) {
					string name = (elem as FuncCallScriptElement).name;
					ScriptElement[] parameters = (elem as FuncCallScriptElement).parameters;

					if(!functions.ContainsKey(name)) {
						functions[name] = new FuncDataElement(name);
					}
					functions[name].lastOffset = code.Count;

					code.Add(functions[name]);
					code.Add(new RawDataElement(parameters.Length));

					foreach(ScriptElement param in parameters) {
						if(param is DataScriptElement) {
							code.Add((param as DataScriptElement).elem);
						}
						else {
							throw new Exception("Expected DataScriptElement, got " + param.GetType().Name);
						}
					}
				}
				else if(elem is DataScriptElement) {
					DataElement data = (elem as DataScriptElement).elem;

					if(data is ControlDataElement) {
						(data as ControlDataElement).codeOffset = code.Count;
					}

					code.Add(data);
				}
				else {

				}
				if(FlagCollection.current.Has('v')) {
					Console.WriteLine(elem);
				}
			}
			Console.ForegroundColor = ConsoleColor.Yellow;

			DataManager dataManager = new DataManager();
			List<Tuple<int, int, int, int>> index = new List<Tuple<int, int, int, int>>();
			List<int> codeData = new List<int>();

			foreach(DataElement elem in code) {
				if(elem is ControlDataElement) {
					string name = (elem as ControlDataElement).name;
					if(jumpLabels.ContainsKey(name)) {
						(elem as ControlDataElement).link = jumpLabels[name];
					}
				}

				if(elem is RawDataElement) {
					codeData.Add((elem as RawDataElement).value);
				}
				else {
					elem.WriteData(dataManager);
					Tuple<int, int, int, int> entry = elem.GetIndex();
					if(FlagCollection.current.Has('v')) {
						Console.WriteLine(entry);
					}
					int i = index.IndexOf(entry);
					if(i == -1) {
						i = index.Count;
						index.Add(entry);
					}
					codeData.Add(i);
				}
			}
			Console.ResetColor();

			BinaryWriter bw = new BinaryWriter(s);

			// write header
			s.Write(new byte[] { 0x59, 0x4B, 0x53, 0x30, 0x30, 0x31, 0x01, 0x00 }, 0, 8);
			s.Write(new byte[] { 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 8);

			bw.Write(0x30);
			bw.Write(codeData.Count);
			bw.Write(0x30 + codeData.Count * 4);
			bw.Write(index.Count);
			bw.Write(0x30 + codeData.Count * 4 + index.Count * 16);
			bw.Write(dataManager.offset);
			bw.Write(tempVarID);
			bw.Write(0);

			// write code sector
			foreach(int cmd in codeData) {
				bw.Write(cmd);
			}

			// write index
			foreach(Tuple<int, int, int, int> entry in index) {
				bw.Write(entry.Item1);
				bw.Write(entry.Item2);
				bw.Write(entry.Item3);
				bw.Write(entry.Item4);
			}

			// write data sector
			dataManager.WriteTo(s);
			return s.Position - offset;
		}

		/// <summary>
		/// Flattens nested script elements to a list
		/// </summary>
		/// <param name="elem">The element to flatten</param>
		/// <param name="nested">Whether this is NOT a top-level element</param>
		/// <returns>The flattened list of elements</returns>
		List<ScriptElement> Flatten(ScriptElement elem, bool nested) {
			List<ScriptElement> list = new List<ScriptElement>();
			if(elem is FuncCallScriptElement) {
				// new parameter array
				ScriptElement[] parameters = new ScriptElement[(elem as FuncCallScriptElement).parameters.Length];

				for(int i = 0; i < parameters.Length; i++) {
					ScriptElement param = (elem as FuncCallScriptElement).parameters[i];
					if(param is DataScriptElement) {
						parameters[i] = param;
					}
					else {
						// flatten all non-data parameters
						list.AddRange(Flatten(param, true));
						parameters[i] = new DataScriptElement(new VarTempRefDataElement(tempVarID - 1));
					}
				}

				if(nested) {
					list.Add(new DataScriptElement(new VarTempRefDataElement(tempVarID++)));
					list.Add(new FuncCallScriptElement("=", new ScriptElement[0]));
				}
				list.Add(new FuncCallScriptElement((elem as FuncCallScriptElement).name, parameters));
			}
			else if(elem is ExpressionScriptElement) {
				ScriptElement[] parameters = new ScriptElement[(elem as ExpressionScriptElement).parameters.Length + (elem as ExpressionScriptElement).operators.Length];

				for(int i = 0; i < (elem as ExpressionScriptElement).parameters.Length; i++) {
					if(i > 0) {
						string op = (elem as ExpressionScriptElement).operators[i - 1];
						if(op.Equals("==")) {
							op = "=";
						}
						parameters[i * 2 - 1] = new DataScriptElement(new ControlDataElement(op));
					}
					ScriptElement param = (elem as ExpressionScriptElement).parameters[i];
					if(param is DataScriptElement) {
						parameters[i * 2] = param;
					}
					else {
						// flatten all non-data parameters
						list.AddRange(Flatten(param, true));
						parameters[i * 2] = new DataScriptElement(new VarTempRefDataElement(tempVarID - 1));
					}
				}

				list.Add(new DataScriptElement(new VarTempRefDataElement(tempVarID++)));
				list.Add(new FuncCallScriptElement("=", parameters));
			}
			else if(elem is BranchScriptElement) {
				ScriptElement cond = (elem as BranchScriptElement).condition;

				if(!(cond is DataScriptElement)) {
					list.AddRange(Flatten(cond, true));
					cond = new DataScriptElement(new VarTempRefDataElement(tempVarID - 1));
				}

				ControlDataElement openingThen = new ControlDataElement("{"), closingThen = new ControlDataElement("}"), openingElse = null, closingElse = null;
				if((elem as BranchScriptElement).falsebody != null) {
					openingElse = new ControlDataElement("{");
					closingElse = new ControlDataElement("}");
					openingThen.link = openingElse;
					closingThen.link = openingThen;
					openingElse.link = closingElse;
					closingElse.link = openingElse;
				}
				else {
					openingThen.link = closingThen;
					closingThen.link = openingThen;
				}

				list.Add(new FuncCallScriptElement("if", new ScriptElement[] { cond }));

				list.Add(new DataScriptElement(openingThen));
				foreach(ScriptElement cmd in (elem as BranchScriptElement).truebody) {
					list.AddRange(Flatten(cmd, false));
				}
				list.Add(new DataScriptElement(closingThen));

				if((elem as BranchScriptElement).falsebody != null) {
					list.Add(new DataScriptElement(new ControlDataElement("else")));
					list.Add(new DataScriptElement(openingElse));
					foreach(ScriptElement cmd in (elem as BranchScriptElement).falsebody) {
						list.AddRange(Flatten(cmd, false));
					}
					list.Add(new DataScriptElement(closingElse));
				}
			}
			else if(elem is SwitchFunctionScriptElement) {
				// new parameter array
				ScriptElement[] parameters = new ScriptElement[(elem as SwitchFunctionScriptElement).parameters.Length];

				for(int i = 0; i < parameters.Length; i++) {
					ScriptElement param = (elem as SwitchFunctionScriptElement).parameters[i];
					if(param is DataScriptElement) {
						parameters[i] = param;
					}
					else {
						// flatten all non-data parameters
						list.AddRange(Flatten(param, true));
						parameters[i] = new DataScriptElement(new VarTempRefDataElement(tempVarID - 1));
					}
				}

				list.Add(new FuncCallScriptElement((elem as SwitchFunctionScriptElement).name, parameters));
				ControlDataElement opening = new ControlDataElement("{");
				ControlDataElement closing = new ControlDataElement("}");
				opening.link = closing;
				closing.link = opening;

				list.Add(new DataScriptElement(opening));
				foreach(ScriptElement cmd in (elem as SwitchFunctionScriptElement).body) {
					list.AddRange(Flatten(cmd, false));
				}
				list.Add(new DataScriptElement(closing));
			}
			else if(elem is AssignmentScriptElement) {
				ScriptElement expr = (elem as AssignmentScriptElement).expression;
				DataElement var = (elem as AssignmentScriptElement).var;

				if(expr is DataScriptElement) {
					// special exception for the $ variable (e.g. clear_main.yks)
					if(var is IntDataElement) {
						list.Add(new DataScriptElement(new VarTempRefDataElement(tempVarID++)));
						list.Add(new FuncCallScriptElement("=", new[] { expr }));
						list.Add(new DataScriptElement(var));
						list.Add(new FuncCallScriptElement("=", new[] { new DataScriptElement(new VarTempRefDataElement(tempVarID - 1)) }));
					}
					else {
						list.Add(new DataScriptElement(var));
						list.Add(new FuncCallScriptElement("=", new[] { expr }));
					}
				}
				else {
					List<ScriptElement> cmds = Flatten(expr, true);
					// special exception for the $ variable (e.g. clear_main.yks)
					if(var is IntDataElement) {
						list.AddRange(cmds);
						list.Add(new DataScriptElement(var));
						list.Add(new FuncCallScriptElement("=", new[] { new DataScriptElement(new VarTempRefDataElement(tempVarID - 1)) }));
					}
					else {
						// replace the last temp var reference by the variable
						for(int i = cmds.Count - 1; i >= 0; i--) {
							if(cmds[i] is DataScriptElement && (cmds[i] as DataScriptElement).elem is VarTempRefDataElement) {
								cmds[i] = new DataScriptElement((elem as AssignmentScriptElement).var);
								list.AddRange(cmds);
								return list;
							}
						}
						throw new Exception("No VarTempRefDataElement found");
					}
				}
			}
			else if(elem is JumpLabelScriptElement) {
				list.Add(new DataScriptElement(new ControlDataElement((elem as JumpLabelScriptElement).name)));
			}
			return list;
		}
	}
}
