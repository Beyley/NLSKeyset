namespace NLSKeyset; 

public interface INativeKeyGrabber {
	public void Initialize();
	
	public void GrabKeys();
	public void ReleaseKeys();

	public void Poll();
	public bool Key1State();
	public bool Key2State();
	public bool Key3State();
	public bool Key4State();
	public bool Key5State();
	public bool ControlState();

	public bool LeftState();
	public bool RightState();

	public void Dispose();
}