using SDL2;
using Silk.NET.SDL;

namespace NLSKeyset; 

public class SDLTexture {
	public unsafe Texture* TexturePtr;

	public int Width;
	public int Height;

	public unsafe SDLTexture(int width, int height) {
		this.TexturePtr = SDLWindow.SDL.CreateTexture(SDLWindow.SDLRendererPtr, SDL.SDL_PIXELFORMAT_RGBA8888, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, width, height);

		SDLWindow.SDL.SetTextureBlendMode(this.TexturePtr, BlendMode.BlendmodeBlend);
		// SDLWindow.SDL.SetTextureScaleMode(this.TexturePtr, ScaleMode.ScaleModeNearest);

		this.Width  = width;
		this.Height = height;
	}
}
