using System.Text;

namespace VsaTemplate.Pipeline.Tests.TestInfrastructure.Services;

// writes every char separately, like a buffered stream splitting a long write,
// so concurrent writes interleave unless the caller synchronizes them
public sealed class CharByCharTextWriter : TextWriter
{
    private readonly StringBuilder _output = new();
    private readonly Lock _lock = new();

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        lock (_lock)
        {
            _output.Append(value);
        }

        Thread.Yield();
    }

    public override string ToString()
    {
        lock (_lock)
        {
            return _output.ToString();
        }
    }
}
