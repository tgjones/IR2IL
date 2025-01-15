using System.Runtime.CompilerServices;

namespace IR2IL.Runtime;

// Copied from https://github.com/dotnet/runtime/blob/d548f38d46f5702b0ad1f2b987cecff6d8e95970/src/libraries/System.Private.CoreLib/src/System/Runtime/Intrinsics/Vector32DebugView_1.cs

internal readonly struct Vector32DebugView<T>
    where T : unmanaged
{
    private readonly Vector32<T> _value;

    public Vector32DebugView(Vector32<T> value)
    {
        _value = value;
    }

    public byte[] ByteView
    {
        get
        {
            var items = new byte[Vector32<byte>.Count];
            Unsafe.WriteUnaligned(ref items[0], _value);
            return items;
        }
    }

    public double[] DoubleView
    {
        get
        {
            var items = new double[Vector32<double>.Count];
            Unsafe.WriteUnaligned(ref Unsafe.As<double, byte>(ref items[0]), _value);
            return items;
        }
    }

    public short[] Int16View
    {
        get
        {
            var items = new short[Vector32<short>.Count];
            Unsafe.WriteUnaligned(ref Unsafe.As<short, byte>(ref items[0]), _value);
            return items;
        }
    }

    public int[] Int32View
    {
        get
        {
            var items = new int[Vector32<int>.Count];
            Unsafe.WriteUnaligned(ref Unsafe.As<int, byte>(ref items[0]), _value);
            return items;
        }
    }

    public long[] Int64View
    {
        get
        {
            var items = new long[Vector32<long>.Count];
            Unsafe.WriteUnaligned(ref Unsafe.As<long, byte>(ref items[0]), _value);
            return items;
        }
    }

    public nint[] NIntView
    {
        get
        {
            var items = new nint[Vector32<nint>.Count];
            Unsafe.WriteUnaligned(ref Unsafe.As<nint, byte>(ref items[0]), _value);
            return items;
        }
    }

    public nuint[] NUIntView
    {
        get
        {
            var items = new nuint[Vector32<nuint>.Count];
            Unsafe.WriteUnaligned(ref Unsafe.As<nuint, byte>(ref items[0]), _value);
            return items;
        }
    }

    public sbyte[] SByteView
    {
        get
        {
            var items = new sbyte[Vector32<sbyte>.Count];
            Unsafe.WriteUnaligned(ref Unsafe.As<sbyte, byte>(ref items[0]), _value);
            return items;
        }
    }

    public float[] SingleView
    {
        get
        {
            var items = new float[Vector32<float>.Count];
            Unsafe.WriteUnaligned(ref Unsafe.As<float, byte>(ref items[0]), _value);
            return items;
        }
    }

    public ushort[] UInt16View
    {
        get
        {
            var items = new ushort[Vector32<ushort>.Count];
            Unsafe.WriteUnaligned(ref Unsafe.As<ushort, byte>(ref items[0]), _value);
            return items;
        }
    }

    public uint[] UInt32View
    {
        get
        {
            var items = new uint[Vector32<uint>.Count];
            Unsafe.WriteUnaligned(ref Unsafe.As<uint, byte>(ref items[0]), _value);
            return items;
        }
    }

    public ulong[] UInt64View
    {
        get
        {
            var items = new ulong[Vector32<ulong>.Count];
            Unsafe.WriteUnaligned(ref Unsafe.As<ulong, byte>(ref items[0]), _value);
            return items;
        }
    }
}