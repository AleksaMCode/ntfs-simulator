namespace FileSystem.Enums
{
    public enum AttributeFlags : ushort
    {
        CompressionMask = 0x0001,   // ATTRIBUTE_FLAG_COMPRESSION_MASK (0x00FF)
        Encrypted = 0x4000,         // ATTRIBUTE_FLAG_ENCRYPTED (0x4000)
        Sparse = 0x8000             // ATTRIBUTE_FLAG_SPARSE (0x8000)
    }
}
