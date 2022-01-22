using System.IO.Ports;

public class XDController : GCController
{

    /// <summary>
    /// Provides NINTENDO GAMECUBE controller emulation via Arduino.
    /// </summary>
    /// <param name="setting">Setting object</param>
    public XDController(Setting setting) : base(setting.devices.controller.port) {}
    
    /// <summary>
    /// Press RESET button.
    /// </summary>
    public new void Reset()
    {
        Reset(true);
    }
    /// <summary>
    /// Press RESET button.
    /// </summary>
    /// <param name="delay">Delay after press(ms).</param>
    public new void Reset(int delay)
    {
        Reset(delay, true);
    }
}