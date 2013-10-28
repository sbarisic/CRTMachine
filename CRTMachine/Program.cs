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

using DynamicLua;

namespace CRTMachine {
	class Program {
		public static DynamicLua.DynamicLua Lua;
		public static dynamic DLua;

		static void Main(string[] args) {
			Console.Title = "CRTConsole";

			Lua = new DynamicLua.DynamicLua();
			DLua = Lua;

			Machine M = new Machine();
			while (M.Window.IsOpen()) {
				M.Run();
			}
			Environment.Exit(0);
		}
	}

	class Machine {
		const int Width = 60;
		const int Height = 30;

		TextDisplay TextDisplay;
		public RenderWindow Window;

		Shader CRT;
		RenderTexture RT;
		Sprite RTSprite;

		float R, G, B;

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
			RT = new RenderTexture(Window.Size.X, Window.Size.Y);
			RTSprite = new Sprite(RT.Texture);

			Window.Closed += (sender, e) => {
				Window.Close();
				Environment.Exit(0);
			};

			Program.Lua.DoFile("config.lua");
			R = (float) (Program.DLua.R != null ? Program.DLua.R : 0);
			G = (float) (Program.DLua.G != null ? Program.DLua.G : 0);
			B = (float) (Program.DLua.B != null ? Program.DLua.B : 0);

			Program.DLua.print = new Action<string>((S) => {
				TextDisplay.DrawText(CX, CY, S, new Character(foreground: Foreground, background: Background));
			});

			Program.DLua.setforeground = new Action<string>((C) => { Foreground = StringToColor(C); });
			Program.DLua.setbackground = new Action<string>((C) => { Background = StringToColor(C); });
			Program.DLua.setpos = new Action<int, int>((X, Y) => { CX = X; CY = Y; });
		}

		public int StringToColor(string S) {
			ConsoleColor C;
			if (Enum.TryParse<ConsoleColor>(S.Trim(), true, out C)) return (int) C;
			return 0;
		}

		int Foreground = (int) ConsoleColor.Gray;
		int Background = (int) ConsoleColor.Black;
		int CX, CY;

		float T;

		public void Run() {
			Window.DispatchEvents();
			T += .01f;

			RT.Clear(new Color(0, 0, 0, 0));
			StartShader();
			RTSprite.Draw(RT, RenderStates.Default);
			StartShader(false);
			RT.Display();

			Window.Clear(Color.Black);

			TextDisplay.Clear(Character.Transparent);

			/*TextDisplay.DrawText(0, 0, "Hello, world!", new Character(foreground: (int) ConsoleColor.White));
			TextDisplay.DrawText(0, 1, "Hello, world!", new Character(foreground: (int) ConsoleColor.Green));
			TextDisplay.DrawText(0, 2, "Hello, world!", new Character(foreground: (int) ConsoleColor.Red));
			TextDisplay.DrawText(0, 3, "Hello, world!", new Character(foreground: (int) ConsoleColor.Blue));*/

			try {
				Program.Lua.DoFile("main.lua");
			} catch (Exception E) {
				Console.Beep();
				Console.WriteLine("\nLua error:");
				Console.WriteLine(E.Message);
				Console.ReadLine();
			}


			TextDisplay.Draw(Window, new Vector2f(0, 0));
			RTSprite.Draw(Window, RenderStates.Default);

			Window.Display();
		}

		public void StartShader(bool DoStart = true) {
			if (DoStart) {
				CRT.SetParameter("time", T);
				CRT.SetParameter("R", R);
				CRT.SetParameter("G", G);
				CRT.SetParameter("B", B);
				Shader.Bind(CRT);
			} else {
				Shader.Bind(null);
			}
		}
	}
}