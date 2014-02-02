using CRTMachine.Texter;
using SFML.Graphics;
using SFML.Window;
using System;
using System.Timers;

namespace CRTMachine {
	class Program {
		static void Main(string[] args) {
			Console.Title = "CRTConsole";

			Machine M = new Machine();
			//System.Threading.Thread RunThread = new System.Threading.Thread(() => {
			while (M.Window.IsOpen()) {
				M.Run();
			}
			/*});
			RunThread.Start();*/
		}
	}

	public class Machine {
		internal TextDisplay TextDisplay;
		internal object THREAD_LOCK = new object();
		internal bool IsFocused = true;
		public RenderWindow Window;

		Shader CRT;
		RenderTexture Render;
		Sprite RenderSprite;

		internal CRT.Config Cfg;
		internal Memory VMem;
		CRT.System Sys;
		Timer CaretTimer;

		bool CaretVisible = true;
		internal bool CaretEnabled = true;
		RectangleShape CaretShape;

		public Machine() {
			Cfg = new global::CRT.Config();
			TextDisplay = new TextDisplay(Cfg.Width, Cfg.Height);

			uint Scale = 2;

			Window = new RenderWindow(new VideoMode(Scale * Cfg.Width * TextDisplay.CharacterWidth, Scale * Cfg.Height * TextDisplay.CharacterHeight), "CRTMachine", Styles.Close);

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

			Window.TextEntered += TextEntered;
			Window.KeyPressed += KeyPressed;
			Window.KeyReleased += KeyReleased;
			/*Window.GainedFocus += (S, E) => { IsFocused = true; };
			Window.LostFocus += (S, E) => { IsFocused = false; };*/

			CaretShape = new RectangleShape(new Vector2f(6, 2));
			VMem = new Memory(TextDisplay.Width, TextDisplay.Height);
			Sys = new global::CRT.System(this);
			CaretTimer = new Timer(Cfg.CaretBlinkTime);
			CaretTimer.Elapsed += (S, E) => {
				CaretVisible = !CaretVisible;
			};
			CaretTimer.AutoReset = true;
			CaretTimer.Start();

			System.Threading.Thread TEST_THREAD = new System.Threading.Thread(() => {
				Sys.print("HELLO WORLD!\nWrite char: ");
				char C = Sys.read();
				Sys.print("\nYou entered: ");
				Sys.print(C.ToString());
			});
			//TEST_THREAD.Start();

			Lua.Initialize();
			//Lua.Remove("string", "xpcall", "package", "os", "loadfile", "error", "load", "setfenv", "dofile", "_VERSION", "loadstring", "gcinfo", "select", "coroutine", "table", "pcall", "debug", "math", "luanet", "module", "rawequal", "io", "assert",  "getfenv", "require");
			// TODO: Fix

			Lua.LoadLibs(Sys);
			Lua.Register("system.run", (T) => {
				Run();
				return null;
			});
		}

		void TextEntered(object sender, TextEventArgs e) {
			Sys.read(e.Unicode[0]);
		}

		void KeyReleased(object sender, KeyEventArgs e) {
		}

		void KeyPressed(object sender, KeyEventArgs e) {
			switch (e.Code) {
				case Keyboard.Key.Up:
					if (CaretEnabled)
						VMem.CaretY--;
					break;
				case Keyboard.Key.Down:
					if (CaretEnabled)
						VMem.CaretY++;
					break;
				case Keyboard.Key.Left:
					if (CaretEnabled)
						VMem.CaretX--;
					break;
				case Keyboard.Key.Right:
					if (CaretEnabled)
						VMem.CaretX++;
					break;
				case Keyboard.Key.Delete:
					Sys.delete();
					break;
				case Keyboard.Key.F1:
					Cfg.ShaderEnabled = !Cfg.ShaderEnabled;
					break;
			}
			Sys.read((int) e.Code);
		}

		public int StringToColor(string S) {
			ConsoleColor C;
			if (Enum.TryParse<ConsoleColor>(S.Trim(), true, out C))
				return (int) C;
			return 0;
		}

		float T;

		public void Run() {
			Window.DispatchEvents();
			T += .01f;
			TextDisplay.Clear(Character.Transparent);
			TextDisplay.DrawImage(0, 0, VMem);
			TextDisplay.Draw(Render, new Vector2f(0, 0));
			if (CaretEnabled && CaretVisible && IsFocused) {
				CaretShape.Position = new Vector2f((int) (VMem.CaretX * TextDisplay.CharacterWidth) + 1, (int) (VMem.CaretY * TextDisplay.CharacterHeight) + 10);
				CaretShape.Draw(Render, RenderStates.Default);
			}
			Render.Display();

			StartShader(true);
			RenderSprite.Draw(Render, RenderStates.Default);
			StartShader(false);
			RenderSprite.Draw(Window, RenderStates.Default);
			Window.Display();

			RunOnce();
		}

		bool DID_RUN = false;
		public void RunOnce() {
			if (DID_RUN)
				return;
			DID_RUN = true;
			Lua.DoFile("Script/bios.lua");
		}

		public void StartShader(bool DoStart = true) {
			if (!Cfg.ShaderEnabled)
				return;
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