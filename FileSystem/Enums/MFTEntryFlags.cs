using System;

namespace FileSystem.Enums
{
    [Flags]
    public enum MFTEntryFlags : ushort
    {
        FileRecordSegmentInUse = 1,     // 0x0001
        FileNameIndexPresent = 2        // 0x0002 | Has file name (or $I30) index - When this flag is present the file entry is a directory (or contains sub file entries)
                                        // 0x0004 | Unknown (set for $ObjId, $Quota, $Reparse, $UsnJrnl)
                                        // 0x0008 | Unknown (set for $ObjId, $Quota, $Reparse. $Secure)
    }
}
