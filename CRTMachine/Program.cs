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

	[Serializable]
	public class Machine {
		const int Width = 60;
		const int Height = 30;

		internal TextDisplay TextDisplay;
		public RenderWindow Window;

		Shader CRT;
		RenderTexture RT;
		Sprite RTSprite;

		internal CRT.Config Cfg;
		ASMS ASMScript;

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

			Init();
		}

		public void Init() {
			Cfg = new CRT.Config();

			

			ASMScript = new ASMS("Script");
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

			RT.Clear(new Color(0, 0, 0, 0));
			StartShader();
			RTSprite.Draw(RT, RenderStates.Default);
			StartShader(false);
			RT.Display();

			Window.Clear(Color.Black);

			TextDisplay.Clear(Character.Transparent);

			try {

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
				CRT.SetParameter("R", Cfg.R);
				CRT.SetParameter("G", Cfg.G);
				CRT.SetParameter("B", Cfg.B);
				Shader.Bind(CRT);
			} else {
				Shader.Bind(null);
			}
		}
	}
}