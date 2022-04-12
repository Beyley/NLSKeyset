using SDL2;
using static SDL2.SDL;

namespace NLSKeyset; 

public static class Window {
	public static IntPtr WindowPtr;

	public static IntPtr X11WindowPtr;
	public static IntPtr X11DisplayPtr;
	
	public static void Initialize() {
		SDL_Init(0);

		WindowPtr = SDL_CreateWindow("NLS Keyset", 100, 100, 640, 480, SDL_WindowFlags.SDL_WINDOW_HIDDEN);

		SDL_SysWMinfo info = new();
		info.version.major = 2;
		info.version.minor = 0;
		info.version.patch = 0;

		SDL_GetWindowWMInfo(WindowPtr, ref info);

		X11WindowPtr = info.info.x11.window;
		X11DisplayPtr = info.info.x11.display;
	}

	public static void Destroy() {
		SDL_DestroyWindow(WindowPtr);
	}
}
