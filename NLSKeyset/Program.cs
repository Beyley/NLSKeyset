using System.Diagnostics;
using System.Numerics;
using Desktop.Robot;
using FontStashSharp;
using Silk.NET.Maths;
using Silk.NET.SDL;
using X11;
using Event = Silk.NET.SDL.Event;
using KeyCode = X11.KeyCode;
using Thread = System.Threading.Thread;

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

	private static KeyCode _KeycodeControl;
	private static KeyCode _KeycodeDisable;
	private static KeyCode _KeycodeEnable;

	private static X11.Window _XRootWindow;
	private static Robot      _RobotTyper = new Robot();

	private static char   _LastChord;
	private static double _LastChordTime;

	private static bool GetKeyBit(byte bit) => (_KeyState & (1 << bit)) != 0;

	private static bool _Enabled = true;
	
	// private static bool triggered = false;
	private static byte    _ToTrigger;
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
		Xlib.XFlush(SDLWindow.X11DisplayPtr);
		Xlib.XSync(SDLWindow.X11DisplayPtr, false);
	
		_RobotTyper.KeyPress(key);
		Xlib.XFlush(SDLWindow.X11DisplayPtr);
		Xlib.XSync(SDLWindow.X11DisplayPtr, false);
		Console.WriteLine($"Typing key: {key}");
		
		BindKeys();
		Xlib.XFlush(SDLWindow.X11DisplayPtr);
		Xlib.XSync(SDLWindow.X11DisplayPtr, false);
	}
	
	private static void TriggerChord(byte state) {
		if (state == 0)
			return;

		if (ChordStates.States.TryGetValue(state, out char key)) {
			Console.WriteLine($"Typing chord {state}:{key}");
			TriggerKeyPress(key);
			_LastChord     = key;
			_LastChordTime = (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
		}
		else {
			Console.WriteLine($"Invalid Chord! {state:x2}");
		}
	}

	private const KeyButtonMask MASK        = KeyButtonMask.Mod2Mask;
	private const KeyButtonMask ENABLE_MASK = MASK | KeyButtonMask.ShiftMask;

	private static unsafe void BindKeys() {
		Xlib.XGrabKey(SDLWindow.X11DisplayPtr, _Keycode1, MASK, _XRootWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(SDLWindow.X11DisplayPtr, _Keycode2, MASK, _XRootWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(SDLWindow.X11DisplayPtr, _Keycode3, MASK, _XRootWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(SDLWindow.X11DisplayPtr, _Keycode4, MASK, _XRootWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(SDLWindow.X11DisplayPtr, _Keycode5, MASK, _XRootWindow, false, GrabMode.Async, GrabMode.Async);

		// Xlib.XGrabKey(Window.X11DisplayPtr, _KeycodeLeft, MASK, _XWindow, false, GrabMode.Async, GrabMode.Async);
		// Xlib.XGrabKey(Window.X11DisplayPtr, _KeycodeMiddle, MASK, _XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabButton(SDLWindow.X11DisplayPtr, Button.LEFT, MASK, _XRootWindow, false, EventMask.ButtonPressMask | EventMask.ButtonReleaseMask, GrabMode.Async, GrabMode.Async, X11.Window.None, FontCursor.None);
		Xlib.XGrabButton(SDLWindow.X11DisplayPtr, Button.RIGHT, MASK, _XRootWindow, false, EventMask.ButtonPressMask | EventMask.ButtonReleaseMask, GrabMode.Async, GrabMode.Async, X11.Window.None, FontCursor.None);
		
		Xlib.XGrabKey(SDLWindow.X11DisplayPtr, _KeycodeDisable, MASK, _XRootWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(SDLWindow.X11DisplayPtr, _KeycodeEnable, ENABLE_MASK, _XRootWindow, false, GrabMode.Async, GrabMode.Async);

		bool test;
		XLibB.XkbSetDetectableAutoRepeat(SDLWindow.X11DisplayPtr, true, &test);
		
		Xlib.XFlush(SDLWindow.X11DisplayPtr);
	}

	private static void UnbindKeys() {
		Xlib.XUngrabKey(SDLWindow.X11DisplayPtr, _Keycode1, MASK, _XRootWindow);
		Xlib.XUngrabKey(SDLWindow.X11DisplayPtr, _Keycode2, MASK, _XRootWindow);
		Xlib.XUngrabKey(SDLWindow.X11DisplayPtr, _Keycode3, MASK, _XRootWindow);
		Xlib.XUngrabKey(SDLWindow.X11DisplayPtr, _Keycode4, MASK, _XRootWindow);
		Xlib.XUngrabKey(SDLWindow.X11DisplayPtr, _Keycode5, MASK, _XRootWindow);

		Xlib.XUngrabButton(SDLWindow.X11DisplayPtr, Button.LEFT, KeyButtonMask.Mod2Mask, _XRootWindow);
		Xlib.XUngrabButton(SDLWindow.X11DisplayPtr, Button.RIGHT, KeyButtonMask.Mod2Mask, _XRootWindow);
		// Xlib.XUngrabKey(Window.X11DisplayPtr, _KeycodeLeft, MASK, _XWindow);
		// Xlib.XUngrabKey(Window.X11DisplayPtr, _KeycodeMiddle, MASK, _XWindow);

		Xlib.XUngrabKey(SDLWindow.X11DisplayPtr, _KeycodeDisable, MASK, _XRootWindow);
		Xlib.XUngrabKey(SDLWindow.X11DisplayPtr, _KeycodeEnable, ENABLE_MASK, _XRootWindow);

		Xlib.XFlush(SDLWindow.X11DisplayPtr);
	}

	private static void GetKeycodes() {
		_Keycode5 = Xlib.XKeysymToKeycode(SDLWindow.X11DisplayPtr, (KeySym)KeySyms.a);
		_Keycode4 = Xlib.XKeysymToKeycode(SDLWindow.X11DisplayPtr, (KeySym)KeySyms.s);
		_Keycode3 = Xlib.XKeysymToKeycode(SDLWindow.X11DisplayPtr, (KeySym)KeySyms.h);
		_Keycode2 = Xlib.XKeysymToKeycode(SDLWindow.X11DisplayPtr, (KeySym)KeySyms.t);
		_Keycode1 = Xlib.XKeysymToKeycode(SDLWindow.X11DisplayPtr, (KeySym)KeySyms.space);

		// _KeycodeLeft = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.KP_Multiply);
		// _KeycodeMiddle = Xlib.XKeysymToKeycode(Window.X11DisplayPtr, (KeySym)KeySyms.KP_Subtract);

		_KeycodeDisable = Xlib.XKeysymToKeycode(SDLWindow.X11DisplayPtr, (KeySym)KeySyms.g);
		_KeycodeEnable  = Xlib.XKeysymToKeycode(SDLWindow.X11DisplayPtr, (KeySym)KeySyms.space);
		_KeycodeControl  = Xlib.XKeysymToKeycode(SDLWindow.X11DisplayPtr, (KeySym)KeySyms.Control_L);
	}

	public static bool IsKeyDown(byte[] arr, KeyCode code) => (arr[(byte)code >> 3] & (1 << ((byte)code & 7))) == 1 << ((byte)code & 7);

	public static unsafe void Main(string[] args) {
		if (!OperatingSystem.IsLinux())
			throw new Exception("Only Linux is supported currently!");
		
		// Window.X11DisplayPtr = Xlib.XOpenDisplay(":0");
		SDLWindow.Initialize();
		
		_XRootWindow = Xlib.XDefaultRootWindow(SDLWindow.X11DisplayPtr);

		GetKeycodes();
		BindKeys();
		Xlib.XFlush(SDLWindow.X11DisplayPtr);

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

		SDLFontStashSharpRenderer renderer = new();
		
		FontSystem fontSystem = new(new FontSystemSettings());
		fontSystem.AddFont(File.ReadAllBytes("font.ttf"));
		DynamicSpriteFont? font = fontSystem.GetFont(50);

		Event @event;
		while (continueRunning) {
			XLibB.QueryKeymap(SDLWindow.X11DisplayPtr, queryReturnArr);
			Xlib.XQueryPointer(SDLWindow.X11DisplayPtr, _XRootWindow, ref pointerWindow, ref pointerChild, ref pointerRootX, ref pointerRootY, ref pointerWinX, ref pointerWinY, ref pointerMask);

			SetKeyBit(0, IsKeyDown(queryReturnArr, _Keycode1)); //Space
			SetKeyBit(1, IsKeyDown(queryReturnArr, _Keycode2)); //T
			SetKeyBit(2, IsKeyDown(queryReturnArr, _Keycode3)); //H
			SetKeyBit(3, IsKeyDown(queryReturnArr, _Keycode4)); //S
			SetKeyBit(4, IsKeyDown(queryReturnArr, _Keycode5)); //A
			
			SetKeyBit(5, IsKeyDown(queryReturnArr, _KeycodeControl)); //Control
			
			SetKeyBit(7, (pointerMask & (1 << 8)) != 0);  //Left
			SetKeyBit(6, (pointerMask & (1 << 10)) != 0); //Right

			if(SDLWindow.SDL.PollEvent(&@event) != 0) {
				if(@event.Type == (ulong)EventType.Quit) {
					continueRunning = false;
				}
			}

			SDLWindow.Clear(new(0, 0, 0));
			
			SDLWindow.DrawRect(new(100, 110, 500, 110), Color.White, false);

			SDLWindow.DrawLine(Color.White, new(340, 110), new(340, 0));
			SDLWindow.DrawLine(Color.White, new(360, 110), new(360, 0));
			
			SDLWindow.DrawOctagon(Color.White, new(175, 163));
			SDLWindow.DrawOctagon(Color.White, new(525, 163));

			SDLWindow.DrawRect(new(100, 220, 500, 330), Color.White, false);

			SDLWindow.DrawKey(new(110, 220), GetKeyBit(4), _Enabled);
			SDLWindow.DrawKey(new(210, 220), GetKeyBit(3), _Enabled);
			SDLWindow.DrawKey(new(310, 220), GetKeyBit(2), _Enabled);
			SDLWindow.DrawKey(new(410, 220), GetKeyBit(1), _Enabled);
			SDLWindow.DrawKey(new(510, 220), GetKeyBit(0), _Enabled);
			// SDLWindow.DrawRect(new(110, 250, 80, 300), new Color(255, 255, 255), false);
			// SDLWindow.DrawRect(new(210, 250, 80, 300), new Color(255, 255, 255), false);
			// SDLWindow.DrawRect(new(310, 250, 80, 300), new Color(255, 255, 255), false);
			// SDLWindow.DrawRect(new(410, 250, 80, 300), new Color(255, 255, 255), false);
			// SDLWindow.DrawRect(new(510, 250, 80, 300), new Color(255, 255, 255), false);

			if(((double)Stopwatch.GetTimestamp() / Stopwatch.Frequency) - _LastChordTime < 1d || !_Enabled) {
				string toDraw = _Enabled ? $"{_LastChord}" : "Disabled";

				Vector2 measureString = font.MeasureString(toDraw);
				font.DrawText(renderer, toDraw, new((SDLWindow.WIDTH / 2f) - (measureString.X / 2f), 165 - (measureString.Y / 2f)), System.Drawing.Color.White);
			}
			
			SDLWindow.SDL.RenderPresent(SDLWindow.SDLRendererPtr);

			Thread.Sleep(5);
		}

		UnbindKeys();
		
		SDLWindow.Destroy();
	}
}
