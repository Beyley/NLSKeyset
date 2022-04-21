using System.Runtime.InteropServices;

namespace NLSKeyset; 

[StructLayout(LayoutKind.Sequential)]
public struct Color {
	public byte R;
	public byte G;
	public byte B;
	public byte A;

	public static readonly Color White = new(255, 255, 255);
	public static readonly Color Grey  = new(0x77, 0x77, 0x77);

	public Color(byte r, byte g, byte b, byte a = 255) {
		this.R = r;
		this.G = g;
		this.B = b;
		this.A = a;
	}
}
