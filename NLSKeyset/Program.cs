using X11;

namespace NLSKeyset;

public static class Program {
	public static byte ChordState = 0;

	public static KeyCode Keycode1;
	public static KeyCode Keycode2;
	public static KeyCode Keycode3;
	public static KeyCode Keycode4;
	public static KeyCode Keycode5;

	public static  KeyCode KeycodeReset;
	private static KeyCode KeycodeEnable;

	private static X11.Window XWindow;

	private static bool Press1;
	private static bool Press2;
	private static bool Press3;
	private static bool Press4;
	private static bool Press5;

	private static bool GetChordBit(byte bit) => (ChordState & (1 << bit)) != 0;

	private static void SetChordBit(byte bit, bool value) {
		byte origState = ChordState;

		if (GetChordBit(bit) == value) return;

		if (value)
			ChordState = (byte)(ChordState | (1 << bit));
		else
			ChordState = (byte)(ChordState & ~(1 << bit));

		if (origState != ChordState) {
			Console.WriteLine($"Chord state changed! {ChordState:x2}");
		}
	}

	private static void TriggerChord() {
		if (ChordState == 0)
			return;

		Console.WriteLine($"User input chord {ChordState:x2}");
		ChordState = 0;
	}

	private const KeyButtonMask MASK        = KeyButtonMask.Mod2Mask;
	private const KeyButtonMask ENABLE_MASK = MASK | KeyButtonMask.ShiftMask;

	private static void BindKeys() {
		Xlib.XGrabKey(Window.X11DisplayPtr, Keycode1, MASK, XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, Keycode2, MASK, XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, Keycode3, MASK, XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, Keycode4, MASK, XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, Keycode5, MASK, XWindow, false, GrabMode.Async, GrabMode.Async);

		Xlib.XGrabKey(Window.X11DisplayPtr, KeycodeReset, MASK, XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, KeycodeEnable, ENABLE_MASK, XWindow, false, GrabMode.Async, GrabMode.Async);

		Xlib.XFlush(Window.X11DisplayPtr);
	}

	private static void UnbindKeys() {
		Xlib.XUngrabKey(Window.X11DisplayPtr, Keycode1, MASK, XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, Keycode2, MASK, XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, Keycode3, MASK, XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, Keycode4, MASK, XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, Keycode5, MASK, XWindow);

		Xlib.XUngrabKey(Window.X11DisplayPtr, KeycodeReset, MASK, XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, KeycodeEnable, ENABLE_MASK, XWindow);

		Xlib.XFlush(Window.X11DisplayPtr);
	}

	private static void GetKeycodes() {
		Keycode5 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.a);
		Keycode4 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.s);
		Keycode3 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.h);
		Keycode2 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.t);
		Keycode1 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.space);

		KeycodeReset  = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.g);
		KeycodeEnable = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.space);
	}

	public static unsafe void Main(string[] args) {
		Window.Initialize();

		XWindow = Xlib.XDefaultRootWindow(Window.X11DisplayPtr);

		GetKeycodes();

		BindKeys();

		Xlib.XSelectInput(Window.X11DisplayPtr, XWindow, EventMask.KeyPressMask | EventMask.KeyReleaseMask);
		Xlib.XFlush(Window.X11DisplayPtr);

		XAnyEvent  ev          = new();
		XAnyEvent* evPtr       = &ev;
		IntPtr     eventReturn = (IntPtr)evPtr;

		byte[] queryreturnArr = new byte[32];

		bool continueRunning = true;
		// fixed(void* ptr = ev)
		while (continueRunning) {
			if (Xlib.XPending(Window.X11DisplayPtr) > 0) {
				Xlib.XNextEvent(Window.X11DisplayPtr, eventReturn);
				switch (ev.type) {
					case (int)Event.KeyPress: { // Console.WriteLine($"Got KeyPress Event!");
						XKeyEvent evKey = *(XKeyEvent*)evPtr;

						KeyCode code = (KeyCode)evKey.keycode;

						XLibB.QueryKeymap(Window.X11DisplayPtr, queryreturnArr); //6=4

						if ((queryreturnArr[6] & 4) == 4) {
							Console.WriteLine($"Enable keyset!");
							continue;
						}

						if (code == Keycode1) {
							Press1 = true;
						}
						else if (code == Keycode2) {
							Press2 = true;
						}
						else if (code == Keycode3) {
							Press3 = true;
						}
						else if (code == Keycode4) {
							Press4 = true;
						}
						else if (code == Keycode5) {
							Press5 = true;
						}
						else if (code == KeycodeReset && ChordState == 0) {
							Console.WriteLine("Should disable Keyset!");
						}

						Console.WriteLine($"{code} pressed");

						break;
					}
					case (int)Event.KeyRelease: {
						XKeyEvent evKey = *(XKeyEvent*)evPtr;

						KeyCode code = (KeyCode)evKey.keycode;

						if (code == Keycode1) {
							Press1 = false;
						}
						else if (code == Keycode2) {
							Press2 = false;
						}
						else if (code == Keycode3) {
							Press3 = false;
						}
						else if (code == Keycode4) {
							Press4 = false;
						}
						else if (code == Keycode5) {
							Press5 = false;
						}

						Console.WriteLine($"{code} depressed");

						if (evKey.keycode == (ulong)KeycodeReset) continue;

						TriggerChord();
						break;
					}
				}
			}
			else {
				Thread.Sleep(100);
			}
		}

		UnbindKeys();

		Environment.Exit(0);
		// Window.Destroy();
	}
}
