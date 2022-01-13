using System.IO.Ports;

public class GCButton
{
    public string Name { get; set; }
    public string Down { get; set; }
    public string Up { get; set; }
    public GCButton(string name, string down, string up)
    {
        this.Name = name;
        this.Down = down;
        this.Up = up;
    }

    public string ToJson()
    {
        return "{\"Name\":\"" + Name + "\",\"Down\":\"" + Down + "\",\"Up\":\"" + Up + "\"}";
    }

    public static GCButton A { get; } = new GCButton("A", "a", "m");
    public static GCButton B { get; } = new GCButton("B", "b", "n");
    public static GCButton X { get; } = new GCButton("X", "c", "o");
    public static GCButton Y { get; } = new GCButton("Y", "d", "p");
    public static GCButton L { get; } = new GCButton("L", "e", "q");
    public static GCButton R { get; } = new GCButton("R", "f", "r");
    public static GCButton Z { get; } = new GCButton("Z", "g", "s");
    public static GCButton St { get; } = new GCButton("St", "h", "t");
    public static GCButton dL { get; } = new GCButton("dL", "i", "u");
    public static GCButton dR { get; } = new GCButton("dR", "j", "v");
    public static GCButton dD { get; } = new GCButton("dD", "k", "w");
    public static GCButton dU { get; } = new GCButton("dU", "l", "x");
}

public class GCOperation
{
    public GCButton Button { get; set; }
    public int Duration { get; set; }
    public int Delay { get; set; }
    public GCOperation(GCButton button, int duration, int delay)
    {
        this.Button = button;
        this.Duration = duration;
        this.Delay = delay;
    }

    public string ToJson()
    {
        return "{\"Button\":" + Button.ToJson() + ",\"Duration\":" + Duration + ",\"Delay\":" + Delay + "}";
    }

    public static GCOperation PressA { get; } = new GCOperation(GCButton.A, 150, 200);
    public static GCOperation PressB { get; } = new GCOperation(GCButton.B, 150, 200);
    public static GCOperation PressX { get; } = new GCOperation(GCButton.X, 150, 200);
    public static GCOperation PressY { get; } = new GCOperation(GCButton.Y, 150, 200);
    public static GCOperation PressL { get; } = new GCOperation(GCButton.L, 150, 200);
    public static GCOperation PressR { get; } = new GCOperation(GCButton.R, 150, 200);
    public static GCOperation PressZ { get; } = new GCOperation(GCButton.Z, 150, 200);
    public static GCOperation PressSt { get; } = new GCOperation(GCButton.St, 150, 200);
    public static GCOperation PressdL { get; } = new GCOperation(GCButton.dL, 150, 200);
    public static GCOperation PressdR { get; } = new GCOperation(GCButton.dR, 150, 200);
    public static GCOperation PressdD { get; } = new GCOperation(GCButton.dD, 150, 200);
    public static GCOperation PressdU { get; } = new GCOperation(GCButton.dU, 150, 200);
}

public class GCController : IDisposable
{
    private string _port;
    private SerialPort _serialPort;
    private bool _disposed = false;

    /// <summary>
    /// Provides NINTENDO GAMECUBE controller emulation via Arduino.
    /// 
    /// https://github.com/mizuyoukanao/WHALE
    /// https://docs.microsoft.com/ja-jp/windows-server/administration/windows-commands/mode
    /// https://qiita.com/yapg57kon/items/58d7f47022b3e405b5f3
    /// </summary>
    /// <param name="port">A serial port name(e.g. "COM6") of Arduino.</param>
    public GCController(Setting setting)
    {   
        this._port = setting.devices.controller.port;

        this._serialPort = new SerialPort(this._port, 4800, Parity.None);
        this._serialPort.DataBits = 8;
        this._serialPort.StopBits = StopBits.One;
        this._serialPort.DtrEnable = false;
        this._serialPort.RtsEnable = false;
        this._serialPort.Handshake = Handshake.None;
        try
        {
            this._serialPort.Open();
        }
        catch
        {
            throw new Exception("シリアルポートを取得できませんでした。");
        }
    }

    private void Write(string c, int delay)
    {
        try
        {
            if (c != "") 
            {
                _serialPort.WriteLine(c.Substring(0, 1));
            }
        }
        catch
        {
            throw new Exception("シリアルポートを取得できませんでした。");
        }
        Thread.Sleep(delay);
    }

    /// <summary>
    /// Press RESET button.
    /// </summary>
    /// <param name="delay">Delay after press(ms).</param>
    public void Reset(int delay)
    {
        // Write("@", delay);
        InvokeSequence(new GCOperation[]
        {
            new GCOperation(new GCButton("B_Hold", "b", ""), 0, 100),
            new GCOperation(new GCButton("X_Hold", "c", ""), 0, 100),
            new GCOperation(new GCButton("St_Hold", "h", ""), 0, 5000),
            new GCOperation(new GCButton("B_Up", "", "n"), 0, 100),
            new GCOperation(new GCButton("X_Up", "", "o"), 0, 100),
            new GCOperation(new GCButton("St_Up", "", "t"), 0, delay - 5000)
        });
    }

    /// <summary>
    /// Execute the operation.
    /// </summary>
    /// <param name="operation">A GCOperation object.</param>
    public void InvokeOperation(GCOperation operation)
    {
#if DEBUG
        Console.WriteLine("{0}[33mGCController.InvokeOperation({1}){0}[0m", Char.ConvertFromUtf32(27), operation.ToJson());
#endif
        Write(operation.Button.Down, operation.Duration);
        Write(operation.Button.Up, operation.Delay);
    }

    /// <summary>
    /// Execute the sequence.
    /// </summary>
    /// <param name="sequence">An array of GCOperation objects.</param>
    public void InvokeSequence(GCOperation[] sequence)
    {
        foreach(var operation in sequence) InvokeOperation(operation);
    }

    public void Dispose()
    {
        Dispose(true);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _serialPort.Dispose();
            }
            _disposed = true;
        }
    }
}