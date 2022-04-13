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
	private static KeyCode _KeycodeLeft;
	private static KeyCode _KeycodeMiddle;

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
		else if(bit != 5) {
			if (_KeyState > origState)
				_ToTrigger = _KeyState;

			if ((_KeyState & 0b00011111) == 0 && bit < 5) {
				//Mask out the control key
				TriggerChord((byte)(_ToTrigger & 0b11011111));
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

		// Xlib.XGrabKey(Window.X11DisplayPtr, _KeycodeLeft, MASK, _XWindow, false, GrabMode.Async, GrabMode.Async);
		// Xlib.XGrabKey(Window.X11DisplayPtr, _KeycodeMiddle, MASK, _XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabButton(Window.X11DisplayPtr, Button.LEFT, MASK, _XWindow, false, EventMask.ButtonPressMask | EventMask.ButtonReleaseMask, GrabMode.Async, GrabMode.Async, X11.Window.None, FontCursor.None);
		Xlib.XGrabButton(Window.X11DisplayPtr, Button.RIGHT, MASK, _XWindow, false, EventMask.ButtonPressMask | EventMask.ButtonReleaseMask, GrabMode.Async, GrabMode.Async, X11.Window.None, FontCursor.None);
		
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

		Xlib.XUngrabButton(Window.X11DisplayPtr, Button.LEFT, KeyButtonMask.Mod2Mask, _XWindow);
		Xlib.XUngrabButton(Window.X11DisplayPtr, Button.RIGHT, KeyButtonMask.Mod2Mask, _XWindow);
		// Xlib.XUngrabKey(Window.X11DisplayPtr, _KeycodeLeft, MASK, _XWindow);
		// Xlib.XUngrabKey(Window.X11DisplayPtr, _KeycodeMiddle, MASK, _XWindow);

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

		// _KeycodeLeft = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.KP_Multiply);
		// _KeycodeMiddle = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.KP_Subtract);

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

		X11.Window pointerWindow = new();
		X11.Window pointerChild = new();

		int pointerRootX = 0;
		int pointerRootY = 0;
		int pointerWinX = 0;
		int pointerWinY = 0;

		uint pointerMask = 0;
		
		while (continueRunning) {
			XLibB.QueryKeymap(Window.X11DisplayPtr, queryReturnArr);
			Xlib.XQueryPointer(Window.X11DisplayPtr, _XWindow, ref pointerWindow, ref pointerChild, ref pointerRootX, ref pointerRootY, ref pointerWinX, ref pointerWinY, ref pointerMask);

			// foreach (byte b in queryReturnArr) {
			// 	if(b != 0)
			// 		Debugger.Break();
			// }
			
			// Console.WriteLine($"Left: {(pointerMask & (1 << 8)) != 0} Right: {(pointerMask & (1 << 10)) != 0}");
			
			SetKeyBit(0, (queryReturnArr[8] & 2)   == 2); //Space
			SetKeyBit(1, (queryReturnArr[5] & 2)   == 2); //T
			SetKeyBit(2, (queryReturnArr[5] & 1)   == 1); //H
			SetKeyBit(3, (queryReturnArr[4] & 128) == 128); //S
			SetKeyBit(4, (queryReturnArr[4] & 64)  == 64); //A
			
			SetKeyBit(5, (queryReturnArr[4] & 32) == 32); //Control
			
			SetKeyBit(7, (pointerMask & (1 << 8)) != 0);  //Left
			SetKeyBit(6, (pointerMask & (1 << 10)) != 0); //Right
			
			Thread.Sleep(5);
		}

		UnbindKeys();

		Xlib.XCloseDisplay(Window.X11DisplayPtr);
	}
}
