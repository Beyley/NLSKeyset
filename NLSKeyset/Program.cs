using Desktop.Robot;
using Desktop.Robot.Extensions;
using SDL2;
using X11;

namespace NLSKeyset;

public static class Program {
	public static byte ChordState = 0;
	public static byte KeyState = 0;

	public static KeyCode Keycode1;
	public static KeyCode Keycode2;
	public static KeyCode Keycode3;
	public static KeyCode Keycode4;
	public static KeyCode Keycode5;

	public static  KeyCode KeycodeDisable;
	private static KeyCode KeycodeEnable;

	private static X11.Window XWindow;
	private static KeySyms    LatestKeyPress;
	private static Robot      RobotTyper;

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
	
	private static bool GetKeyBit(byte bit) => (KeyState & (1 << bit)) != 0;

	private static bool triggered = false;
	private static void SetKeyBit(byte bit, bool value) {
		byte origState = KeyState;

		if (GetKeyBit(bit) == value) return;

		if (value)
			KeyState = (byte)(KeyState | (1 << bit));
		else
			KeyState = (byte)(KeyState & ~(1 << bit));

		if (origState != KeyState) {
			Console.WriteLine($"Key state changed! {KeyState:x2}");
			if (!value && !triggered) {
				TriggerChord(origState);
				triggered = true;
			}

			if (KeyState == 0)
				triggered = false;
		}
	}

	private static void TriggerKeyPress(char key) {
		UnbindKeys();
		Xlib.XFlush(Window.X11DisplayPtr);
		Xlib.XSync(Window.X11DisplayPtr, false);
	
		// RobotTyper.KeyDown(key);
		// RobotTyper.KeyUp(key);
		RobotTyper.Type(key.ToString());
		Xlib.XFlush(Window.X11DisplayPtr);
		Xlib.XSync(Window.X11DisplayPtr, false);
		Console.WriteLine($"Typing key: {key}");
		
		BindKeys();
		Xlib.XFlush(Window.X11DisplayPtr);
		Xlib.XSync(Window.X11DisplayPtr, false);
	}
	
	private static void TriggerChord(byte state) {
		if (state == 0)
			return;

		if (ChordStates.States.TryGetValue(state, out char key)) {
			Console.WriteLine($"Typing chord {state}:{key}");
			TriggerKeyPress(key);
		}
		else {
			Console.WriteLine($"Invalid Chord! {state:x2}");
		}
		
		// ChordState = 0;
	}

	private const KeyButtonMask MASK        = KeyButtonMask.Mod2Mask;
	private const KeyButtonMask ENABLE_MASK = MASK | KeyButtonMask.ShiftMask;

	private static unsafe void BindKeys() {
		Xlib.XGrabKey(Window.X11DisplayPtr, Keycode1, MASK, XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, Keycode2, MASK, XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, Keycode3, MASK, XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, Keycode4, MASK, XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, Keycode5, MASK, XWindow, false, GrabMode.Async, GrabMode.Async);

		Xlib.XGrabKey(Window.X11DisplayPtr, KeycodeDisable, MASK, XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, KeycodeEnable, ENABLE_MASK, XWindow, false, GrabMode.Async, GrabMode.Async);

		bool test;
		XLibB.XkbSetDetectableAutoRepeat(Window.X11DisplayPtr, true, &test);
		
		Xlib.XFlush(Window.X11DisplayPtr);
	}

	private static void UnbindKeys() {
		Xlib.XUngrabKey(Window.X11DisplayPtr, Keycode1, MASK, XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, Keycode2, MASK, XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, Keycode3, MASK, XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, Keycode4, MASK, XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, Keycode5, MASK, XWindow);

		Xlib.XUngrabKey(Window.X11DisplayPtr, KeycodeDisable, MASK, XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, KeycodeEnable, ENABLE_MASK, XWindow);

		Xlib.XFlush(Window.X11DisplayPtr);
	}

	private static void GetKeycodes() {
		Keycode5 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.a);
		Keycode4 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.s);
		Keycode3 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.h);
		Keycode2 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.t);
		Keycode1 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.space);

		KeycodeDisable  = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.g);
		KeycodeEnable = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.space);
	}

	public static unsafe void Main(string[] args) {
		Window.X11DisplayPtr = Xlib.XOpenDisplay(":0");
		
		XWindow = Xlib.XDefaultRootWindow(Window.X11DisplayPtr);

		RobotTyper = new Robot();

		GetKeycodes();
		BindKeys();

		// Xlib.XSelectInput(Window.X11DisplayPtr, XWindow, EventMask.KeyPressMask | EventMask.KeyReleaseMask);
		// Xlib.XFlush(Window.X11DisplayPtr);

		XAnyEvent  ev          = new();
		XAnyEvent* evPtr       = &ev;
		IntPtr     eventReturn = (IntPtr)evPtr;

		byte[] queryReturnArr = new byte[32];

		int pressedKeys = 0;
		
		bool continueRunning = true;
		// fixed(void* ptr = ev)
		while (continueRunning) {
			XLibB.QueryKeymap(Window.X11DisplayPtr, queryReturnArr);

			SetKeyBit(0, (queryReturnArr[8] & 2)   == 2); //Space
			SetKeyBit(1, (queryReturnArr[5] & 2)   == 2); //T
			SetKeyBit(2, (queryReturnArr[5] & 1)   == 1); //H
			SetKeyBit(3, (queryReturnArr[4] & 128) == 128); //S
			SetKeyBit(4, (queryReturnArr[4] & 64)  == 64); //A

			Thread.Sleep(5);
			// Xlib.XNextEvent(Window.X11DisplayPtr, eventReturn);
			// switch (ev.type) {
			// 	case (int)Event.KeyPress: { // Console.WriteLine($"Got KeyPress Event!");
			// 		// if (LatestKeyPress != 0) continue;
			// 		
			// 		XKeyEvent evKey = *(XKeyEvent*)evPtr;
			//
			// 		pressedKeys++;
			// 		
			// 		KeyCode code = (KeyCode)evKey.keycode;
			//
			// 		XLibB.QueryKeymap(Window.X11DisplayPtr, queryReturnArr); //6=4
			//
			// 		//    a is [4] = 64
			// 		//    s is [4] = 128
			// 		//    h is [5] = 1
			// 		//    t is [5] = 2
			// 		//space is [8] = 2
			// 		
			// 		if (code == KeycodeEnable && (queryReturnArr[6] & 4) == 4) {
			// 			Console.WriteLine("Enable Keyset!");
			// 			continue;
			// 		}
			// 		
			// 		if (code == Keycode1) {
			// 			SetKeyBit(0, true);
			// 			SetChordBit(0, true);
			// 		}
			// 		else if (code == Keycode2) {
			// 			SetKeyBit(1, true);
			// 			SetChordBit(1, true);
			// 		}
			// 		else if (code == Keycode3) {
			// 			SetKeyBit(2, true);
			// 			SetChordBit(2, true);
			// 		}
			// 		else if (code == Keycode4) {
			// 			SetKeyBit(3, true);
			// 			SetChordBit(3, true);
			// 		}
			// 		else if (code == Keycode5) {
			// 			SetKeyBit(4, true);
			// 			SetChordBit(4, true);
			// 		}
			// 		else if (code == KeycodeDisable && ChordState == 0) {
			// 			Console.WriteLine("Disable Keyset!");
			// 		}
			//
			// 		break;
			// 	}
			// 	case (int)Event.KeyRelease: {
			// 		XKeyEvent evKey = *(XKeyEvent*)evPtr;
			//
			// 		// #region I HATE THIS
			// 		// if (pressedKeys != 0) {
			// 		// 	pressedKeys--;
			// 		// 	if (pressedKeys == 0) {
			// 		// 		Xlib.XFlush(Window.X11DisplayPtr);
			// 		// 		XLibB.XUngrabKeyboard(Window.X11DisplayPtr, evKey.time);
			// 		// 		Console.WriteLine("ungrabbed");
			// 		// 	}
			// 		// 	else {
			// 		// 		XLibB.XGrabKeyboard(Window.X11DisplayPtr, XWindow, false, GrabMode.Async, GrabMode.Async, evKey.time);
			// 		// 		Console.WriteLine("grabbed");
			// 		// 	}
			// 		// }
			// 		// #endregion
			// 		//
			// 		// KeyCode code = (KeyCode)evKey.keycode;
			// 		//
			// 		// // if (code == Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)LatestKeyPress)) {
			// 		// // 	LatestKeyPress = 0;
			// 		// // }
			// 		//
			// 		// if (evKey.keycode == (ulong)KeycodeDisable) continue;
			// 		//
			// 		//
			// 		// if (code == Keycode1) {
			// 		// 	SetKeyBit(0, false);
			// 		// }
			// 		// else if (code == Keycode2) {
			// 		// 	SetKeyBit(1, false);
			// 		// }
			// 		// else if (code == Keycode3) {
			// 		// 	SetKeyBit(2, false);
			// 		// }
			// 		// else if (code == Keycode4) {
			// 		// 	SetKeyBit(3, false);
			// 		// }
			// 		// else if (code == Keycode5) {
			// 		// 	SetKeyBit(4, false);
			// 		// }
			// 		//
			// 		// if(KeyState == 0)
			// 		// 	TriggerChord();
			//
			// 		break;
			// 	}
			// }
		}

		UnbindKeys();

		Xlib.XCloseDisplay(Window.X11DisplayPtr);
	}
}
