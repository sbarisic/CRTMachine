using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CRTMachine;

namespace CRT {
	public class System {
		internal Machine M;

		public System(Machine M) {
			this.M = M;
		}

		public void print(string S) {
			try {
				M.TextDisplay.DrawText(M.Cfg.X, M.Cfg.Y, S, new CRTMachine.Texter.Character(foreground: M.Cfg.Foreground, background: M.Cfg.Background));
			} catch (Exception E) {
				Console.WriteLine(E);
			}

		}
	}

	public class Config {
		public float R, G, B;
		public int Foreground = (int) ConsoleColor.Gray;
		public int Background = (int) ConsoleColor.Black;
		public int X = 0;
		public int Y = 0;

		public Config() {
			R = G = B = X = Y = 0;
		}
	}
}