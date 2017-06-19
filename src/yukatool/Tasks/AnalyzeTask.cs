using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yuka.Tasks {
	class AnalyzeTask : Task {
		public override Task NewTask() {
			return new AnalyzeTask();
		}

		public override void DefaultFlags(FlagCollection flags) {
			flags.Add('v', "verbose", "Outputs additional information", false);
			flags.Add('w', "wait", "Waits for enter before closing the console", false);
		}

		protected override void Execute() {
			if(arguments.Length == 0) Fail("No source file specified");

			string sourceBasePath = Helpers.AbsolutePath(arguments[0]);
			string targetBasePath = arguments.Length > 1 ? Helpers.AbsolutePath(arguments[1]) : Path.ChangeExtension(sourceBasePath, Constants.html);

			string[] files = null;
			if(Directory.Exists(sourceBasePath)) {
				files = Directory.GetFileSystemEntries(sourceBasePath, "*", SearchOption.AllDirectories);

				// TODO analyze directory
			}
			else if(File.Exists(sourceBasePath)) {
				string ext = Path.GetExtension(sourceBasePath).ToLower();

				// Binary script
				if(ext.Equals('.' + Constants.yks)) {
					FileStream fs = new FileStream(sourceBasePath, FileMode.Open);
					BinaryReader r = new BinaryReader(fs);

					// read header
					var report = new AnalyzerReport.ScriptBinary() {
						Magic = r.ReadBytes(6),
						Version = r.ReadInt16(),
						HeaderSize = r.ReadInt32(),
						Unknown1 = r.ReadInt32(),
						CodeOffset = r.ReadInt32(),
						CodeCount = r.ReadInt32(),
						IndexOffset = r.ReadInt32(),
						IndexCount = r.ReadInt32(),
						DataOffset = r.ReadInt32(),
						DataLength = r.ReadInt32(),
						VarPoolSize = r.ReadInt32(),
						Unknown2 = r.ReadInt32()
					};

					// read data
					var data = new byte[report.DataLength];
					fs.Seek(report.DataOffset, SeekOrigin.Begin);
					for(int i = 0; i < report.DataLength; i++) {
						data[i] = (byte)(r.ReadByte() ^ 0xAA);
					}

					// read index
					fs.Seek(report.IndexOffset, SeekOrigin.Begin);
					report.Index = new AnalyzerReport.ScriptBinary.IndexEntry[report.IndexCount];
					for(int i = 0; i < report.IndexCount; i++) {
						var entry = new AnalyzerReport.ScriptBinary.IndexEntry(i, r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
						report.Index[i] = entry;
					}

					// read code
					var code = new List<AnalyzerReport.ScriptBinary.CodeEntry>();
					int maxArgs = 0;
					fs.Seek(report.CodeOffset, SeekOrigin.Begin);
					for(int i = 0; i < report.CodeCount; i++) {
						int instrID = r.ReadInt32();
						var instruction = (instrID >= 0 && instrID < report.Index.Length) ? report.Index[instrID] : new AnalyzerReport.ScriptBinary.IndexEntry(instrID, -1, -1, -1, -1);
						if(instruction.Type == AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_FUNC) {
							int argCount = r.ReadInt32();
							if(argCount > maxArgs) maxArgs = argCount;
							i += argCount + 1;
							var arguments = new AnalyzerReport.ScriptBinary.IndexEntry[argCount];
							for(int j = 0; j < argCount; j++) {
								int argID = r.ReadInt32();
								arguments[j] = (argID >= 0 && argID < report.Index.Length) ? report.Index[argID] : new AnalyzerReport.ScriptBinary.IndexEntry(argID, -1, -1, -1, -1);
							}
							code.Add(new AnalyzerReport.ScriptBinary.CodeEntry(i, instruction, arguments));
						}
						else {
							code.Add(new AnalyzerReport.ScriptBinary.CodeEntry(i, instruction));
						}
					}
					report.Code = code.ToArray();





					var w = StartOutput(targetBasePath);
					{
						StartSection("Meta Information", w);
						{
							Echo("File Name:			" + Path.GetFileName(sourceBasePath) + '\n', w);
							Echo("File Type:			Compiled Yuka Script (.yks)\n", w);
							Echo("File Size:			" + new FileInfo(sourceBasePath).Length + " bytes\n", w);
						}
						FinishSection(w);

						StartSection("File Header", w);
						{
							Echo("Magic:				", w);
							Echo(BitConverter.ToString(report.Magic).Replace('-', ' '), " ", "const const-str", w);
							Echo("# ", "comment", w);
							Echo('"' + Encoding.ASCII.GetString(report.Magic) + '"', "\n", "const const-str", w);

							Echo("Version:			", w);
							Echo(report.Version.ToString("X4"), "              ", "const const-int", w);
							Echo("# ", "comment", w);
							Echo(report.Version.ToString(), "\n", "const const-int", w);

							Echo("HeaderSize:			", w);
							Echo(report.HeaderSize.ToString("X8"), "          ", "const const-int", w);
							Echo("# ", "comment", w);
							Echo(report.HeaderSize.ToString(), "\n", "const const-int", w);

							Echo("?:					", w);
							Echo(report.Unknown1.ToString("X8"), "          ", "const const-int", w);
							Echo("# ", "comment", w);
							Echo(report.Unknown1.ToString(), "\n", "const const-int", w);

							Echo("Code Offset:		", w);
							Echo(report.CodeOffset.ToString("X8"), "          ", "const const-int", w);
							Echo("# ", "comment", w);
							Echo(report.CodeOffset.ToString(), "\n", "const const-int", w);

							Echo("Instruction Count:	", w);
							Echo(report.CodeCount.ToString("X8"), "          ", "const const-int", w);
							Echo("# ", "comment", w);
							Echo(report.CodeCount.ToString(), "\n", "const const-int", w);

							Echo("Index Offset:		", w);
							Echo(report.IndexOffset.ToString("X8"), "          ", "const const-int", w);
							Echo("# ", "comment", w);
							Echo(report.IndexOffset.ToString(), "\n", "const const-int", w);

							Echo("Entry Count:		", w);
							Echo(report.IndexCount.ToString("X8"), "          ", "const const-int", w);
							Echo("# ", "comment", w);
							Echo(report.IndexCount.ToString(), "\n", "const const-int", w);

							Echo("Data Offset:		", w);
							Echo(report.DataOffset.ToString("X8"), "          ", "const const-int", w);
							Echo("# ", "comment", w);
							Echo(report.DataOffset.ToString(), "\n", "const const-int", w);

							Echo("Data Lentgh:		", w);
							Echo(report.DataLength.ToString("X8"), "          ", "const const-int", w);
							Echo("# ", "comment", w);
							Echo(report.DataLength.ToString(), "\n", "const const-int", w);

							Echo("Ver Pool Size:		", w);
							Echo(report.VarPoolSize.ToString("X8"), "          ", "const const-int", w);
							Echo("# ", "comment", w);
							Echo(report.VarPoolSize.ToString(), "\n", "const const-int", w);

							Echo("?:					", w);
							Echo(report.Unknown2.ToString("X8"), "          ", "const const-int", w);
							Echo("# ", "comment", w);
							Echo(report.Unknown2.ToString(), "\n", "const const-int", w);
						}
						FinishSection(w);

						StartSection("Code", w);
						{
							Echo(maxArgs == 0
								? "          Function" : maxArgs == 1
								? "          Function ArgCount Argument"
								: "          Function ArgCount |---Arguments---" + new string('-', maxArgs * 9 - 18) + "|\n", w);
							foreach(var instr in report.Code) {
								Echo(instr.Address.ToString("X8"), ": ", "address", w);
								switch(instr.Instruction.Type) {
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_FUNC:
										Echo(instr.Instruction.ID.ToString("X8"), " ", "ref ref-fnc", w);
										Echo(instr.Arguments.Length.ToString("X8"), " ", "const const-int", w);
										foreach(var arg in instr.Arguments) {
											switch(arg.Type) {
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_FUNC:
													Echo(arg.ID.ToString("X8"), " ", "ref ref-fnc", w);
													break;
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CTRL:
													Echo(arg.ID.ToString("X8"), " ", "ref ref-lbl", w);
													break;
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CINT:
													Echo(arg.ID.ToString("X8"), " ", "ref ref-int", w);
													break;
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CSTR:
													Echo(arg.ID.ToString("X8"), " ", "ref ref-str", w);
													break;
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VINT:
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VSTR:
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VTMP:
													Echo(arg.ID.ToString("X8"), " ", "ref ref-var", w);
													break;
												default:
													Echo(arg.ID.ToString("X8"), " ", "ref", w);
													break;
											}
										}
										Echo(new string(' ', (maxArgs - instr.Arguments.Length) * 9), "", w);
										Echo("# ", "comment", w);
										Echo(Helpers.ToZeroTerminatedString(data, instr.Instruction.Field1), "src src-fnc", w);
										Echo("(", "comment", w);
										bool flag = false;
										foreach(var arg in instr.Arguments) {
											if(flag) {
												Echo(", ", "comment", w);
											}
											switch(arg.Type) {
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_FUNC:
													Echo("&" + Helpers.ToZeroTerminatedString(data, arg.Field1), "src src-fnc", w);
													break;
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CTRL:
													Echo(":" + Helpers.ToZeroTerminatedString(data, arg.Field1), "src src-lbl", w);
													break;
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CINT:
													Echo(arg.ID.ToString(), "src src-int", w);
													break;
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CSTR:
													Echo('"' + Helpers.ToZeroTerminatedString(data, arg.Field2) + '"', "src src-str", w);
													break;
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VINT:
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VSTR:
													Echo(Helpers.ToZeroTerminatedString(data, arg.Field1) + ':' + BitConverter.ToInt32(data, arg.Field3), "src src-var", w);
													break;
												case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VTMP:
													Echo('$' + arg.Field2.ToString(), "src src-var", w);
													break;
												default:
													Echo(arg.ID.ToString(), "src", w);
													break;
											}
											flag = true;
										}
										Echo(")", "\n", "comment", w);
										break;
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CTRL:
										Echo(instr.Instruction.ID.ToString("X8"), " ", "ref ref-lbl", w);
										Echo(new string(' ', (maxArgs + 1) * 9), w);
										Echo("# ", "comment", w);
										Echo(":" + Helpers.ToZeroTerminatedString(data, instr.Instruction.Field1), "\n", "src src-lbl", w);
										break;
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CINT:
										Echo(instr.Instruction.ID.ToString("X8"), " ", "ref ref-int", w);
										Echo(new string(' ', (maxArgs + 1) * 9), w);
										Echo("# ", "comment", w);
										Echo(instr.Instruction.ID.ToString(), "\n", "src src-int", w);
										break;
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CSTR:
										Echo(instr.Instruction.ID.ToString("X8"), " ", "ref ref-str", w);
										Echo(new string(' ', (maxArgs + 1) * 9), w);
										Echo("# ", "comment", w);
										Echo('"' + Helpers.ToZeroTerminatedString(data, instr.Instruction.Field2) + '"', "\n", "src src-str", w);
										break;
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VINT:
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VSTR:
										Echo(instr.Instruction.ID.ToString("X8"), " ", "ref ref-var", w);
										Echo(new string(' ', (maxArgs + 1) * 9), w);
										Echo("# ", "comment", w);
										Echo(Helpers.ToZeroTerminatedString(data, instr.Instruction.Field1) + ':' + BitConverter.ToInt32(data, instr.Instruction.Field3), "\n", "src src-var", w);
										break;
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VTMP:
										Echo(instr.Instruction.ID.ToString("X8"), " ", "ref ref-var", w);
										Echo(new string(' ', (maxArgs + 1) * 9), w);
										Echo("# ", "comment", w);
										Echo('$' + instr.Instruction.Field2.ToString(), "\n", "src src-var", w);
										break;
									default:
										Echo(instr.Instruction.ID.ToString("X8"), " ", "ref", w);
										Echo(new string(' ', (maxArgs + 1) * 9), w);
										Echo("# ", "comment", w);
										Echo(instr.Instruction.ID.ToString(), "\n", "src", w);
										break;
								}
							}
						}
						FinishSection(w);

						StartSection("Index", w);
						{
							foreach(var entry in report.Index) {
								switch(entry.Type) {
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_FUNC:
										Echo(entry.ID.ToString("X8"), ": ", "address", w);
										Echo("YKS_FUNC", " ", "type type-func", w);
										Echo(entry.Field1.ToString("X8"), " ", "ref ref-str", w);
										Echo(entry.Field2.ToString("X8"), " ", "const const-int", w);
										Echo(entry.Field3.ToString("X8"), " ", "unused", w);
										Echo("# function ", "comment", w);
										Echo("\"" + Helpers.ToZeroTerminatedString(data, entry.Field1) + "\"", "const const-str", w);
										Echo(", last used at ", "comment", w);
										Echo(entry.Field2.ToString("X8"), "\n", "const, const-int", w);
										break;
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CTRL:
										Echo(entry.ID.ToString("X8"), ": ", "address", w);
										Echo("YKS_CTRL", " ", "type type-ctrl", w);
										Echo(entry.Field1.ToString("X8"), " ", "ref ref-str", w);
										Echo(entry.Field2.ToString("X8"), " ", "ref ref-int", w);
										Echo(entry.Field3.ToString("X8"), " ", "unused", w);
										Echo("# jump label ", "comment", w);
										Echo("\"" + Helpers.ToZeroTerminatedString(data, entry.Field1) + "\"", "const const-str", w);
										Echo(", linked to ", "comment", w);
										Echo(BitConverter.ToInt32(data, entry.Field2).ToString("X8"), "\n", "const, const-int", w);
										break;
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CINT:
										Echo(entry.ID.ToString("X8"), ": ", "address", w);
										Echo("YKS_CINT", " ", "type type-cint", w);
										Echo(entry.Field1.ToString("X8"), " ", "unused", w);
										Echo(entry.Field2.ToString("X8"), " ", "ref ref-int", w);
										Echo(entry.Field3.ToString("X8"), " ", "unused", w);
										Echo("# integer constant: ", "comment", w);
										Echo(BitConverter.ToInt32(data, entry.Field2).ToString(), "const const-int", w);
										Echo(" (", "comment", w);
										Echo("0x" + BitConverter.ToInt32(data, entry.Field2).ToString("X"), "const const-int", w);
										Echo(")", "\n", "comment", w);
										break;
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CSTR:
										Echo(entry.ID.ToString("X8"), ": ", "address", w);
										Echo("YKS_CSTR", " ", "type type-cstr", w);
										Echo(entry.Field1.ToString("X8"), " ", "unused", w);
										Echo(entry.Field2.ToString("X8"), " ", "ref ref-str", w);
										Echo(entry.Field3.ToString("X8"), " ", "unused", w);
										Echo("# string constant: ", "comment", w);
										Echo("\"" + Helpers.ToZeroTerminatedString(data, entry.Field2) + "\"", "\n", "const const-str", w);
										break;
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VINT:
										Echo(entry.ID.ToString("X8"), ": ", "address", w);
										Echo("YKS_VINT", " ", "type type-vint", w);
										Echo(entry.Field1.ToString("X8"), " ", "ref ref-str", w);
										Echo(entry.Field2.ToString("X8"), " ", "unused", w);
										Echo(entry.Field3.ToString("X8"), " ", "ref ref-int", w);
										Echo("# integer variable: ", "comment", w);
										Echo(Helpers.ToZeroTerminatedString(data, entry.Field1), "const const-str", w);
										Echo("[", "comment", w);
										Echo(BitConverter.ToInt32(data, entry.Field3).ToString(), "const, const-int", w);
										Echo("]", "\n", "comment", w);
										break;
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VSTR:
										Echo(entry.ID.ToString("X8"), ": ", "address", w);
										Echo("YKS_VSTR", " ", "type type-vstr", w);
										Echo(entry.Field1.ToString("X8"), " ", "ref ref-str", w);
										Echo(entry.Field2.ToString("X8"), " ", "unused", w);
										Echo(entry.Field3.ToString("X8"), " ", "ref ref-int", w);
										Echo("# string variable: ", "comment", w);
										Echo(Helpers.ToZeroTerminatedString(data, entry.Field1), "const const-str", w);
										Echo("[", "comment", w);
										Echo(BitConverter.ToInt32(data, entry.Field3).ToString(), "const, const-int", w);
										Echo("]", "\n", "comment", w);
										break;
									case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VTMP:
										Echo(entry.ID.ToString("X8"), ": ", "address", w);
										Echo("YKS_VTMP", " ", "type type-vtmp", w);
										Echo(entry.Field1.ToString("X8"), " ", "unused", w);
										Echo(entry.Field2.ToString("X8"), " ", "ref ref-int", w);
										Echo(entry.Field3.ToString("X8"), " ", "unused", w);
										Echo("# temporary variable: $", "comment", w);
										Echo(entry.Field2.ToString(), "\n", "const, const-int", w);
										break;
									default:
										Echo(entry.ID.ToString("X8"), ": ", "address", w);
										Echo(entry.Type.ToString("X"), " ", "type type-unknown", w);
										Echo(entry.Field1.ToString("X8"), " ", "unused", w);
										Echo(entry.Field2.ToString("X8"), " ", "unused", w);
										Echo(entry.Field3.ToString("X8"), " ", "unused", w);
										Echo("#", "\n", "comment", w);
										break;
								}
							}
						}
						FinishSection(w);

						StartSection("Data", w);
						{
							int offset = 0;
							while(offset < data.Length) {
								bool isInt = false, isString = false;
								foreach(var entry in report.Index) {
									switch(entry.Type) {
										case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_FUNC:
											if(entry.Field1 == offset) isString = true;
											break;
										case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CTRL:
											if(entry.Field1 == offset) isString = true;
											if(entry.Field2 == offset) isInt = true;
											break;
										case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CINT:
											if(entry.Field2 == offset) isInt = true;
											break;
										case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_CSTR:
											if(entry.Field2 == offset) isString = true;
											break;
										case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VINT:
											if(entry.Field1 == offset) isString = true;
											if(entry.Field3 == offset) isInt = true;
											break;
										case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VSTR:
											if(entry.Field1 == offset) isString = true;
											if(entry.Field3 == offset) isInt = true;
											break;
										case AnalyzerReport.ScriptBinary.IndexEntry.EntryType.YKS_VTMP:
										default:
											break;
									}
								}
								if(isInt) {
									int res = BitConverter.ToInt32(data, offset);
									Echo(offset.ToString("X8"), ": ", "address", w);
									Echo(res.ToString("X8"), " ", "const const-int", w);
									Echo("(", res.ToString(), ")\n", "const const-int", w);
									offset += 4;
								}
								else if(isString) {
									string res = Helpers.ToZeroTerminatedString(data, offset);
									byte[] bytes = Encoding.GetEncoding("Shift-JIS").GetBytes(res);
									Echo(offset.ToString("X8"), ": ", "address", w);
									Echo(BitConverter.ToString(bytes).Replace('-', ' ') + " 00", " ", "const const-str", w);
									Echo("(", '"' + res + '"', ")\n", "const const-str", w);
									offset += bytes.Length + 1;
								}
								else {
									Echo("Data sector analysis failed.\n", "error", w);
									break;
								}
							}
						}
						FinishSection(w);
					}
					FinishOutput(w);
				}
				else {
					Console.WriteLine("At this point in time only compiled scripts can be analyzed.");
				}

				// TODO analyze other file types
			}
			else {
				Fail("The specified source file does not exist");
			}
			currentFile = "";
			if(flags.Has('w')) {
				Console.ReadLine();
			}
		}

		public TextWriter StartOutput(string filename) {
			var w = new StreamWriter(new FileStream(filename, FileMode.Create));
			w.WriteLine($"<html><head><meta name=\"encoding\" content=\"{w.Encoding.WebName}\"><style>");
			w.WriteLine("pre        { tab-size: 4; -moz-tab-size: 4; }");
			w.WriteLine(".error     { color: #ff0000; font-weigth: bold; }");
			w.WriteLine(".comment   { color: #999999; }");
			w.WriteLine(".unused    { color: #aaaaaa; }");
			w.WriteLine(".const     {  }");
			w.WriteLine(".const-int { color: #1161ff; }");
			w.WriteLine(".const-str { color: #ff1161; }");
			w.WriteLine(".ref       { text-decoration: underline; }");
			w.WriteLine(".ref-fnc   { color: #a605ff; }");
			w.WriteLine(".ref-lbl   { color: #ff7400; }");
			w.WriteLine(".ref-int   { color: #1161ff; }");
			w.WriteLine(".ref-str   { color: #ff1161; }");
			w.WriteLine(".ref-var   { color: #00d67a; }");
			w.WriteLine(".type      { color: #a605ff; }");
			w.WriteLine(".type-func {  }");
			w.WriteLine(".type-ctrl {  }");
			w.WriteLine(".type-cint {  }");
			w.WriteLine(".type-cstr {  }");
			w.WriteLine(".type-vint {  }");
			w.WriteLine(".type-vstr {  }");
			w.WriteLine(".type-vtmp {  }");
			w.WriteLine(".src       {  }");
			w.WriteLine(".src-fnc   { color: #a605ff; }");
			w.WriteLine(".src-lbl   { color: #ff7400; }");
			w.WriteLine(".src-int   { color: #1161ff; }");
			w.WriteLine(".src-str   { color: #ff1161; }");
			w.WriteLine(".src-var   { color: #00d67a; }");
			w.WriteLine("</style></head><body>");
			return w;
		}

		public void FinishOutput(TextWriter w) {
			w.WriteLine("</body></html>");
			w.Close();
		}

		public void StartSection(string title, TextWriter w) {
			w.WriteLine($"<h2>{title}</h2>");
			w.WriteLine("<pre>");
		}

		public void FinishSection(TextWriter w) {
			w.WriteLine("</pre>");
		}

		public void Echo(string value, string type, TextWriter w) => Echo("", value, "", type, w);
		public void Echo(string value, string suffix, string type, TextWriter w) => Echo("", value, suffix, type, w);
		public void Echo(string prefix, string value, string suffix, string type, TextWriter w) {
			if(flags.Has('v')) Console.Write(prefix + value + suffix);
			w.Write($@"{prefix}<span class=""code {type}"">{value}</span>{suffix}");
		}
		public void Echo(string value, TextWriter w) {
			if(flags.Has('v')) Console.Write(value);
			w.Write(value);
		}

		public abstract class AnalyzerReport {
			public FileType Type;

			public AnalyzerReport(FileType type) {
				Type = type;
			}

			public class ScriptBinary : AnalyzerReport {
				public byte[] Magic;
				public short Version;
				public int HeaderSize, Unknown1, CodeOffset, CodeCount, IndexOffset, IndexCount, DataOffset, DataLength, VarPoolSize, Unknown2;

				public IndexEntry[] Index;
				public CodeEntry[] Code;

				public ScriptBinary() : base(FileType.ScriptBinary) { }

				public class IndexEntry {
					public int ID;
					public EntryType Type;
					public int Field1, Field2, Field3;

					public IndexEntry(int id, int type, int field1, int field2, int field3) {
						ID = id;
						Type = (EntryType)type;
						Field1 = field1;
						Field2 = field2;
						Field3 = field3;
					}

					public enum EntryType : int {
						YKS_FUNC = 0x00,
						YKS_CTRL = 0x01,

						YKS_CINT = 0x04,
						YKS_CSTR = 0x05,

						YKS_VINT = 0x08,
						YKS_VSTR = 0x09,
						YKS_VTMP = 0x0A
					}
				}

				public class CodeEntry {
					public int Address;
					public IndexEntry Instruction;
					public IndexEntry[] Arguments;

					public CodeEntry(int address, IndexEntry instruction) {
						Address = address;
						Instruction = instruction;
					}

					public CodeEntry(int address, IndexEntry instruction, IndexEntry[] arguments) {
						Address = address;
						Instruction = instruction;
						Arguments = arguments;
					}
				}
			}

			public enum FileType {
				Unknown, Raw, Folder, Archive,
				ScriptSource, ScriptMeta, ScriptBinary,
				GraphicsSource, GraphicsMeta, GraphicsBinary
			}
		}
	}
}
