using System;
using System.Text;

namespace FileSystem.Boot
{
    /// <summary>
    /// Extended BPB used in Boot Sector. (
    /// <a href = "https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2003/cc781134(v=ws.10)#maximum-sizes-on-an-ntfs-volume" > Microsoft </ a >)
    /// </summary>
    public class ExtendedBPB
    {
        public ulong TotalSectors { get; set; }                 // The partition size in sectors. The total number of sectors on the hard disk. - 8 bytes

        /// <summary>
        /// The cluster that contains the Master File Table or Logical Cluster Number for the File $Mft identifies the location of the MFT by using its logical cluster number.
        /// </summary>
        public ulong MFTClusterNumber { get; set; }             // 8 bytes total
        public ulong MFTMirrClusterNumber { get; set; }         // The cluster that contains a backup of the Master File Table (location of $MftMirr)  - 8 bytes

        /// <summary>
        /// Clusters Per MFT Record. The size of each record. NTFS creates a file record for each file and a folder record for each folder that is created on an NTFS volume.
        /// Files and folders smaller than this size are contained within the MFT. If this number is positive (up to 7F), then it represents clusters per MFT record.
        /// If the number is negative (80 to FF), then the size of the file record is 2 raised to the absolute value of this number.
        /// </summary>
        public uint ClustersPerMFTRecord { get; set; }          // The number of clusters in a File Record Segment. A negative number denotes that the size is 2 to the power of the absolute value. (0xF6 = -10 → 2^10 = 1024) - 1 byte ? DWORD -> 1 byte + 3 unused bytes 
        // 0x41 1 byte Not used by NTFS.

        /// <summary>
        /// The size of each index buffer, which is used to allocate space for directories. If this number is positive (up to 7F), then it represents clusters per MFT record.
        /// If the number is negative (80 to FF), then the size of the file record is 2 raised to the absolute value of this number.
        /// </summary>
        public uint ClustersPerIndexBuffer { get; set; }        // The number of clusters in an Index Buffer. This uses the same algorithm for negative numbers as the "Clusters Per File Record Segment." - 1 byte + 3 unused bytes ?
        // 0x45 3 bytes Not used by NTFS.
        public ulong VolumeSerialNumber { get; set; }           // A unique random number assigned to this partition, to keep things organized - 8 bytes
        public uint CheckSum { get; set; }                      // 0x50 4 bytes not used by NTFS ???

        public ExtendedBPB()
        {
            VolumeSerialNumber = LongRandom(long.MinValue, long.MaxValue, new Random());
        }

        public ExtendedBPB(byte[] data, int offset)
        {
            TotalSectors = BitConverter.ToUInt64(data, offset + 40);                                                    // 0x28
            MFTClusterNumber = BitConverter.ToUInt64(data, offset + 48);                                                // 0x30
            MFTMirrClusterNumber = BitConverter.ToUInt64(data, offset + 56);                                            // 0x38
            ClustersPerMFTRecord = ClusterNumberAccountForNegative(BitConverter.ToUInt32(data, offset + 64));           // 0x40
            ClustersPerIndexBuffer = ClusterNumberAccountForNegative(BitConverter.ToUInt32(data, offset + 68));         // 0x44
            VolumeSerialNumber = BitConverter.ToUInt64(data, offset + 72);                                              // 0x48
            CheckSum = BitConverter.ToUInt32(data, offset + 80);                                                        // 0x50
        }

        public byte[] DataWritter()
        {
            byte[] ebpb = new byte[44];
            Array.Copy(BitConverter.GetBytes(TotalSectors), 0, ebpb, 0, 8);
            Array.Copy(BitConverter.GetBytes(MFTClusterNumber), 0, ebpb, 8, 8);
            Array.Copy(BitConverter.GetBytes(MFTMirrClusterNumber), 0, ebpb, 16, 8);
            Array.Copy(BitConverter.GetBytes(ClustersPerMFTRecord), 0, ebpb, 24, 4);
            Array.Copy(BitConverter.GetBytes(ClustersPerIndexBuffer), 0, ebpb, 28, 4);
            Array.Copy(BitConverter.GetBytes(VolumeSerialNumber), 0, ebpb, 32, 8);
            Array.Copy(BitConverter.GetBytes(VolumeSerialNumber), 0, ebpb, 40, 4);

            return ebpb;
        }

        private static uint ClusterNumberAccountForNegative(uint number)
        {
            int bytes = 0;
            for (int i = 0; i < 4; ++i)
                if (number >= (uint)0xFF << (i * 8))
                    bytes = i + 1;

            uint negativeNumber = 0x80;
            for (int i = 0; i < bytes; ++i)
                negativeNumber <<= 8;

            // Number was positive, return the value
            if ((negativeNumber & number) != negativeNumber)
                return number;

            int newNumber = (int)number;
            for (int i = bytes + 1; i < 4; ++i)
                newNumber |= 0xFF << (i * 8);

            return (uint)Math.Pow(2, -newNumber);
        }
         
        /// <summary>
        /// The solution used here can be found on 
        /// <a href = "https://stackoverflow.com/a/6651661/9917714" > StackOverflow </ a >.         
        /// </summary>
        /// <returns>Random ulang number used as the 'Volume Serial Number'</returns>
        private static ulong LongRandom(long min, long max, Random random)
        {
            byte[] buf = new byte[8];
            random.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (ulong)(Math.Abs(longRand % (max - min)) + min);
        }
        
    }
}
