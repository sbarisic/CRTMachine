using CRTMachine.Texter;
using System;

namespace CRTMachine {
	internal class Memory : TextRenderer {
		private byte[] memory;
		public uint CaretPosition = 0;
		public uint MinX, MaxX, MinY, MaxY;
		public uint CaretX {
			set {
				if (value < MinX || value > MaxX - 1)
					return;
				CaretPosition = CaretY * Width + value;
			}
			get {
				return CaretPosition % Width;
			}
		}
		public uint CaretY {
			set {
				if (value > MaxY - 1 || value < MinY)
					return;
				CaretPosition = value * Width + CaretX;
			}
			get {
				return CaretPosition / Width;
			}
		}

		public string GetText(uint STARTt) {
			uint START = STARTt * 3;
			string T = "";
			char C = '\0';
			int Counter = -1;
			do {
				Counter++;
				if (Counter % 3 == 0) {
					C = (char) this[(uint) (START + Counter)];
					if (C != '\0') T += C.ToString();
				}
			} while (C != '\0');
			return T;
		}

		public byte this[uint i] {
			get {
				if (!IsInsideBounds(i))
					throw new Exception(string.Format("Memory read from {0}", i));
				return memory[i];
			}
			set {
				if (!IsInsideBounds(i))
					throw new Exception(string.Format("Memory write to {0}", i));
				memory[i] = value;
			}
		}

		public Memory(uint W, uint H) {
			memory = new byte[W * H * 3];
			Width = W;
			Height = H;
			ResetLimits();
		}

		public void ResetLimits() {
			MinX = MinY = 0;
			MaxX = Width;
			MaxY = Height;
		}

		public bool IsInsideBounds(uint i) {
			if (i >= memory.Length)
				return false;
			return true;
		}

		public void Clear(byte ClearByte = 0) {
			for (int i = 0; i < memory.Length; i++)
				memory[i] = ClearByte;
		}

		public override void Set(int x, int y, Character character, bool blend = true) {
			uint cell = (uint) ((y * Width + x) * 3);
			this[cell] = (byte) character.Glyph;
			this[cell + 1] = (byte) character.Foreground;
			this[cell + 2] = (byte) character.Background;
		}

		public override Character Get(int x, int y) {
			var cell = (y * Width + x) * 3;
			return new Character(memory[cell], memory[cell + 1], memory[cell + 2]);
		}
	}
}