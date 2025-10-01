using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;

namespace dotnet_sso_wpf_probe
{
    public class TextBoxWriter : TextWriter
    {
        private readonly TextBox _textBox;
        private readonly Dispatcher _dispatcher;

        public TextBoxWriter(TextBox textBox)
        {
            _textBox = textBox;
            _dispatcher = textBox.Dispatcher;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            _dispatcher.Invoke(() =>
            {
                _textBox.AppendText(value.ToString());
                _textBox.ScrollToEnd();
            });
        }

        public override void Write(string? value)
        {
            if (value == null) return;
            _dispatcher.Invoke(() =>
            {
                _textBox.AppendText(value);
                _textBox.ScrollToEnd();
            });
        }

        public override void WriteLine(string? value)
        {
            Write(value + "\r\n");
        }
    }
}