using System.Numerics;
using System.Transactions;
using SDL2;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace NLSKeyset; 

public static class SDLWindow {
	public static Sdl SDL;
	
	public static unsafe  Window*   SDLWindowPtr;
	public static unsafe  Renderer* SDLRendererPtr;
	private static unsafe Texture*  WhiteTexture;
	private static unsafe Texture*  DisabledTexture;

	public static IntPtr X11WindowPtr;
	public static IntPtr X11DisplayPtr;

	private static uint            SDL_WINDOWPOS_UNDEFINED_MASK = 0x1FFF0000u;
	private static uint            SDL_WINDOWPOS_UNDEFINED_DISPLAY(uint X) => SDL_WINDOWPOS_UNDEFINED_MASK | X;
	private static uint            SDL_WINDOWPOS_UNDEFINED = SDL_WINDOWPOS_UNDEFINED_DISPLAY(0);
	private static Vector2D<int>[] _calculatedPoints;
	private static int             Detail = 8;

	public static unsafe void Initialize() {
		SDL = Sdl.GetApi();
		int sdlInit = SDL.Init(0);

		if (sdlInit != 0) {
			throw new Exception($"SDL Failed to init! msg:{SDL.GetErrorS()}");
		}
		
		CreateWindow();
		GetWindowAndDisplayPtr();
		CreateRenderer();
		
		_calculatedPoints = new Vector2D<int>[Detail];

		for (int i = 0; i < Detail; i++) {
			double angle = 2 * Math.PI * i / Detail;

			angle += Math.PI * (360f / 8f / 360f);

			_calculatedPoints[i] = new((int)(Math.Cos(angle) * 20), (int)(Math.Sin(angle) * 20));
		}
	}

	public const int WIDTH  = 700;
	public const int HEIGHT = 600;
	
	private static unsafe void CreateWindow() {
		SDLWindowPtr = SDL.CreateWindow("NLS Keyset", (int)SDL_WINDOWPOS_UNDEFINED, (int)SDL_WINDOWPOS_UNDEFINED, WIDTH, HEIGHT, (uint)WindowFlags.WindowResizable);
		if (SDLWindowPtr == (void*)0) {
			throw new Exception($"SDL Failed to create window! msg:{SDL.GetErrorS()}");
		}
	}

	private static unsafe void CreateRenderer() {
		SDLRendererPtr = SDL.CreateRenderer(SDLWindowPtr, -1, (uint)RendererFlags.RendererAccelerated);
		if (SDLWindowPtr == (void*)0) {
			throw new Exception($"SDL Failed to create renderer! msg:{SDL.GetErrorS()}");
		}

		SDL.RenderSetLogicalSize(SDLRendererPtr, WIDTH, HEIGHT);
		
		WhiteTexture = SDL.CreateTexture(SDLRendererPtr, (uint)PixelFormatEnum.PixelformatRgba32, (int)TextureAccess.TextureaccessStatic, 1, 1);
		Rectangle<int> rect         = new(0, 0, 1, 1);
		Color          color        = Color.White;
		int            updateResult = SDL.UpdateTexture(WhiteTexture, ref rect, &color, 4);
		if (updateResult != 0) 
			throw new Exception($"SDL Failed to create texture! msg:{SDL.GetErrorS()}");

		DisabledTexture = SDL.CreateTexture(SDLRendererPtr, (uint)PixelFormatEnum.PixelformatRgba32, (int)TextureAccess.TextureaccessStatic, 1, 1);
		color           = Color.Grey;
		updateResult    = SDL.UpdateTexture(DisabledTexture, ref rect, &color, 4);
		if (updateResult != 0)
			throw new Exception($"SDL Failed to create texture! msg:{SDL.GetErrorS()}");

		SDL.SetRenderDrawBlendMode(SDLRendererPtr, BlendMode.BlendmodeBlend);
	}

	private static unsafe void GetWindowAndDisplayPtr() {
		SysWMInfo info = new();
		info.Version.Major = 2;
		info.Version.Minor = 0;
		info.Version.Patch = 0;

		bool getInfo = SDL.GetWindowWMInfo(SDLWindowPtr, &info);
		if (!getInfo) {
			throw new Exception($"Failed to get window info! msg:{SDL.GetErrorS()}");
		}

		X11WindowPtr  = (IntPtr)info.Info.X11.Window;
		X11DisplayPtr = (IntPtr)info.Info.X11.Display;
	}

	public static unsafe void Destroy() {
		SDL.DestroyWindow(SDLWindowPtr);
		SDL.DestroyRenderer(SDLRendererPtr);
	}
	
	public static unsafe void Clear(Color color) {
		SDL.SetRenderDrawColor(SDLRendererPtr, color.R, color.G, color.B, color.A);
		SDL.RenderClear(SDLRendererPtr);
	}
	public static unsafe void DrawRect(Rectangle<int> rect, Color color, bool filled) {
		SDL.SetRenderDrawColor(SDLRendererPtr, color.R, color.G, color.B, color.A);
		if (filled) {
			SDL.RenderFillRect(SDLRendererPtr, &rect);
		}
		else {
			SDL.RenderDrawRect(SDLRendererPtr, &rect);
		}
	}

	public static unsafe void DrawLine(Color color, Vector2D<int> start, Vector2D<int> end) {
		SDL.SetRenderDrawColor(SDLRendererPtr, color.R, color.G, color.B, color.A);

		SDL.RenderDrawLine(SDLRendererPtr, start.X, start.Y, end.X, end.Y);
	}

	public static void DrawOctagon(Color color, Vector2D<int> pos) {
		for (int i = 0; i < _calculatedPoints.Length; i++) {
			DrawLine(
				color,
				_calculatedPoints[i]                                                                  + pos,
				(i == _calculatedPoints.Length - 1 ? _calculatedPoints[0] : _calculatedPoints[i + 1]) + pos
			);
		}
	}
	
	public static unsafe void DrawKey(Vector2D<int> pos, bool filled, bool enabled) {
		const int width      = 80;
		const int height     = 320;
		const int curveAmount = 10;

		if (filled) {
			Color fillColor = enabled ? Color.White : Color.Grey;

			DrawRect(new(pos, new(width, height - curveAmount)), fillColor, true);
			DrawRect(new(new(pos.X              + curveAmount, pos.Y + height - curveAmount), new(width - curveAmount - curveAmount, curveAmount)), fillColor, true);

			Rectangle<int> srcRect   = new(0, 0, 1, 1);
			float          destWidth = MathF.Sqrt(2 * curveAmount * curveAmount);
			FRect          destRect  = new(pos.X, pos.Y + height - curveAmount, destWidth, destWidth);
			FPoint         point     = new(0, 0);

			SDL.RenderCopyExF(SDLRendererPtr, enabled ? WhiteTexture : DisabledTexture, &srcRect, &destRect, -45d, &point, RendererFlip.FlipNone);
			
			destRect = new(pos.X + width + 1, pos.Y + height - curveAmount + 1, destWidth, destWidth);
			SDL.RenderCopyExF(SDLRendererPtr, enabled ? WhiteTexture : DisabledTexture, &srcRect, &destRect, -180 - 45, &point, RendererFlip.FlipNone);
		}
		
		//Top line
		// DrawLine(Color.White, pos, new(pos.X + width, pos.Y));

		//Left line
		DrawLine(Color.White, pos, new(pos.X, pos.Y + height - curveAmount));
		
		//Bottom Left curve
		DrawLine(Color.White, new(pos.X, pos.Y + height - curveAmount), new(pos.X + curveAmount, pos.Y + height));
		
		//Bottom line
		DrawLine(Color.White, new(pos.X + curveAmount, pos.Y + height), new(pos.X + width - curveAmount, pos.Y + height));
		
		//Bottom Right curve
		DrawLine(Color.White, new(pos.X + width - curveAmount, pos.Y + height), new(pos.X + width, pos.Y + height - curveAmount));
		
		//Right line
		DrawLine(Color.White, new(pos.X + width, pos.Y), new(pos.X + width, pos.Y + height - curveAmount));
	}
}
