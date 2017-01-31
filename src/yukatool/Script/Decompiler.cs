using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Yuka.Data;

namespace Yuka.Script {
	class Decompiler {
		int codeoffset, codecount, indexoffset, indexcount, dataoffset, datalength;
		Dictionary<string, string> stringTable = new Dictionary<string, string>();
		List<ScriptElement> commands = new List<ScriptElement>();
		Dictionary<int, ScriptElement> substituteExpressions = new Dictionary<int, ScriptElement>();

		int nameCount, lineCount, stringCount;
		string speaker = "";

		/**
		 * Decompiles a binary script from a stream.
		 */
		public YukaScript FromBinary(Stream s) {
			BinaryReader br = new BinaryReader(s);

			// read header
			s.Seek(0x10, SeekOrigin.Begin);
			codeoffset = br.ReadInt32();
			codecount = br.ReadInt32();
			indexoffset = br.ReadInt32();
			indexcount = br.ReadInt32();
			dataoffset = br.ReadInt32();
			datalength = br.ReadInt32();

			// seek to the start of the code block
			s.Seek(codeoffset, SeekOrigin.Begin);

			// as long as we have not reached the end of the code block
			while(s.Position < codeoffset + codecount * 4) {
				// try to read the next script element.
				ScriptElement elem = NextScriptElement(s);

				// if you found one, then...
				if(elem != null) {
					// ...add it to the list of commands.
					commands.Add(elem);

					// some logging logic.
					if(FlagCollection.current.Has('v')) {
						string output = elem.ToString();
						if(output.StartsWith("yuka.Script")) {
							Console.ForegroundColor = ConsoleColor.Cyan;
						}
						Console.WriteLine(output);
						Console.ResetColor();
					}
				}
				else {

				}
			}

			// create new script instance from list of commands and attached string data and return it
			return new YukaScript(commands, stringTable);
		}

		/**
		 * Reads the next script element from the stream.
		 * This could be an entire command or a single parameter.
		 */
		ScriptElement NextScriptElement(Stream s) {
			BinaryReader br = new BinaryReader(s);
			ScriptElement elem = null;

			// read the next data element.
			DataElement cmd = NextDataElement(s);

			// if no data element could be found, ...
			if(cmd == null) {
				// ...return nothing.
				return null;
			}

			// if the element is...
			switch(cmd.type) {
				// ...a function call, then...
				case DataElement.TYPE_FUNC: {
						// ...determine how many parameters the function call takes.
						int paramCount = br.ReadInt32();
						ScriptElement[] parameters = new ScriptElement[paramCount];

						// for each parameter...
						for(var j = 0; j < paramCount; j++) {
							// ...read it's value
							DataElement param = NextDataElement(s);

							// if the parameter is a temporary variable reference, then...
							if(param is VarTempRefDataElement) {
								// ...replace it by the appropriate substitution expression.
								parameters[j] = substituteExpressions[((VarTempRefDataElement)param).id];
								// after that, continue with the next parameter.
								continue;
							}

							// if the function is called... 
							// ..."StrOut" and this parameter is a string, ...
							if((((FuncDataElement)cmd).name.Equals("StrOut") || ((FuncDataElement)cmd).name.Equals("StrOutNW")) && param is StringDataElement) {
								// (...then this is a line of text to be displayed.)
								// (we want to store those in a separate file to make it easier to translate them.)

								// get the text
								string line = ((StringDataElement)param).data;
								// generate a new line number
								string id = "L" + (++lineCount);
								// add those to the list of known strings...
								stringTable.Add(id, speaker + '|' + line);
								// ...and replace the string parameter with reference to the line number
								parameters[j] = new DataScriptElement(new ExternalStringDataElement(id, stringTable));
							}
							// ..."StrOutNWC", ...
							else if(((FuncDataElement)cmd).name.Equals("StrOutNWC")) {
								// (...then this parameter represents a name.)
								// if the parameter is...
								// ...a string, then...
								if(param is StringDataElement) {
									// (...we want to store it separately to make it easier to translate.)
									string name = ((StringDataElement)param).data;
									// set the current speaker to this name.
									speaker = name;
									string id = null;
									// for each entry on our list of known strings... 
									foreach(var entry in stringTable) {
										// ...check if it is a name.
										if(entry.Key[0] == 'N' && entry.Value.Equals(name)) {
											// if it is, remember it's ID...
											id = entry.Key;
											// ...and stop searching.
											break;
										}
									}
									// if you didn't find a matching ID, ...
									if(id == null) {
										// generate a new one...
										id = "N" + (++nameCount);
										// ...and add it - together with the name - to the list of known strings.
										stringTable.Add(id, name);
									}
									// finally, replace the parameter by a reference to the name ID.
									parameters[j] = new DataScriptElement(new ExternalStringDataElement(id, stringTable));
								}
								// ...a variable string reference, then...
								else if(param is VarStringRefDataElement) {
									// ...set the current speaker to "me"...
									speaker = "me";
									// ...and keep the parameter as it is.
									parameters[j] = new DataScriptElement(param);
								}
								// ...something else, ...
								else {
									// ...then we just keep it as it is.
									parameters[j] = new DataScriptElement(param);
								}
							}
							// ...none of the above, ...
							else {
								// ...then we just keep the parameter as it is.
								parameters[j] = new DataScriptElement(param);
							}
						}

						// if the function is called "PF" (PrintFlush?), then...
						if(((FuncDataElement)cmd).name.Equals("PF")) {
							// ...reset the current speaker.
							speaker = "";
						}

						// save the current position in the code stream.
						long pos = s.Position;
						// read the next data element
						DataElement next = NextDataElement(s);
						// check if the next data element is a opening control flow element.
						// if it is, then...
						if(next is ControlDataElement && ((ControlDataElement)next).name.Equals("{")) {
							// ...create an empty list of script elements.
							List<ScriptElement> body = new List<ScriptElement>();

							ScriptElement cur;
							// (read script elements and add them to the list of commands until you encounter a closing flow control element.)
							// repeat:
							while(true) {
								// read the next script element.
								cur = NextScriptElement(s);
								// if it is a closing flow control element, ...
								if(cur is JumpLabelScriptElement && ((JumpLabelScriptElement)cur).name.Equals("}")) {
									// ...stop repeating immediately
									break;
								}
								// otherwise, add it to the list of commands.
								body.Add(cur);
							}

							// if the function is a branching operation, then...
							if(((FuncDataElement)cmd).name.Equals("if")) {
								// ...save the current position in the code stream.
								pos = s.Position;
								// read the next data element
								next = NextDataElement(s);
								// check if the next data element is a else clause.
								// if it is, then...
								if(next is ControlDataElement && ((ControlDataElement)next).name.Equals("else")) {
									// ...discard the opening flow control element.
									NextDataElement(s);
									// ...create an empty list of script elements.
									List<ScriptElement> falsebody = new List<ScriptElement>();

									// (read script elements and add them to the list of commands until you encounter a closing flow control element.)
									// repeat:
									while(true) {
										// read the next script element.
										cur = NextScriptElement(s);
										// if it is a closing flow control element, ...
										if(cur is JumpLabelScriptElement && ((JumpLabelScriptElement)cur).name.Equals("}")) {
											// ...stop repeating immediately
											break;
										}
										// otherwise, add it to the list of commands.
										falsebody.Add(cur);
									}
									// use the first (and only) parameter of the function call as branching condition, wrap it - together with the code blocks - in a script element and save it as the result.
									elem = new BranchScriptElement(parameters[0], body, falsebody);
								}
								// it it's not, then...
								else {
									// ...jump back to the previously saved code position.
									s.Seek(pos, SeekOrigin.Begin);
									// use the first (and only) parameter of the function call as branching condition, wrap it - together with the code block - in a script element and save it as the result.
									elem = new BranchScriptElement(parameters[0], body, null);
								}
							}
							// if it it not, then
							else {
								elem = new SwitchFunctionScriptElement(((FuncDataElement)cmd).name, parameters, body);
							}
						}
						// if not, ...
						else {
							// ...jump back to the previously saved code position.
							s.Seek(pos, SeekOrigin.Begin);
							// wrap the method name and and list of parameters in a script element and save it as the result.
							elem = new FuncCallScriptElement(((FuncDataElement)cmd).name, parameters);
						}
						break;
					}
				// ...a flow control element, then...
				case DataElement.TYPE_CTRL: {
						// ...create a new jump label with the same name and save it as the result.
						elem = new JumpLabelScriptElement(((ControlDataElement)cmd).name);
						break;
					}
				// ...a variable int or string reference, then...
				case DataElement.TYPE_VINT:
				case DataElement.TYPE_VSTR: {
						// (...we expect an assignment operation to follow.)
						// read assigned expression.
						ScriptElement expression = NextAssignment(s);
						// wrap the variable int/string reference and the expression in a new script element and save it as the result.
						elem = new AssignmentScriptElement(cmd, expression);
						break;
					}
				// ...a temporary variable id, then...
				case DataElement.TYPE_VTMP: {
						// (...we expect an assignment operation to follow.)
						// (after that we expect a command which uses this temporary variable.)
						// read assigned expression.
						ScriptElement expression = NextAssignment(s);
						// set the current temp variable expression to the one we just read...
						substituteExpressions[((VarTempRefDataElement)cmd).id] = expression;
						// ...and return nothing this time.
						break;
					}
				// ...an integer constant, then...
				case DataElement.TYPE_CINT: {
						// (...we have reached a very confusing part of this binary script format.)
						// (we expect an assignment to follow, basically "0 = <something>".)
						// (this sets the current flag pointer (represented by the 0) to <something>.)
						// (the flag pointed to by the pointer can be a accessed by using flag_$, globalflag_$, str_$ or globalstr_$.)
						// read the next script element (the expected assignment).
						FuncCallScriptElement assignment = NextScriptElement(s) as FuncCallScriptElement;
						ScriptElement expression = null;

						// if the assignment has...
						// ...more than 1 element, ...
						if(assignment.parameters.Length > 1) {
							// ...then create an expression from those.
							expression = new ExpressionScriptElement(assignment.parameters);
						}
						// ...exactly 1 element, ...
						else if(assignment.parameters.Length == 1) {
							// ...then it already is a valid expression.
							expression = assignment.parameters[0];
						}
						// ...no elements, ...
						else {
							// ...then read the next script element and use it as expression.
							expression = NextScriptElement(s);
						}

						// wrap the integer and the expression in a new script element and save it as the result.
						elem = new AssignmentScriptElement(cmd, expression);
						break;
					}
				// ...something else, ...
				default: {
						// the script is probably damaged, so we throw an error.
						throw new Exception("Script format invalid");
					}
			}

			// return the saved result
			return elem;
		}

		ScriptElement NextAssignment(Stream s) {
			// read the next script element (the expected assignment).
			FuncCallScriptElement assignment = NextScriptElement(s) as FuncCallScriptElement;
			ScriptElement[] parameters = new ScriptElement[assignment.parameters.Length];

			// go through all parameters of the assignment.
			for(int i = 0; i < assignment.parameters.Length; i++) {
				ScriptElement param = assignment.parameters[i];
				// if the parameter is...
				// ...a string constant, then...
				if(param is DataScriptElement && ((DataScriptElement)param).elem is StringDataElement) {
					// ...retrieve it's value.
					string str = ((StringDataElement)((DataScriptElement)param).elem).data;
					// if it contains only whitespace, if it is a number, or if it is part of a file name, then...
					if(str.Trim().Length == 0 || str.All(char.IsDigit) || str.Contains("\\") || str.ToLower().Contains(".yk") || str.ToLower().Contains(".ogg")) {
						// ...just use it in the expression.
						parameters[i] = param;
					}
					// otherwise...
					else {
						string id = null;
						// ... go through all entries on our list of known strings.
						/*foreach(var entry in stringTable) {
							// check if it is a name.
							if(entry.Key[0] == 'S' && entry.Value.Equals(str)) {
								// if it is, remember it's ID...
								id = entry.Key;
								// ...and stop searching.
								break;
							}
						}*/
						// if you didn't find a matching ID, then...
						//if(id == null) { // (keep duplicates, they may have different translations)
						// ...generate a new one...
						id = "S" + (++stringCount);
						// ...and add it - together with the name - to the list of known strings.
						stringTable.Add(id, str);
						//}
						// then use an external string reference as the new expression.
						parameters[i] = new DataScriptElement(new ExternalStringDataElement(id, stringTable));
					}
				}
				// ...a temporary variable reference, then...
				else if(param is DataScriptElement && ((DataScriptElement)param).elem is VarTempRefDataElement) {
					// ...replace it by the current substitution expression.
					parameters[i] = substituteExpressions[((VarTempRefDataElement)((DataScriptElement)param).elem).id];
				}
				// ...something else, then...
				else {
					// ...just use it in the expression.
					parameters[i] = param;
				}
			}

			ScriptElement expression = null;

			// if the assignment has...
			// ...more than 1 elements, then...
			if(parameters.Length > 1) {
				// ...create an expression from those.
				expression = new ExpressionScriptElement(parameters);
			}
			// ...exactly 1 element, then...
			else if(assignment.parameters.Length == 1) {
				expression = parameters[0];
			}
			// ...no elements, ...
			else {
				// ...then read the next script element and use it as expression.
				expression = NextScriptElement(s);
			}
			return expression;
		}

		/**
		 * Reads the next data element from code, looks it up in the index and fetches the corresponding data
		 */
		DataElement NextDataElement(Stream s) {
			// if we are beyond the code sector, ...
			if(s.Position >= codeoffset + codecount * 4) {
				// ...return nothing.
				return null;
			}

			BinaryReader br = new BinaryReader(s);
			// read data id from the code stream
			int index = br.ReadInt32();

			// save the current position in code
			long pos = s.Position;
			// seek to the position of that id in the index
			s.Seek(indexoffset + index * 16, SeekOrigin.Begin);
			// read the data type and 3 data values
			int type = br.ReadInt32();
			int data1 = br.ReadInt32();
			int data2 = br.ReadInt32();
			int data3 = br.ReadInt32();
			// jump back to the previous code position
			s.Seek(pos, SeekOrigin.Begin);

			// if the data type is...
			switch(type) {
				// ...a function call, then...
				case DataElement.TYPE_FUNC: {
						// (...ignore data2 and data3, ...)
						// ...read the string at the offset within the data sector denoted by the first meta data value.
						string name = ReadStringAt(data1, s);
						// create a new function data element with this string as it's name and return it.
						return new FuncDataElement(name);
					}
				// ...a flow control element, then...
				case DataElement.TYPE_CTRL: {
						// (...ignore data3, ...)
						// ...read the string at the offset within the data sector denoted by the first meta data value.
						string name = ReadStringAt(data1, s);
						// ...read the integer at the offset within the data sector denoted by the second meta data value.
						int offset = ReadIntAt(data2, s);
						// create a new flow control data element with this string as it's name and the integer as jump offset and return it.
						return new ControlDataElement(name);
					}
				// ...a integer constant, then...
				case DataElement.TYPE_CINT: {
						// (...ignore data1 and data3, ...)
						// ...read the integer at the offset within the data sector denoted by the second meta data value.
						int value = ReadIntAt(data2, s);
						// create a new int constant data element with this integer as it's value and return it.
						return new IntDataElement(value);
					}
				// ...a string constant, then...
				case DataElement.TYPE_CSTR: {
						// (...ignore data1 and data3, ...)
						// ...read the string at the offset within the data sector denoted by the second meta data value.
						string value = ReadStringAt(data2, s);
						// create a new string constant data element with this integer as it's value and return it.
						return new StringDataElement(value);
					}
				// ...a integer variable reference, then...
				case DataElement.TYPE_VINT: {
						// (...ignore data2, ...)
						// ...read the string at the offset within the data sector denoted by the first meta data value.
						string reftype = ReadStringAt(data1, s);
						// ...read the integer at the offset within the data sector denoted by the second meta data value.
						int refid = ReadIntAt(data3, s);
						// create a new integer variable reference data element with the string as it's reference type and the integer as it's reference id and return it.
						return new VarIntRefDataElement(reftype, refid);
					}
				// ...a string variable reference, then...
				case DataElement.TYPE_VSTR: {
						// (...ignore data2, ...)
						// ...read the string at the offset within the data sector denoted by the first meta data value.
						string reftype = ReadStringAt(data1, s);
						// ...read the integer at the offset within the data sector denoted by the second meta data value.
						int refid = ReadIntAt(data3, s);
						// create a new integer variable reference data element with the string as it's reference type and the integer as it's reference id and return it.
						return new VarStringRefDataElement(reftype, refid);
					}
				// ...a temp variable reference, then...
				case DataElement.TYPE_VTMP: {
						// (...ignore data1 and data3, ...)
						// create a new temprary variable reference data element with the second meta data value as it's id and return it.
						return new VarTempRefDataElement(data2);
					}
			}

			// return no data element (nothing found).
			return null;
		}

		/**
		 * Reads an "encrypted" integer at the specified offset within the data sector.
		 */
		int ReadIntAt(int offset, Stream s) {
			// save the current position in the stream.
			long pos = s.Position;
			// jump to the specified offset relative to the start of the data sector.
			s.Seek(dataoffset + offset, SeekOrigin.Begin);

			// read a 32 bit signed integer and flip every second bit (this is the basic encryption used by this script format).
			int data = (int)(new BinaryReader(s).ReadInt32() ^ 0xAAAAAAAA);

			// jump to the previously saved position in the stream.
			s.Seek(pos, SeekOrigin.Begin);
			// return the read integer.
			return data;
		}

		/**
		 * Reads an "encrypted" string of characters at the specified offset within the data sector.
		 */
		string ReadStringAt(int offset, Stream s) {
			// save the current position in the stream.
			long pos = s.Position;
			// jump to the specified offset relative to the start of the data sector.
			s.Seek(dataoffset + offset, SeekOrigin.Begin);

			// create an empty list of bytes.
			List<byte> data = new List<byte>();
			int buf;
			// as long as the next byte is not 0xAA...
			while((buf = s.ReadByte()) != 0xAA) {
				// ...flip every second bit and add it to the list of bytes.
				data.Add((byte)(buf ^ 0xAA));
			}
			// jump to the previously saved position in the stream.
			s.Seek(pos, SeekOrigin.Begin);
			// interpret the list of bytes as a Shift-JIS encoded string and return it.
			return Encoding.GetEncoding("shift-jis").GetString(data.ToArray());
		}

		public void ToSource(YukaScript script, string path) {
			string sourcePath = Path.ChangeExtension(path, Constants.ykd);
			string stringPath = Path.ChangeExtension(path, Constants.csv);

			// write source code
			File.WriteAllText(sourcePath, script.Source());

			// write string data
			if(script.stringTable.Count > 0) {
				StreamWriter w = new StreamWriter(new FileStream(stringPath, FileMode.Create));
				w.WriteLine("ID,Speaker,Original,Translation,TLC,Edit,QC,Comments,Generated");
				// write names
				bool flag = false;
				foreach(var entry in script.stringTable) {
					if(entry.Key.StartsWith("N")) {
						if(!flag) {
							w.WriteLine("\n#Names:");
							flag = true;
						}
						w.WriteLine(entry.Key + ",," + entry.Value);
					}
				}
				// write lines
				flag = false;
				foreach(var entry in script.stringTable) {
					if(!entry.Key.StartsWith("N")) {
						if(!flag) {
							w.WriteLine("\n#Lines:");
							flag = true;
						}

						string speaker = "";
						string line = entry.Value;

						int index = line.IndexOf('|');
						if(index >= 0) {
							speaker = line.Substring(0, index);
							line = line.Substring(index + 1);
						}

						line = "\"" + line.Replace("\"", "\"\"") + "\"";

						w.WriteLine(entry.Key + ',' + speaker + ',' + line);
					}
				}
				w.Close();
			}
		}
	}
}
