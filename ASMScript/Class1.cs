using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ASMScript {
	public class ASMS {
		internal string ScriptFolderDir;

		public ASMS(string ScriptDir) {
			if (Directory.Exists(ScriptDir)) {
				ScriptFolderDir = Path.GetFullPath(ScriptDir);
			} else throw new InvalidOperationException("Script directory \"" + ScriptDir + "\" does not exist!");
		}

		public void DoString(string S) {

		}

		public void DoFile(string path) {
			string Contents;
			if (File.Exists(path)) {
				Contents = File.ReadAllText(path);
			} else if (File.Exists(Path.Combine(ScriptFolderDir, path))) {
				DoString(File.ReadAllText(Path.Combine(ScriptFolderDir, path)));
			} else throw new Exception("Script file not found: " + path);
		}

	}
}