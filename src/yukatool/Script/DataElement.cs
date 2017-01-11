using System;
using System.Collections.Generic;
using System.Text;

namespace Yuka.Script {
	abstract class DataElement {
		public int type;

		public DataElement(int type) {
			this.type = type;
		}

		public abstract void WriteData(DataManager m);
		public abstract Tuple<int, int, int, int> GetIndex();

		public const int TYPE_FUNC = 0x00;
		public const int TYPE_CTRL = 0x01;

		public const int TYPE_CINT = 0x04;
		public const int TYPE_CSTR = 0x05;

		public const int TYPE_VINT = 0x08;
		public const int TYPE_VSTR = 0x09;
		public const int TYPE_VTMP = 0x0A;
	}

	class FuncDataElement : DataElement {
		public string name;
		public int lastOffset = -1;
		int offset = -1;

		public FuncDataElement(string name) : base(TYPE_FUNC) {
			this.name = name;
		}

		public override void WriteData(DataManager m) {
			byte[] temp = Encoding.ASCII.GetBytes(name);
			// add terminating zero byte
			byte[] zero = new byte[temp.Length + 1];
			Array.Copy(temp, zero, temp.Length);
			offset = m.Put(zero);
		}

		public override Tuple<int, int, int, int> GetIndex() {
			return new Tuple<int, int, int, int>(TYPE_FUNC, offset, lastOffset, 0);
		}

		public override string ToString() {
			return name;
		}
	}

	class ControlDataElement : DataElement {
		public string name;
		public ControlDataElement link;
		public int codeOffset = -1;
		int nameOffset = -1;
		int linkOffset = -1;

		public ControlDataElement(string name) : base(TYPE_CTRL) {
			this.name = name;
		}

		public override void WriteData(DataManager m) {
			byte[] temp = Encoding.ASCII.GetBytes(name);
			// add terminating zero byte
			byte[] zero = new byte[temp.Length + 1];
			Array.Copy(temp, zero, temp.Length);
			nameOffset = m.Put(zero);
			linkOffset = m.Put(BitConverter.GetBytes(link != null ? link.codeOffset : name.Equals("else") ? -1 : 0xFFFF));
		}

		public override Tuple<int, int, int, int> GetIndex() {
			return new Tuple<int, int, int, int>(TYPE_CTRL, nameOffset, linkOffset, -1);
		}

		public override string ToString() {
			return ':' + name/* + '[' + codeOffset + ']'*/;
		}
	}

	class IntDataElement : DataElement {
		public int data;
		int offset = -1;

		public IntDataElement(int data) : base(TYPE_CINT) {
			this.data = data;
		}

		public override void WriteData(DataManager m) {
			offset = m.Put(BitConverter.GetBytes(data));
		}

		public override Tuple<int, int, int, int> GetIndex() {
			return new Tuple<int, int, int, int>(TYPE_CINT, 0, offset, 0);
		}

		public override string ToString() {
			return data.ToString();
		}
	}

	class StringDataElement : DataElement {
		public string data;
		int offset = -1;

		public StringDataElement(string data) : base(TYPE_CSTR) {
			this.data = data;
		}

		public override void WriteData(DataManager m) {
			byte[] temp = Encoding.GetEncoding("shift-jis").GetBytes(data);
			// add terminating zero byte
			byte[] zero = new byte[temp.Length + 1];
			Array.Copy(temp, zero, temp.Length);
			offset = m.Put(zero);
		}

		public override Tuple<int, int, int, int> GetIndex() {
			return new Tuple<int, int, int, int>(TYPE_CSTR, 0, offset, 0);
		}

		public override string ToString() {
			return '"' + data.Replace("\"", "\"\"") + '"';
		}
	}

	class ExternalStringDataElement : DataElement {
		public string id;
		public Dictionary<string, string> stringTable;
		int offset = -1;

		public ExternalStringDataElement(string id, Dictionary<string, string> stringTable) : base(TYPE_CSTR) {
			this.id = id;
			this.stringTable = stringTable;
		}

		public override void WriteData(DataManager m) {
			byte[] temp;
			if(stringTable.ContainsKey(id) && !id.StartsWith("@")) {
				temp = Encoding.GetEncoding("shift-jis").GetBytes(stringTable[id]);
			}
			else {
				ConsoleColor color = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine(DateTime.Now.ToString() + " Missing translation for @" + id + " in " + Task.currentTask.currentFile);
				Console.ForegroundColor = color;
				temp = Encoding.GetEncoding("shift-jis").GetBytes('@' + id);
			}
			// add terminating zero byte
			byte[] zero = new byte[temp.Length + 1];
			Array.Copy(temp, zero, temp.Length);
			offset = m.Put(zero);
		}

		public override Tuple<int, int, int, int> GetIndex() {
			return new Tuple<int, int, int, int>(TYPE_CSTR, 0, offset, 0);
		}

		public override string ToString() {
			return '@' + id;
		}
	}

	class VarIntRefDataElement : DataElement {
		public string name;
		public int id;
		int nameOffset = -1;
		int idOffset = -1;

		public VarIntRefDataElement(string name, int id) : base(TYPE_VINT) {
			this.name = name;
			this.id = id;
		}

		public override void WriteData(DataManager m) {
			byte[] temp = Encoding.ASCII.GetBytes(name);
			// add terminating zero byte
			byte[] zero = new byte[temp.Length + 1];
			Array.Copy(temp, zero, temp.Length);
			nameOffset = m.Put(zero);
			idOffset = m.Put(BitConverter.GetBytes(id));
		}

		public override Tuple<int, int, int, int> GetIndex() {
			return new Tuple<int, int, int, int>(TYPE_VINT, nameOffset, 0, idOffset);
		}

		public override string ToString() {
			if("Flag".Equals(name)) {
				if(Constants.knownVars.ContainsKey("flag_" + id)) {
					return '$' + Constants.knownVars["flag_" + id];
				}
				return "$flag_" + id;
			}
			else if("GlobalFlag".Equals(name)) {
				if(Constants.knownVars.ContainsKey("globalflag_" + id)) {
					return '$' + Constants.knownVars["globalflag_" + id];
				}
				return "$globalflag_" + id;
			}
			return name + "_" + id;
		}
	}

	class VarStringRefDataElement : DataElement {
		public string name;
		public int id;
		int nameOffset = -1;
		int idOffset = -1;

		public VarStringRefDataElement(string name, int id) : base(TYPE_VSTR) {
			this.name = name;
			this.id = id;
		}

		public override void WriteData(DataManager m) {
			byte[] temp = Encoding.ASCII.GetBytes(name);
			// add terminating zero byte
			byte[] zero = new byte[temp.Length + 1];
			Array.Copy(temp, zero, temp.Length);
			nameOffset = m.Put(zero);
			idOffset = m.Put(BitConverter.GetBytes(id));
		}
		
		public override Tuple<int, int, int, int> GetIndex() {
			return new Tuple<int, int, int, int>(TYPE_VSTR, nameOffset, 0, idOffset);
		}

		public override string ToString() {
			if("String".Equals(name)) {
				if(Constants.knownVars.ContainsKey("str_" + id)) {
					return '$' + Constants.knownVars["str_" + id];
				}
				return "$str_" + id;
			}
			else if("GlobalString".Equals(name)) {
				if(Constants.knownVars.ContainsKey("globalstr_" + id)) {
					return '$' + Constants.knownVars["globalstr_" + id];
				}
				return "$globalstr_" + id;
			}
			return name + "_" + id;
		}
	}

	class VarTempRefDataElement : DataElement {
		public int id;

		public VarTempRefDataElement(int id) : base(TYPE_VTMP) {
			this.id = id;
		}

		public override void WriteData(DataManager m) {
		}

		public override Tuple<int, int, int, int> GetIndex() {
			return new Tuple<int, int, int, int>(TYPE_VTMP, 0, id, 0);
		}

		public override string ToString() {
			return "$" + id;
		}
	}

	class RawDataElement : DataElement {
		public int value;

		public RawDataElement(int value) : base(-1) {
			this.value = value;
		}

		public override void WriteData(DataManager m) {
		}

		public override Tuple<int, int, int, int> GetIndex() {
			return null;
		}

		public override string ToString() {
			return "[" + value + "]";
		}
	}
}
