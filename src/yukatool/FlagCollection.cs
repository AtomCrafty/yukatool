using System.Collections.Generic;

namespace Yuka {
	public class FlagCollection {
		public static FlagCollection current;

		class Flag {
			internal char? code;
			internal string name;
			internal string desc;
			internal bool value;

			public Flag(char? code, string name, string desc, bool value) {
				this.code = code;
				this.name = name;
				this.desc = desc;
				this.value = value;
			}
		}

		List<Flag> flags = new List<Flag>();

		public void Set(char code) {
			foreach(Flag flag in flags) {
				if(flag.code == code) {
					flag.value = true;
					break;
				}
			}
		}

		public void Set(string name) {
			foreach(Flag flag in flags) {
				if(flag.name.Equals(name)) {
					flag.value = true;
					break;
				}
			}
		}

		public void Unset(char code) {
			foreach(Flag flag in flags) {
				if(flag.code == code) {
					flag.value = false;
					break;
				}
			}
		}

		public void Unset(string name) {
			foreach(Flag flag in flags) {
				if(flag.name.Equals(name)) {
					flag.value = false;
					break;
				}
			}
		}

		public bool Has(char code) {
			foreach(Flag flag in flags) {
				if(flag.code == code) {
					return flag.value;
				}
			}
			return false;
		}

		public bool Has(string name) {
			foreach(Flag flag in flags) {
				if(flag.name.Equals(name)) {
					return flag.value;
				}
			}
			return false;
		}

		public void Add(char? code, string name, string desc, bool value) {
			if(code != null) Remove((char)code);
			if(name != null) Remove(name);
			flags.Add(new Flag(code, name, desc, value));
		}

		public void Add(char? code, string name, bool value) {
			Add(code, name, null, value);
		}

		public void Add(char? code, string name, string desc) {
			Add(code, name, desc, false);
		}

		public void Add(char? code, string name) {
			Add(code, name, null, false);
		}

		public void Remove(char code) {
			for(int i = 0; i < flags.Count; i++) {
				if(flags[i].code == code) {
					flags.RemoveAt(i);
					break;
				}
			}
		}

		public void Remove(string name) {
			for(int i = 0; i < flags.Count; i++) {
				if(flags[i].name.Equals(name)) {
					flags.RemoveAt(i);
					break;
				}
			}
		}
	}
}
