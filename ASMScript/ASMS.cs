using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ASMScript {
	public class ASMS {
		public delegate object csOpcode(params object[] Args);
		internal string ScriptFolderDir;
		public Dictionary<string, csOpcode> OpcodeList;
		internal Dictionary<int, string[]> ProcessedOpcodes;
		internal int PosPtr;
		internal int[] Positions;
		internal List<object> STACK;
		internal int STACKPTR;
		internal object TEMP;

		public ASMS(string ScriptDir) {
			if (Directory.Exists(ScriptDir)) {
				ScriptFolderDir = Path.GetFullPath(ScriptDir);
			} else throw new InvalidOperationException("Script directory \"" + ScriptDir + "\" does not exist!");
			OpcodeList = new Dictionary<string, csOpcode>();
			ProcessedOpcodes = new Dictionary<int, string[]>();
			STACK = new List<object>();
			STACKPTR = -1;

			OpcodeList.Add("GOTO", (A) => {
				GOTO((int) A[0]);
				return null;
			});

			OpcodeList.Add("PUSH", (A) => {
				STACK.Add(TEMP);
				STACKPTR++;
				TEMP = null;
				return null;
			});

			OpcodeList.Add("SAVE", (A) => {
				return A[0];
			});
		}

		public void LoadString(string S) {
			ProcessedOpcodes.Clear();
			string[] Lines = S.Trim().Replace("\r", "").Split('\n');
			foreach (var L in Lines) if (L.Trim().Length > 0) Execute(L.Trim());
			var SPOpcodes = (from KVPair in ProcessedOpcodes orderby KVPair.Key ascending select KVPair).ToArray();
			Positions = new int[SPOpcodes.Length];
			for (int i = 0; i < SPOpcodes.Length; i++) Positions[i] = SPOpcodes[i].Key;
			for (PosPtr = 0; PosPtr < Positions.Length; PosPtr++) 
				TEMP = Execute(Positions[PosPtr]);
		}

		public void LoadFile(string path) {
			if (File.Exists(path)) {
				LoadString(File.ReadAllText(path));
			} else if (File.Exists(Path.Combine(ScriptFolderDir, path))) {
				LoadString(File.ReadAllText(Path.Combine(ScriptFolderDir, path)));
			} else throw new Exception("Script file not found: " + path);
		}

		public void Execute(string Instruction) {
			string[] Instr = Split(Instruction.Trim());
			int N = int.Parse(Instr[0]);
			if (!OpcodeList.ContainsKey(Instr[1].ToUpper())) throw new InvalidOperationException("Instruction on line " + N + " not found " + Instr[1].ToUpper());
			else {
				string[] PInstr = new string[Instr.Length - 1];
				Array.Copy(Instr, 1, PInstr, 0, Instr.Length - 1);
				ProcessedOpcodes.Add(N, PInstr);
			}
		}

		internal void GOTO(int N) {
			PosPtr = Array.IndexOf(Positions, N) - 1;
		}

		internal object Execute(int N) {
			string[] Args;
			try {
				Args = new string[ProcessedOpcodes[N].Length];
			} catch (KeyNotFoundException E) {
				Console.WriteLine("Key not found {0}", N);
				throw;
			}
			Array.Copy(ProcessedOpcodes[N], 1, Args, 0, ProcessedOpcodes[N].Length - 1);
			return OpcodeList[ProcessedOpcodes[N][0]](ToObject(Args));
		}

		internal object ToObject(string S) {
			if (S == null) return null;
			object R = null;
			int IntR;
			if (int.TryParse(S, out IntR)) return IntR;
			if (S.ToUpper().StartsWith("@PEEK")) return STACK[STACKPTR - int.Parse(S.Split(':')[1])];
			R = S;
			return R;
		}

		internal object[] ToObject(string[] S) {
			List<object> R = new List<object>();
			for (int i = 0; i < S.Length; i++) R.Add(ToObject(S[i]));
			return R.ToArray();
		}

		internal string[] Split(string S) {
			List<string> Ret = new List<string>();
			StringBuilder Buffer = new StringBuilder();
			bool SingleQuoting, DoubleQuoting, DidPush;
			SingleQuoting = DoubleQuoting = DidPush = false;
			foreach (char Chr in S) switch (Chr) {
					case '\'':
						if (DoubleQuoting) goto default;
						SingleQuoting = !SingleQuoting;
						if (!SingleQuoting) goto case ' ';
						break;
					case '\"':
						if (SingleQuoting) goto default;
						DoubleQuoting = !DoubleQuoting;
						if (!DoubleQuoting) goto case ' ';
						break;
					case ' ':
						if (DidPush) break;
						DidPush = true;
						if (SingleQuoting || DoubleQuoting) goto default;
						//if (Buffer.ToString().Trim().Length == 0) break;
						Ret.Add(Buffer.ToString()/*.Trim()*/);
						Buffer.Clear();
						break;
					default:
						Buffer.Append(Chr);
						DidPush = false;
						break;
				}
			if (Buffer.ToString().Trim().Length > 0) Ret.Add(Buffer.ToString().Trim());
			return Ret.ToArray();
		}

	}
}