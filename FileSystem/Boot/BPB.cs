using System;
using System.Text;
using System.Linq;

namespace FileSystem.Boot
{
    /// <summary>
    /// BIOS Parameter Block (BPB) Structure used in Boot Sector. (
    /// <a href = "https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2003/cc781134(v=ws.10)#maximum-sizes-on-an-ntfs-volume" > Microsoft </ a >)
    /// </summary>
    public class BPB
    {
        public ushort BytesPerSector { get; set; }              // The number of bytes in a disk sector. The size of a hardware sector. For most disks used in the United States, the value of this field is 512. - 2 bytes
        public byte SectorPerCluster { get; set; }              // The number of sectors in a cluster. - 1 byte
        public ushort ReservedSectors { get; set; }             // How much space is reserved by the OS at the start of disk. Value must be 0 or NTFS fails to mount the volume.
        // 0x0E 2 bytes Reserved Sector  00 00
        // 0x10 3 bytes         -        00 00 00
        // 0x13 2 bytes         -        00 00
        // total 7 bytes reserved, filled with 0s

        // Media descriptor entries are a legacy of MS-DOS FAT16 disks
        public byte MediaDescriptor { get; set; }               // The type of drive. 0xF8 is used to denote a hard drive (in contrast to the several sizes of floppy) - 1 byte
        // 0x16 2 bytes Value must be 0 or NTFS fails to mount the volume.

        public ushort SectorsPerTrack { get; set; }             // The number of disk sectors in a drive track. Not used or checked by NTFS. - 2 bytes
        public ushort NumberOfHeads { get; set; }               // The number of heads on the drive. Not used or checked by NTFS. - 2 bytes
        public uint HiddenSectors { get; set; }                 // The number of sectors preceding the partition. Not used or checked by NTFS. - 4 bytes
        // 0x20 4 bytes The value must be 0 or NTFS fails to mount the volume.
        // 0x24 4 bytes Not used or checked by NTFS. ????

        public BPB()
        {
            BytesPerSector = 512;
            SectorPerCluster = 0x01;
            ReservedSectors = 0;
            MediaDescriptor = 0xF8;
        }

        public byte[] DataWritter()
        {
            byte[] bpb = new byte[25];
            Array.Copy(BitConverter.GetBytes(BytesPerSector), 0, bpb, 0, 2);
            Array.Copy(BitConverter.GetBytes(SectorPerCluster), 0, bpb, 2, 1);
            Array.Copy(BitConverter.GetBytes(ReservedSectors), 0, bpb, 3, 2);
            //Array.Copy(Enumerable.Repeat(0x00, 5).ToArray(), 0, bpb, 5, 5);
            Array.Copy(BitConverter.GetBytes(MediaDescriptor), 0, bpb, 10, 1);
            //Array.Copy(Enumerable.Repeat(0x00, 2).ToArray(), 0, bpb, 11, 2);
            //Array.Copy(Enumerable.Repeat(0x00, 4).ToArray(), 0, bpb, 21, 4);                                            // 0x20 4 bytes The value must be 0 or NTFS fails to mount the volume.

            return bpb;
        }

        public BPB(byte[] data, int offset)
        {
            BytesPerSector = BitConverter.ToUInt16(data, offset + 11);                                                  // 0x0B
            SectorPerCluster = data[offset + 13];                                                                       // 0x0D
            ReservedSectors = BitConverter.ToUInt16(data, offset + 14);                                                 // 0x0E
            MediaDescriptor = data[offset + 21];                                                                        // 0x15
            SectorsPerTrack = BitConverter.ToUInt16(data, offset + 24);                                                 // 0x18
            NumberOfHeads = BitConverter.ToUInt16(data, offset + 26);                                                   // 0x1A
            HiddenSectors = BitConverter.ToUInt32(data, offset + 28);                                                   // 0x1C
        }
    }
}
