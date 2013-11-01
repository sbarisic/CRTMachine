using CRT;

public static class Program {
	static CRT.Config Cfg;
	static CRT.System Sys;

	public static void Main(CRT.Config Cfg, CRT.System Sys) {
		Program.Cfg = Cfg;
		Program.Sys = Sys;
	}

	public static void Run() {
		SetPos(10, 10);
		Sys.print("Hello World!");
	}

	public static void SetPos(int X, int Y) {
		Cfg.X = X;
		Cfg.Y = Y;
	}
}