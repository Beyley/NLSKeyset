using System.Diagnostics;
using System.Numerics;
using Desktop.Robot;
using FontStashSharp;
using NLSKeyset.X11;
using Silk.NET.SDL;
using X11;
using Event = Silk.NET.SDL.Event;
using Thread = System.Threading.Thread;

namespace NLSKeyset;

public static class Program {
	private static byte _KeyState;

	private static char   _LastChord;
	private static double _LastChordTime;

	private static          bool  GetKeyBit(byte bit) => (_KeyState & (1 << bit)) != 0;
	private static readonly Robot _RobotTyper = new();

	private static bool _Enabled = true;
	
	// private static bool triggered = false;
	private static byte              _ToTrigger;
	public static  INativeKeyGrabber Grabber;
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
					Grabber.GrabKeys();
				else
					Grabber.ReleaseKeys();
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
		Grabber.ReleaseKeys();
	
		_RobotTyper.KeyPress(key);
		if(OperatingSystem.IsLinux()) {
			Xlib.XFlush(SDLWindow.X11DisplayPtr);
			Xlib.XSync(SDLWindow.X11DisplayPtr, false);
		}
		Console.WriteLine($"Typing key: {key}");
		
		Grabber.GrabKeys();
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


	public static unsafe void Main(string[] args) {
		SDLWindow.Initialize();

		if (OperatingSystem.IsLinux())
			Grabber = new X11KeyGrabber();

		if (Grabber == null)
			throw new Exception("Unable to detect which Grabber to use! Check the source code to see why");
		
		Grabber.Initialize();
		
		Grabber.GrabKeys();

		bool continueRunning = true;

		SDLFontStashSharpRenderer renderer = new();
		
		FontSystem fontSystem = new(new FontSystemSettings());
		fontSystem.AddFont(File.ReadAllBytes("font.ttf"));
		DynamicSpriteFont? font = fontSystem.GetFont(50);

		Event @event;
		while (continueRunning) {
			Grabber.Poll();

			SetKeyBit(0, Grabber.Key1State()); 
			SetKeyBit(1, Grabber.Key2State()); 
			SetKeyBit(2, Grabber.Key3State()); 
			SetKeyBit(3, Grabber.Key4State()); 
			SetKeyBit(4, Grabber.Key5State()); 
			
			SetKeyBit(5, Grabber.ControlState()); //Control
			
			SetKeyBit(7, Grabber.LeftState());  //Left
			SetKeyBit(6, Grabber.RightState()); //Right

			while(SDLWindow.SDL.PollEvent(&@event) != 0) {
				if(@event.Type == (ulong)EventType.Quit) {
					continueRunning = false;
				}
			}

			#region Draw UI
			
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

			if((double)Stopwatch.GetTimestamp() / Stopwatch.Frequency - _LastChordTime < 1d || !_Enabled) {
				string toDraw = _Enabled ? $"{_LastChord}" : "Disabled";

				Vector2 measureString = font.MeasureString(toDraw);
				font.DrawText(renderer, toDraw, new((SDLWindow.WIDTH / 2f) - (measureString.X / 2f), 165 - (measureString.Y / 2f)), System.Drawing.Color.White);
			}
			
			SDLWindow.SDL.RenderPresent(SDLWindow.SDLRendererPtr);

			Thread.Sleep(5);
			
			#endregion
		}

		Grabber.ReleaseKeys();
		
		SDLWindow.Destroy();
	}
}
