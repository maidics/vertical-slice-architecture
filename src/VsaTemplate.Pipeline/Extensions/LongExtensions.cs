using System.Globalization;

namespace VsaTemplate.Pipeline.Extensions;

public static class LongExtensions
{
    private static readonly (string Unit, double Bytes)[] _sizeUnits =
    {
        ("B", 1),
        ("KB", 1024d),
        ("MB", 1024d * 1024),
        ("GB", 1024d * 1024 * 1024),
    };

    extension(long bytes)
    {
        public string ToReadableBytes()
        {
            // cast before Abs so long.MinValue doesn't overflow
            double abs = Math.Abs((double)bytes);

            int i = _sizeUnits.Length - 1;
            while (i > 0 && abs < _sizeUnits[i].Bytes)
                i--;

            string sign = bytes < 0 ? "-" : "";
            return string.Create(
                CultureInfo.InvariantCulture,
                $"{sign}{abs / _sizeUnits[i].Bytes:0.##} {_sizeUnits[i].Unit}"
            );
        }
    }
}
