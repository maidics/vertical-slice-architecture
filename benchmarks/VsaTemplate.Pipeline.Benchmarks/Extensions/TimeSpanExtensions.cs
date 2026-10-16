using System.Globalization;

namespace VsaTemplate.Pipeline.Benchmarks.Extensions;

public static class TimeSpanExtensions
{
    private static readonly (string Unit, double Nanoseconds)[] _units =
    {
        ("ns", 1),
        ("µs", 1e3),
        ("ms", 1e6),
        ("s", 1e9),
        ("min", 60e9),
    };

    extension(TimeSpan timeSpan)
    {
        public string ToReadable()
        {
            double ns = Math.Abs(timeSpan.Ticks * 100.0);

            int i = _units.Length - 1;
            while (i > 0 && ns < _units[i].Nanoseconds)
                i--;

            string sign = timeSpan < TimeSpan.Zero ? "-" : "";
            return string.Create(
                CultureInfo.InvariantCulture,
                $"{sign}{ns / _units[i].Nanoseconds:0.##} {_units[i].Unit}"
            );
        }
    }
}
