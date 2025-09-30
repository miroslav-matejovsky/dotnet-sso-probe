using System.IO;
using System.Text;

namespace dotnet_sso_wpf_probe;


/// <summary>
/// A simple TextWriter implementation that forwards written text to a GUI callback.
/// The MainWindow will assign an action that appends the text to the UI thread.
/// Logs written before the UI is ready are buffered and flushed when the callback is attached.
/// </summary>
public class GuiTextWriter : TextWriter
{
    private readonly object _lock = new();
    private readonly List<string> _buffer = new();
    private Action<string>? _onWrite;

    /// <summary>
    /// If set, this action will be invoked when text is written.
    /// When assigned, any buffered text is flushed to the callback.
    /// </summary>
    public Action<string>? OnWrite
    {
        get => _onWrite;
        set
        {
            lock (_lock)
            {
                _onWrite = value;
                if (_onWrite != null && _buffer.Count > 0)
                {
                    foreach (var s in _buffer)
                    {
                        try { _onWrite(s); } catch { }
                    }
                    _buffer.Clear();
                }
            }
        }
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        Write(value.ToString());
    }

    public override void Write(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        lock (_lock)
        {
            try
            {
                if (_onWrite != null)
                {
                    _onWrite(value);
                }
                else
                {
                    // buffer until a callback is attached
                    _buffer.Add(value);
                }
            }
            catch
            {
                // Swallow UI exceptions to avoid crashing background logging calls
            }
        }
    }

    public override void WriteLine(string? value)
    {
        Write((value ?? string.Empty) + Environment.NewLine);
    }

    public override void Flush()
    {
        // no-op; GUI sink doesn't buffer beyond what TextBox does
    }
}
