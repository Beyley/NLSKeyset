using FontStashSharp.Interfaces;
using SDL2;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace NLSKeyset; 

public class SDLFontStashSharpTextureManager : ITexture2DManager {
	public unsafe object CreateTexture(int width, int height) {
		return new SDLTexture(width, height);
	}
	public unsafe Point GetTextureSize(object texture) {
		SDLTexture tex = (SDLTexture)texture;

		return new(tex.Width, tex.Height);
	}
	public unsafe void SetTextureData(object texture, Rectangle bounds, byte[] data) {
		SDLTexture tex = (SDLTexture)texture;

		Rectangle<int> rect = new(bounds.X, bounds.Y, bounds.Width, bounds.Height);
		fixed (void* dataPtr = data)
			SDLWindow.SDL.UpdateTexture(tex.TexturePtr, &rect, dataPtr, bounds.Width * 4);
	}
}
