using System.Runtime.InteropServices;

namespace Myre
{
    /// <summary>
    /// Provides a safe way of bitwise converting a Single to a Uint32
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct SingleUIntUnion
    {
        /// <summary>
        /// The value of this union, interpreted as a single
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public float SingleValue;

        /// <summary>
        /// The value of this union, interpreted as a uint
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public uint UIntValue;
    }

    /// <summary>
    /// Provides a safe way of bitwise converting a Single to a Uint32
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct SingleIntUnion
    {
        /// <summary>
        /// The value of this union, interpreted as a single
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public float SingleValue;

        /// <summary>
        /// The value of this union, interpreted as a uint
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public int IntValue;
    }

    /// <summary>
    /// Provides a safe way of bitwise converting a Double to a ULong
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct DoubleULongUnion
    {
        /// <summary>
        /// The value of this union, interpreted as a double
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public double DoubleValue;

        /// <summary>
        /// The value of this union, interpreted as a ulong
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public ulong ULongValue;
    }

    /// <summary>
    /// Provides a safe way of bitwise converting a Long to a ULong
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct LongULongUnion
    {
        /// <summary>
        /// The value of this union, interpreted as a long
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public long LongValue;

        /// <summary>
        /// The value of this union, interpreted as a ulong
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public ulong ULongValue;
    }

    /// <summary>
    /// Provides a safe way to convert an int32 to 4 bytes
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct LongUInt2Union
    {
        /// <summary>
        /// The value of the first 32 bits of the union
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public uint IntValue1;

        /// <summary>
        /// The last 32 bits of this union
        /// </summary>
        [FieldOffset(sizeof(int))]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public uint IntValue2;

        /// <summary>
        /// The complete 64 bits of this union
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public long LongValue;
    }

    /// <summary>
    /// Provides a safe way of converting an Int32 to a UInt32
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct IntUIntUnion
    {
        /// <summary>
        /// The value of this union, interpreted as a int
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public int IntValue;

        /// <summary>
        /// The value of this union, interpreted as a uint
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public uint UIntValue;
    }

    /// <summary>
    /// Provides a safe way of converting a byte to an sbyte
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ByteSbyteUnion
    {
        /// <summary>
        /// The value of this union, interpreted as a sbyte
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public sbyte SbyteValue;

        /// <summary>
        /// The value of this union, interpreted as a byte
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public byte ByteValue;
    }

    /// <summary>
    /// Provides a safe way of converting an decimal to 2 ulongs
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct DecimalUlong2Union
    {
        /// <summary>
        /// The value of this union, interpreted as a decimal
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public decimal DecimalValue;

        /// <summary>
        /// The value of the first 64 bits of this union, interpreted as a ulong
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public ulong UlongValue1;

        /// <summary>
        /// The value of the last 64 bits of this union, interpreted as a ulong
        /// </summary>
        [FieldOffset(sizeof(ulong))]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public ulong UlongValue2;
    }

    /// <summary>
    /// Provides a safe way to convert an int32 to 2 shorts
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct IntShort2Union
    {
        /// <summary>
        /// The value of this union, interpreted as a int
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public int IntValue;

        /// <summary>
        /// The value of the first 16 bits of this union, interpreted as a short
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public short ShortValue1;

        /// <summary>
        /// The value of the last 16 bits of this union, interpreted as a short
        /// </summary>
        [FieldOffset(sizeof(ushort))]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public short ShortValue2;
    }

    /// <summary>
    /// Provides a safe way to convert an int32 to 4 bytes
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct IntByte4Union
    {
        /// <summary>
        /// The value of this union, interpreted as a int
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public int IntValue;

        /// <summary>
        /// The first 8 bits of this union
        /// </summary>
        [FieldOffset(0)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public byte ByteValue1;

        /// <summary>
        /// The second 8 bits of this union
        /// </summary>
        [FieldOffset(1)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public byte ByteValue2;

        /// <summary>
        /// The third 8 bits of this union
        /// </summary>
        [FieldOffset(2)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public byte ByteValue3;

        /// <summary>
        /// The fourth 8 bits of this union
        /// </summary>
        [FieldOffset(3)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public byte ByteValue4;
    }
}
