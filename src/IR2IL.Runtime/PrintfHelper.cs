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
            while (i < format.Length && "-+ #0".Contains(format[i]))
            {
                i++;
            }

            // Width
            while (i < format.Length && char.IsAsciiDigit(format[i]))
            {
                i++;
            }

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

            switch (spec)
            {
                case 'd':
                case 'i':
                    sb.Append(Convert.ToInt64(arg));
                    break;

                case 'u':
                    sb.Append(Convert.ToUInt64(arg));
                    break;

                case 'f':
                case 'F':
                {
                    var val = Convert.ToDouble(arg);
                    sb.Append(precision >= 0 ? val.ToString("F" + precision) : val.ToString("F6"));
                    break;
                }

                case 'g':
                case 'G':
                {
                    var val = Convert.ToDouble(arg);
                    int sigFigs = precision >= 0 ? (precision == 0 ? 1 : precision) : 6;
                    sb.Append(val.ToString((spec == 'G' ? "G" : "G") + sigFigs));
                    break;
                }

                case 'e':
                case 'E':
                    sb.AppendFormat(spec == 'e' ? "{0:e}" : "{0:E}", Convert.ToDouble(arg));
                    break;

                case 'x':
                    sb.AppendFormat("{0:x}", Convert.ToInt64(arg));
                    break;

                case 'X':
                    sb.AppendFormat("{0:X}", Convert.ToInt64(arg));
                    break;

                case 's':
                    sb.Append(arg is IntPtr ptr ? Marshal.PtrToStringAnsi(ptr) : Convert.ToString(arg));
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
}
