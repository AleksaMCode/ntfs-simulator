using System;
using System.Collections;
using FileSystem.Enums;

namespace FileSystem.Attributes
{
    /// <summary>
    /// The bitmap attribute ($BITMAP) contains the allocation bitmap. It can be stored as either a resident (for a small amount of data) and non-resident MFT attribute. 
    /// It is known to be used in: 
    /// the MFT(nameless), where an allocation element represents a MFT entry, and
    /// indexes($I##), where an allocation element represents an index entry.
    /// </summary>
    public class Bitmap : AttributeRecord
    {
        /// <summary>
        /// It is used to maintain information about which entry is used and which is not. Every bit in the bitmap represents an entry. The index is stored byte-wise with the 
        /// LSB of the byte corresponds to the first allocation element. The allocation element is allocated if the corresponding bit contains 1 or unallocated if 0.
        /// </summary>
        public BitArray BitmapField { get; set; }

        public override AttributeResidentPermition AllowedResidentStates
        {
            get
            {
                return AttributeResidentPermition.Resident; // Non-Resident possible, project allows everything to be Resident.
            }
        }

        internal override void ReadAttributeResident(byte[] data, int maxLength, int offset)
        {
            base.ReadAttributeResident(data, maxLength, offset);

            byte[] tmp = new byte[maxLength];
            Array.Copy(data, offset, tmp, 0, maxLength);
            BitmapField = new BitArray(tmp);
        }
    }
}
