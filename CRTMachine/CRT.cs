using CRTMachine;
using System;
using System.Text;
using System.IO;

namespace CRT {
	public class System {
		internal Machine Machine;
		internal bool IsReading, IsReadingKey;
		internal char InputChar;
		internal int InputKey;

		public System(Machine M) {
			this.Machine = M;
		}

		public void _Dbg_string(string S) {
			StringBuilder SBldr = new StringBuilder();
			foreach (var chr in S) {
				SBldr.Append("0x");
				SBldr.Append((byte) chr);
				SBldr.Append(" ");
			}
			Console.WriteLine("{0}", SBldr.ToString().Trim());
		}

		public void delete() {
			uint OldPos = Machine.VMem.CaretPosition;
			putchr(' ');
			Machine.VMem.CaretPosition = OldPos;
			Machine.VMem.CaretX++;
		}

		public char read() {
			IsReading = true;
			while (IsReading) ;
			return InputChar;
		}

		public int readkey() {
			IsReadingKey = true;
			while (IsReadingKey) ;
			return InputKey;
		}

		public void read(char c) {
			if (IsReading) {
				InputChar = c;
				putchr(c);
				IsReading = false;
			}
		}

		public void read(int c) {
			if (IsReadingKey) {
				InputKey = c;
				IsReadingKey = false;
			}
		}

		internal void putchr(char c) {
			//_Dbg_string(c.ToString());
			switch (c) {
				case (char) 13:
				case '\n':
					Machine.VMem.CaretY++;
					Machine.VMem.CaretX = 0;
					return;
				case '\t':
					for (int i = 0; i < 4; i++)
						putchr(' ');
					return;
				case '\b':
					Machine.VMem.CaretX--;
					putchr('\0');
					Machine.VMem.CaretX--;
					return;
			}
			uint Cell = Machine.VMem.CaretPosition * 3;
			Machine.VMem[Cell] = (byte) c;
			Machine.VMem[Cell + 1] = (byte) Machine.Cfg.Foreground;
			Machine.VMem[Cell + 2] = (byte) Machine.Cfg.Background;
			Machine.VMem.CaretX++;
		}

		public void print(string S) {
			for (int i = 0; i < S.Length; i++)
				putchr(S[i]);
		}

		public void clear() {
			Machine.VMem.Clear();
		}
	}

	public class Config {
		public bool ShaderEnabled = false;
		public int Foreground = (int) ConsoleColor.Gray;
		public int Background = (int) ConsoleColor.Black;
		public uint Width, Height;
		public double CaretBlinkTime = 400;

		public Config() {
			ShaderEnabled = true;
			Width = 70;
			Height = 25;
		}
	}
}