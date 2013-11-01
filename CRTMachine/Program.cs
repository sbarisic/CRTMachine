using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CRTMachine.Texter;
using SFML.Graphics;
using SFML;
using SFML.Window;
using SFML.Audio;

using System.Reflection;
using System.Runtime.Remoting;

using ASMScript;

namespace CRTMachine {
	class Program {
		static void Main(string[] args) {
			Console.Title = "CRTConsole";

			Machine M = new Machine();
			while (M.Window.IsOpen()) {
				M.Run();
			}
			Environment.Exit(0);
		}
	}

	internal class Memory : TextRenderer {
		private byte[] memory;

		public byte this[long i] {
			get {
				if (i < 0 || i > memory.Length)
					throw new Exception(string.Format("Memory read from {0}", i));

				return memory[i];
			}
			set {
				if (i < 0 || i > memory.Length)
					throw new Exception(string.Format("Memory write to {0}", i));

				memory[i] = value;
			}
		}

		public Memory(uint W, uint H) {
			memory = new byte[W * H * 3];
			Width = W;
			Height = H;
		}

		public void Clear() {
			memory = new byte[memory.Length];
		}

		public override void Set(int x, int y, Character character, bool blend = true) {
			throw new NotImplementedException();
		}

		public override Character Get(int x, int y) {
			var cell = (y * Width + x) * 3;
			return new Character(memory[cell], memory[cell + 1], memory[cell + 2]);
		}
	}

	public class Machine {
		const int Width = 60;
		const int Height = 30;

		internal TextDisplay TextDisplay;
		public RenderWindow Window;

		Shader CRT;
		RenderTexture Render;
		Sprite RenderSprite;

		internal CRT.Config Cfg;
		ASMS ASMScript;
		internal Memory VMem;
		CRT.System Sys;

		public Machine() {
			TextDisplay = new TextDisplay(Width, Height);

			uint Scale = 2;

			Window = new RenderWindow(new VideoMode(Scale * Width * TextDisplay.CharacterWidth, Scale * Height * TextDisplay.CharacterHeight), "CRTMachine", Styles.Close);

			View V = Window.GetView();
			V.Zoom(1f / Scale);
			V.Move(V.Size / -2);
			Window.SetView(V);

			Window.SetFramerateLimit(60);

			CRT = new Shader("shaders/empty.vert", "shaders/CRT.frag");
			Render = new RenderTexture(Window.Size.X, Window.Size.Y);
			RenderSprite = new Sprite(Render.Texture);

			Window.Closed += (sender, e) => {
				Window.Close();
				Environment.Exit(0);
			};

			Init();
		}

		public void Init() {
			Cfg = new CRT.Config();
			VMem = new Memory(TextDisplay.Width, TextDisplay.Height);
			Sys = new global::CRT.System(this);

			ASMScript = new ASMS("Script");
			ASMScript.OpcodeList.Add("SETPOS", (A) => {
				Sys.M.Cfg.X = (int) A[0];
				Sys.M.Cfg.Y = (int) A[1];
				return null;
			});
			ASMScript.OpcodeList.Add("PRINT", (A) => {
				if (A != null) foreach (var I in A) if (I != null) Sys.print(I.ToString());
				return null;
			});

			ASMScript.LoadFile("main.asms");
		}

		public int StringToColor(string S) {
			ConsoleColor C;
			if (Enum.TryParse<ConsoleColor>(S.Trim(), true, out C)) return (int) C;
			return 0;
		}

		float T;

		public void Run() {
			Window.DispatchEvents();
			T += .01f;
			TextDisplay.Clear(Character.Transparent);
			TextDisplay.DrawImage(0, 0, VMem);
			TextDisplay.Draw(Render, new Vector2f(0, 0));
			Render.Display();
			StartShader(true);
			RenderSprite.Draw(Render, RenderStates.Default);
			StartShader(false);
			RenderSprite.Draw(Window, RenderStates.Default);
			Window.Display();
		}

		public void StartShader(bool DoStart = true) {
			if (!Cfg.ShaderEnabled) return;
			if (DoStart) {
				CRT.SetParameter("texture", Shader.CurrentTexture);
				CRT.SetParameter("time", T);
				CRT.SetParameter("resolution", Render.DefaultView.Size);
				Shader.Bind(CRT);
			} else {
				Shader.Bind(null);
			}
		}
	}
}