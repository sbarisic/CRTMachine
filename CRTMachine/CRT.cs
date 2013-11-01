using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using CRTMachine;

namespace CRT {
	public class System {
		internal Machine M;

		public System(Machine M) {
			this.M = M;
		}

		public void putchr(char c, bool DontIncreateCursor = false) {
			bool Newline = false;
			switch (c) {
				case '\n':
					Newline = true;
					break;
				default:
					long Cell = (M.Cfg.Y * M.VMem.Width + M.Cfg.X) * 3;
					M.VMem[Cell] = (byte) c;
					M.VMem[Cell + 1] = (byte) M.Cfg.Foreground;
					M.VMem[Cell + 2] = (byte) M.Cfg.Background;
					break;
			}

			if (!DontIncreateCursor) {
				if (!Newline)
					M.Cfg.X++;
				if (M.Cfg.X >= M.VMem.Width || Newline) {
					M.Cfg.X = 0;
					M.Cfg.Y++;
				}
				if (M.Cfg.Y >= M.VMem.Height)
					M.Cfg.Y = 0;
			}
		}

		public void print(string Str) {
			string S = Str.Replace(@"\n", "\n").Replace(@"\t", "    ");
			for (int i = 0; i < S.Length; i++) {
				bool IncCursor = false;
				if (i + 1 < S.Length) IncCursor = S[i + 1] == '\n';
				putchr(S[i], IncCursor);
			}
		}

		public void clear() {
			M.VMem.Clear();
		}
	}

	public class Config {
		public bool ShaderEnabled = false;
		public int Foreground = (int) ConsoleColor.Gray;
		public int Background = (int) ConsoleColor.Black;
		public int X, Y;

		public Config() {
			ShaderEnabled = true;
			X = Y = 0;
		}
	}
}