using System.Runtime.InteropServices;
using System.Text;

namespace IR2IL.Runtime;

public static class PrintfHelper
{
    public static unsafe int Puts(void* str)
    {
        Console.WriteLine(Marshal.PtrToStringAnsi((IntPtr)str) ?? string.Empty);
        return 1;
    }

    public static int PutChar(int c)
    {
        Console.Write((char)c);
        return c;
    }

    public static int PrintfCore(IntPtr format, object[] args)
    {
        var formatString = Marshal.PtrToStringAnsi(format) ?? string.Empty;
        var output = FormatPrintf(formatString, args);
        Console.Write(output);
        return output.Length;
    }

    private static string FormatPrintf(string format, object[] args)
    {
        var sb = new StringBuilder();
        var argIndex = 0;
        var i = 0;

        while (i < format.Length)
        {
            if (format[i] != '%' || i + 1 >= format.Length)
            {
                sb.Append(format[i++]);
                continue;
            }

            i++; // skip '%'

            // Flags
            var flagsStart = i;
            while (i < format.Length && "-+ #0".Contains(format[i]))
            {
                i++;
            }
            var flags = format[flagsStart..i];

            // Width
            var widthStart = i;
            while (i < format.Length && char.IsAsciiDigit(format[i]))
            {
                i++;
            }
            var width = i > widthStart ? int.Parse(format[widthStart..i]) : 0;

            // Precision
            int precision = -1;
            if (i < format.Length && format[i] == '.')
            {
                i++;
                int precStart = i;
                while (i < format.Length && char.IsAsciiDigit(format[i]))
                    i++;
                precision = i > precStart ? int.Parse(format[precStart..i]) : 0;
            }

            // Length modifiers (l, ll, h, hh, z, t, L)
            while (i < format.Length && "lhzLtq".Contains(format[i]))
            {
                i++;
            }

            if (i >= format.Length)
            {
                break;
            }

            var spec = format[i++];
            var arg = argIndex < args.Length ? args[argIndex++] : null;

            string formatted;
            switch (spec)
            {
                case 'd':
                case 'i':
                    formatted = Convert.ToInt64(arg).ToString();
                    sb.Append(ApplyWidth(formatted, width, flags, zeroPad: true));
                    break;

                case 'u':
                    formatted = unchecked((ulong)Convert.ToInt64(arg)).ToString();
                    sb.Append(ApplyWidth(formatted, width, flags, zeroPad: true));
                    break;

                case 'f':
                case 'F':
                {
                    var val = Convert.ToDouble(arg);
                    formatted = precision >= 0 ? val.ToString("F" + precision) : val.ToString("F6");
                    sb.Append(ApplyWidth(formatted, width, flags, zeroPad: true));
                    break;
                }

                case 'g':
                case 'G':
                {
                    var val = Convert.ToDouble(arg);
                    int sigFigs = precision >= 0 ? (precision == 0 ? 1 : precision) : 6;
                    formatted = val.ToString((spec == 'G' ? "G" : "G") + sigFigs);
                    sb.Append(ApplyWidth(formatted, width, flags, zeroPad: true));
                    break;
                }

                case 'e':
                case 'E':
                    formatted = string.Format(spec == 'e' ? "{0:e}" : "{0:E}", Convert.ToDouble(arg));
                    sb.Append(ApplyWidth(formatted, width, flags, zeroPad: true));
                    break;

                case 'x':
                    formatted = string.Format("{0:x}", unchecked((ulong)Convert.ToInt64(arg)));
                    sb.Append(ApplyWidth(formatted, width, flags, zeroPad: true));
                    break;

                case 'X':
                    formatted = string.Format("{0:X}", unchecked((ulong)Convert.ToInt64(arg)));
                    sb.Append(ApplyWidth(formatted, width, flags, zeroPad: true));
                    break;

                case 's':
                    formatted = arg is IntPtr ptr ? Marshal.PtrToStringAnsi(ptr) ?? string.Empty : Convert.ToString(arg) ?? string.Empty;
                    sb.Append(ApplyWidth(formatted, width, flags, zeroPad: false));
                    break;

                case 'c':
                    sb.Append((char)Convert.ToInt32(arg));
                    break;

                case '%':
                    sb.Append('%');
                    argIndex--;
                    break;

                default:
                    sb.Append('%');
                    sb.Append(spec);
                    break;
            }
        }

        return sb.ToString();
    }

    private static string ApplyWidth(string value, int width, string flags, bool zeroPad)
    {
        if (width <= 0 || value.Length >= width)
            return value;

        if (flags.Contains('-'))
            return value.PadRight(width);

        if (zeroPad && flags.Contains('0'))
            return value.PadLeft(width, '0');

        return value.PadLeft(width);
    }
}
