using System.Runtime.InteropServices;

namespace NLSKeyset; 

public class XLibB {
	[DllImport("libX11.so.6", CharSet = CharSet.Ansi)]
	private static extern unsafe void XQueryKeymap(IntPtr display, byte* returnArr);
	
	public static unsafe void QueryKeymap(IntPtr display, ReadOnlySpan<byte> returnArr) {
		if (returnArr.Length != 32)
			throw new Exception("returnArr is not the right length!");
		
		fixed(byte* ptr = returnArr)
			XQueryKeymap(display, ptr);
	}
}
