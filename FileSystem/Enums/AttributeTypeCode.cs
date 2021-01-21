namespace FileSystem.Enums
{
    public enum AttributeTypeCode : uint // $AttrDef
    {
                                            // 0x00000000 is Unused
        STANDARD_INFORMATION = 0x10,        // File attributes (such as read-only and archive), time stamps (such as file creation and last modified), and the hard link count.
        ATTRIBUTE_LIST = 0x20,              // A list of attributes that make up the file and the file reference of the MFT file record in which each attribute is located. (Lists the location of all attribute records that do not fit in the MFT record)
        FILE_NAME = 0x30,                   // The name of the file, in Unicode characters, up to 255 Unicode characters.  The short name is the 8.3, case-insensitive name for the file. Additional names, or hard links, required by POSIX can be included as additional file name attributes.
        OBJECT_ID = 0x40,                   // An 16-byte object identifier assigned by the link-tracking service. A volume-unique file identifier. Used by the distributed link tracking service. Not all files have object identifiers.
        SECURITY_DESCRIPTOR = 0x50,         // Describes who owns the file and who can access it. 
        VOLUME_NAME = 0x60,                 // The volume label. Present in the $Volume file.
        VOLUME_INFORMATION = 0x70,          // The volume information. Present in the $Volume file.
        DATA = 0x80,                        // The contents of the file - contains file data. NTFS allows multiple data attributes per file. Each file typically has one unnamed data attribute. A file can also have one or more named data attributes, each using a particular syntax.
        INDEX_ROOT = 0x90,                  // Used to implement filename allocation for large directories.
        INDEX_ALLOCATION = 0xA0,            // Used to implement filename allocation for large directories.
        BITMAP = 0xB0,                      // A bitmap index for a large directory. - This OOS project renders this useless because it uses only resident files!
        /// <summary>
        /// $SYMBOLIC_LINK = 0xC0 - Removed in NTFS version 3.0
        /// </summary>
        REPARSE_POINT = 0xC0,               // The reparse point data. (Introduced in NTFS version 3.0)
        EA_INFORMATION = 0xD0,              // (HPFS) extended attribute information
        EA = 0xE0,                          // (HPFS) extended attribute
        LOGGED_UTILITY_STREAM = 0x100,      // Similar to a data stream, but operations are logged to the NTFS log file just like NTFS metadata changes. This is used by EFS. ( Introduced in NTFS version 3.0)
                                            // First user defined attribute -> value: 0x00001000
        EndOfAttributes = uint.MaxValue     // End of attributes marker -> value: 0xffffffff
    }
}
