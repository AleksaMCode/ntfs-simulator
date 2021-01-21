namespace FileSystem.Enums
{
    /// <summary>
    /// <a href = "https://www.writeblocked.org/resources/NTFS_CHEAT_SHEETS.pdf" > Page 3 of pdf. </ a >
    /// </summary>
    public enum FileNamespace : byte  // 'File namespace' or 'Name types'
    {
        /// <summary>
        /// Case sensitive character set that consists of all Unicode characters except for:
        /// \0 (zero character), / (forward slash). 
        /// The : (colon) is valid for NTFS but not for Windows.
        /// </summary>
        POSIX = 0,              // POSIX (unicode, case sensitive)

        /// <summary>
        /// A case insensitive sub set of the POSIX character set that consists of all Unicode characters except for: " * / : math: smaller or greater symbol ? \ |
        /// Note that names cannot end with a . (dot) or ' ' (space).
        /// </summary>
        Win32 = 1,              // FILE_NAME_NTFS (or WINDOWS) - Win32 (unicode, case insensitive)

        /// <summary>
        /// A case insensitive sub set of the WINDOWS character set that consists of all upper case ASCII characters except for:    " * + , / : ; = math: smaller or greater symbol ? \
        /// </summary>
        DOS = 2,                // FILE_NAME_DOS (or DOS) - DOS (8.3 ASCII, case insensitive) | FILE_NAME_DOS (0x02)

        /// <summary>
        /// Both the DOS and WINDOWS names are identical.
        /// Which is the same as the DOS character set, with the exception that lower case is used as well.
        /// </summary>
        Win32andDOS = 3         // Win32 7 DOS (when Win32 fits in DOS space)
    }
}