using System;
using System.Text;

namespace FileSystem.Boot
{
    /// <summary>
    /// <a href = "https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-2000-server/cc976796(v=technet.10)" > Microsoft on Boot Sector </ a >
    /// </summary>
    public class BootSector
    {
        public byte[] JMPinstruction { get; set; }              // Causes execution to continue after the data structures in this boot sector - 3 bytes
        public string OEMid { get; set; }                       // This is the magic cookie that indicates this is an NTFS file system - 8 bytes

        public BPB BIOSParameterBlock { get; set; }             // total 25 bytes
        public ExtendedBPB ExtraBPB { get; set; }             // total 44 bytes

        // The executable boot code (or bootstrap code) that starts the operating system.
        public byte[] BootstrapCode { get; set; }               // The code that loads the rest of the operating system. This is pointed to by the first 3 bytes of this sector - 426 bytes

        public byte[] EosMarker { get; set; }                   // End-of-sector Marker - This flag indicates that this is a valid boot sector - 2 bytes

        public BootSector()
        {
            JMPinstruction = new byte[3];                       // Three bytes of Jump instruction (Assembly code) to jump to boot code (mandatory in bootable partition.) -> jmp loc_00000054
            JMPinstruction[0] = 0x54;
            JMPinstruction[1] = JMPinstruction[2] = 0x00;
            BootstrapCode = new byte[426];
            EosMarker = new byte[2];                            // Typical value is 0xAA55 - all values except strings are stored in little endian order
            EosMarker[0] = 0x55;
            EosMarker[1] = 0xAA;
            OEMid = "NTFS    ";                                 // Word "NTFS" followed by four trailing spaces (0x20) -> 46 54 46 53 20 20 20 20
        }

        public byte[] DataWritter()
        {
            byte[] bootSector = new byte[512];
            Array.Copy(JMPinstruction, 0, bootSector, 0, 3);
            Array.Copy(Encoding.ASCII.GetBytes(OEMid), 0, bootSector, 3, 8);

            BIOSParameterBlock =  new BPB();
            Array.Copy(BIOSParameterBlock.DataWritter(), 0, bootSector, 11, 25);

            ExtraBPB = new ExtendedBPB();
            byte[] tmp = ExtraBPB.DataWritter();
            Array.Copy(tmp, 0, bootSector, 40, tmp.Length);

            Array.Copy(BootstrapCode, 0, bootSector, 84, 426);
            Array.Copy(EosMarker, 0, bootSector, 510, 2);

            return bootSector;
        }

        public static BootSector DataReader(byte[] data, int maxLength, int offset)
        {
            if (offset < 0 || offset > data.Length)
                throw new Exception("offset values is not valid, it should be offset∈[0,data lenght]\n");

            if (data.Length - offset < 512) // 0x01FE = 510 + 2 bytes for eosMarker = 512
                throw new Exception("Length of data is not valid, it should be >= 512 bytes\n");

            BootSector bsector = new BootSector();
            //                                                                                                                byte offset
            Array.Copy(data, offset, bsector.JMPinstruction, 0, 3);                                                             // 0x00
            bsector.OEMid = Encoding.ASCII.GetString(data, offset + 3, 8);                                                      // 0x03
            if (!bsector.OEMid.Equals("NTFS    "))
                throw new Exception("FS is not NTFS!\n");

            bsector.BIOSParameterBlock = new BPB(data, offset);                                                                 // 0x0B - 0x1C
            bsector.ExtraBPB = new ExtendedBPB(data, offset);                                                                 // 0x28 - 0x50

            Array.Copy(data, offset + 84, bsector.BootstrapCode, 0, 426);                                                       // 0x54
            Array.Copy(data, offset + 84 + 426, bsector.EosMarker, 0, 2);                                                       // 0x01FE

            if (bsector.EosMarker[0] != 0x55 || bsector.EosMarker[1] != 0xAA)
                throw new Exception("End-of-sector Marker has wrong values! Flag should be 0xAA55.\n");

            return bsector;
        }
    }
}
