using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    /// <summary>
    /// Note that there is no associated header file for this structure.
    /// The MULTI_SECTOR_HEADER structure always contains the signature "FILE" and a description of the location and size of the update sequence array.
    /// (<a href = "https://docs.microsoft.com/en-us/windows/win32/devnotes/multi-sector-header" > Microsoft </ a >)
    /// </summary>
    public class MultiSectorHeader
    {
        /// <summary>
        /// Value should be "FILE" or "BAAD". 
        /// FILE (or Magic Number = 0x454C4946; or as they appear in a disk editor: "46 49 4C 45")
        /// </summary>
        public string Signature { get; set; }                   // The signature. This value is a convenience to the user.

        /// <summary>
        /// The fix-up values offset. Contains an offset relative from the start of the MFT entry.
        /// </summary>
        public ushort UpdateSequenceArrayOffset { get; set; }   // The offset to the update sequence array, from the start of this structure. The update sequence array must end before the last USHORT value in the first sector.

        /// <summary>
        /// The number of fix-up values.
        /// </summary>
        public ushort SequenceArraySize { get; set; }           // The size of the update sequence array, in bytes.

        public byte[] DataWritter()
        {
            byte[] name = new byte[Signature.Length];
            Array.Copy(Encoding.ASCII.GetBytes(Signature), 0, name, 0, Signature.Length);

            return name;
        }

        public MultiSectorHeader()
            => Signature = "FILE";

        public MultiSectorHeader(byte[] data, int offset)
        {
            Signature = Encoding.ASCII.GetString(data, offset/*+ 0*/, 4);
            if (Signature != "FILE")
                throw new Exception("The MULTI_SECTOR_HEADER structure doesn't contain the signature 'FILE'\n");
            // If NTFS finds a multi-sector item where the multi-sector header does not match the values at the end of the sector, 
            // it marks the item as "BAAD" and fill it with 0-byte values except for a fix-up value at the end of the first sector. 
            // The "BAAD" signature has been seen to be used on Windows NT4 and XP.
            else if (Signature == "BAAD")
                throw new Exception("The 'BAAD' signature indicates a bad MFT entry!\n");

            UpdateSequenceArrayOffset = BitConverter.ToUInt16(data, offset + 4);
            SequenceArraySize = BitConverter.ToUInt16(data, offset + 6);
        }
    }
}
