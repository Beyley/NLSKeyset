using System.Numerics;
using FontStashSharp.Interfaces;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Rectangle = System.Drawing.Rectangle;

namespace NLSKeyset; 

public class SDLFontStashSharpRenderer : IFontStashRenderer {
	public unsafe void Draw(object texture, Vector2 pos, Rectangle? src, System.Drawing.Color color, float rotation, Vector2 origin, Vector2 scale, float depth) {
		SDLTexture tex = (SDLTexture)texture;

		Rectangle<int> rect;

		if (src.HasValue) {
			rect = new(src.Value.X, src.Value.Y, src.Value.Width, src.Value.Height);
		}
		else {
			rect = new(0, 0, tex.Width, tex.Height);
		}

		FPoint center = new() {
			X = 0,
			Y = 0
		};

		FRect destRect = new() {
			X = pos.X,
			Y = pos.Y,
			W = rect.Size.X,
			H = rect.Size.Y
		};

		destRect.W *= scale.X;
		destRect.H *= scale.Y;

		destRect.X -= origin.X;
		destRect.Y -= origin.Y;

		destRect.X = MathF.Round(destRect.X);
		destRect.Y = MathF.Round(destRect.Y);
		destRect.W = MathF.Round(destRect.W);
		destRect.H = MathF.Round(destRect.H);
		
		SDLWindow.SDL.RenderCopyExF(SDLWindow.SDLRendererPtr, tex.TexturePtr, &rect, &destRect, rotation, &center, RendererFlip.FlipNone);
	}
	public ITexture2DManager TextureManager {
		get;
	} = new SDLFontStashSharpTextureManager();
}
