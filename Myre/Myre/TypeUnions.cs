using System;
using System.Runtime.InteropServices;

namespace Myre
{
    /// <summary>
    /// Provides a safe way of bitwise converting a Single to a Uint32
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct SingleUIntUnion
    {
        [FieldOffset(0)]
        public float SingleValue;

        [FieldOffset(0)]
        [CLSCompliant(false)]
        public uint UIntValue;
    }

    /// <summary>
    /// Provides a safe way of bitwise converting a Double to a ULong
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct DoubleULongUnion
    {
        [FieldOffset(0)]
        public double DoubleValue;

        [FieldOffset(0)]
        [CLSCompliant(false)]
        public ulong ULongValue;
    }

    /// <summary>
    /// Provides a safe way of bitwise converting a Long to a ULong
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct LongULongUnion
    {
        [FieldOffset(0)]
        public long LongValue;

        [FieldOffset(0)]
        [CLSCompliant(false)]
        public ulong ULongValue;
    }

    /// <summary>
    /// Provides a safe way of converting an Int32 to a UInt32
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct IntUIntUnion
    {
        [FieldOffset(0)]
        public int IntValue;

        [FieldOffset(0)]
        [CLSCompliant(false)]
        public uint UIntValue;
    }
}
