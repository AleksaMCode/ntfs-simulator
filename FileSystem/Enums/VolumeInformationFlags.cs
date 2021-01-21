namespace FileSystem.Enums
{
    public enum VolumeInformationFlags : ushort
    {
        IsDirty = 0x0001,
        ReSizeJournal = 0x0002, // Re-size journal (LogFile)
        UpgradeOnNextMount = 0x0004,
        MountedOnWindowsNT4 = 0x0008,
        DeleteUSNUnderway = 0x0010,
        RepairObjectIdentifiers = 0x0020,
        ModifiedByChkdsk = 0x8000
    }
}
