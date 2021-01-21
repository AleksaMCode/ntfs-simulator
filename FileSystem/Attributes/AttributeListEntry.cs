using System;
using System.Text;
using FileSystem.Enums; // Used for 'AttributeTypeCode'

namespace FileSystem.Attributes
{
    /// <summary>
    /// The attribute list entry header (ATTRIBUTE_LIST_ENTRY) is 26 bytes of size.
    /// (<a href = "https://docs.microsoft.com/en-us/windows/win32/devnotes/attribute-list-entry" > Microsoft </ a >)
    /// </summary>
    public class AttributeListEntry
    {
        public AttributeTypeCode AttributeTypeCode { get; set; }    // The attribute type code.
        public ushort RecordLength { get; set; }                    // The size of this structure, plus the optional name buffer, in bytes.
       
        /* The size of the optional attribute name, in characters. If a name exists, 
         * this value is nonzero and the structure is followed immediately by a Unicode string 
         * of the specified number of characters.
         */
        public byte AttributeNameLength { get; set; }

        /* The lowest virtual cluster number (VCN) for this attribute. 
         * This member is zero unless the attribute requires multiple file record segments and unless this entry is a 
         * reference to a segment other than the first one. In this case, this value is the lowest VCN that is 
         * described by the referenced segment.
         */
        public byte AttributeNameOffset { get; set; }               // Reserved.

        /// <summary>
        /// The data first VCN is used when the attribute data is stored in multiple MFT entries.
        /// The attribute list contains an attribute list entry for every MFT entry.
        /// </summary>
        public ulong LowestVcn { get; set; }                        // Lowest VCN = Starting VCN - Data First VCN
        public MFTSegmentReference SegmentReference { get; set; }   // This a Base File - The master file table (MFT) segment in which the attribute resides.
        public ushort AttributeId { get; set; }                     // Reserved. An unique identifier to distinguish between attributes that contain segmented data.
        public string AttributeName { get; set; }                   // The start of the optional attribute name. Contains an UTF-16 little-endian without the end-of-string character

        public static AttributeListEntry ReadListEntry(byte[] data, int maxLength, int offset)
        {
            if (maxLength < 26)
                throw new Exception("<<max length>> values is not valid.\n");

            AttributeListEntry attribute = new AttributeListEntry();

            attribute.AttributeTypeCode = (AttributeTypeCode)BitConverter.ToUInt32(data, offset);
            attribute.RecordLength = BitConverter.ToUInt16(data, offset + 4);
            attribute.AttributeNameLength = data[offset + 6];
            attribute.AttributeNameOffset = data[offset + 7];
            attribute.LowestVcn = BitConverter.ToUInt64(data, offset + 8);
            attribute.SegmentReference = new MFTSegmentReference(BitConverter.ToUInt64(data, offset + 16));
            attribute.AttributeId = BitConverter.ToUInt16(data, offset + 24);

            if (maxLength < attribute.AttributeNameOffset + attribute.AttributeNameLength * 2)
                throw new Exception("Error!\n");

            attribute.AttributeName = Encoding.Unicode.GetString(data, offset + attribute.AttributeNameOffset, attribute.AttributeNameOffset * 2);

            return attribute;
        }
    }
}
