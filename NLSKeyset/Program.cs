using System.Diagnostics;
using Desktop.Robot;
using Desktop.Robot.Extensions;
using SDL2;
using X11;

namespace NLSKeyset;

public static class Program {
	private static byte _KeyState;

	private static KeyCode _Keycode1;
	private static KeyCode _Keycode2;
	private static KeyCode _Keycode3;
	private static KeyCode _Keycode4;
	private static KeyCode _Keycode5;

	private static KeyCode _KeycodeDisable;
	private static KeyCode _KeycodeEnable;

	private static X11.Window _XWindow;
	private static Robot      _RobotTyper;

	private static bool GetKeyBit(byte bit) => (_KeyState & (1 << bit)) != 0;

	private static bool _Enabled = true;
	
	// private static bool triggered = false;
	private static byte _ToTrigger;
	private static void SetKeyBit(byte bit, bool value) {
		byte origState = _KeyState;

		if (GetKeyBit(bit) == value) return;

		if (value)
			_KeyState = (byte)(_KeyState | (1 << bit));
		else
			_KeyState = (byte)(_KeyState & ~(1 << bit));

		Console.WriteLine($"Key state changed! {_KeyState:x2}");
		
		if(GetKeyBit(5))    //check if control is depressed
			if (bit == 0 && value) { //if we pressed space
				_Enabled = !_Enabled;
				Console.WriteLine($"Enabled set to {_Enabled}");
				
				if(_Enabled)
					BindKeys();
				else
					UnbindKeys();
			}
		
		if (!_Enabled) {
			_ToTrigger = 0;
		}
		else if(bit <= 4) {
			if (_KeyState > origState)
				_ToTrigger = _KeyState;

			if (_KeyState == 0) {
				//Make sure we only pass the last 5 bits (the 5 key positions)
				TriggerChord((byte)(_ToTrigger & 0b00011111));
			}
		}
	}

	private static void TriggerKeyPress(char key) {
		UnbindKeys();
		Xlib.XFlush(Window.X11DisplayPtr);
		Xlib.XSync(Window.X11DisplayPtr, false);
	
		_RobotTyper.KeyPress(key);
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
	}

	private const KeyButtonMask MASK        = KeyButtonMask.Mod2Mask;
	private const KeyButtonMask ENABLE_MASK = MASK | KeyButtonMask.ShiftMask;

	private static unsafe void BindKeys() {
		Xlib.XGrabKey(Window.X11DisplayPtr, _Keycode1, MASK, _XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, _Keycode2, MASK, _XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, _Keycode3, MASK, _XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, _Keycode4, MASK, _XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, _Keycode5, MASK, _XWindow, false, GrabMode.Async, GrabMode.Async);

		Xlib.XGrabKey(Window.X11DisplayPtr, _KeycodeDisable, MASK, _XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(Window.X11DisplayPtr, _KeycodeEnable, ENABLE_MASK, _XWindow, false, GrabMode.Async, GrabMode.Async);

		bool test;
		XLibB.XkbSetDetectableAutoRepeat(Window.X11DisplayPtr, true, &test);
		
		Xlib.XFlush(Window.X11DisplayPtr);
	}

	private static void UnbindKeys() {
		Xlib.XUngrabKey(Window.X11DisplayPtr, _Keycode1, MASK, _XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, _Keycode2, MASK, _XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, _Keycode3, MASK, _XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, _Keycode4, MASK, _XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, _Keycode5, MASK, _XWindow);

		Xlib.XUngrabKey(Window.X11DisplayPtr, _KeycodeDisable, MASK, _XWindow);
		Xlib.XUngrabKey(Window.X11DisplayPtr, _KeycodeEnable, ENABLE_MASK, _XWindow);

		Xlib.XFlush(Window.X11DisplayPtr);
	}

	private static void GetKeycodes() {
		_Keycode5 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.a);
		_Keycode4 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.s);
		_Keycode3 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.h);
		_Keycode2 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.t);
		_Keycode1 = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.space);

		_KeycodeDisable  = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.g);
		_KeycodeEnable = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.space);
	}

	public static unsafe void Main(string[] args) {
		Window.X11DisplayPtr = Xlib.XOpenDisplay(":0");
		
		_XWindow = Xlib.XDefaultRootWindow(Window.X11DisplayPtr);

		_RobotTyper = new Robot();

		GetKeycodes();
		BindKeys();
		Xlib.XFlush(Window.X11DisplayPtr);

		byte[] queryReturnArr = new byte[32];

		int pressedKeys = 0;
		
		bool continueRunning = true;
		
		while (continueRunning) {
			XLibB.QueryKeymap(Window.X11DisplayPtr, queryReturnArr);

			// foreach (byte b in queryReturnArr) {
			// 	if(b != 0)
			// 		Debugger.Break();
			// }

			SetKeyBit(0, (queryReturnArr[8] & 2)   == 2); //Space
			SetKeyBit(1, (queryReturnArr[5] & 2)   == 2); //T
			SetKeyBit(2, (queryReturnArr[5] & 1)   == 1); //H
			SetKeyBit(3, (queryReturnArr[4] & 128) == 128); //S
			SetKeyBit(4, (queryReturnArr[4] & 64)  == 64); //A
			
			SetKeyBit(5, (queryReturnArr[4] & 32) == 32); //Control
			
			Thread.Sleep(5);
		}

		UnbindKeys();

		Xlib.XCloseDisplay(Window.X11DisplayPtr);
	}
}
