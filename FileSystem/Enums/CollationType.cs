namespace FileSystem.Enums
{
    public enum CollationType : uint
    {
        /// <summary>
        /// The first byte is most significant
        /// </summary>
        CollationBinary = 0x00000000,
        /// <summary>
        ///  Unicode strings case-insensitive
        /// </summary>
        CollationFilename = 0x00000001,
        /// <summary>
        /// Unicode strings case-sensitive; Upper case letters should come first
        /// </summary>
        CollationUnicodeString = 0x00000002,
        /// <summary>
        /// Unsigned 32-bit little-endian integer
        /// </summary>
        CollationNtofsUlong = 0x00000010,
        /// <summary>
        /// NT security identifier (SID)
        /// </summary>
        CollationNtofsSid = 0x00000011,
        /// <summary>
        /// Security hash first, then NT security identifier
        /// </summary>
        CollationNtofsSecurityHash = 0x00000012,
        /// <summary>
        /// An array of unsigned 32-bit little-endian integer values
        /// </summary>
        CollationNtofsUlongs = 0x00000013
    }
}
