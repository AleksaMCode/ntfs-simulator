using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSystem.Enums;

namespace FileSystem.Attributes
{
    /// <summary>
    /// The attribute list attribute ($ATTRIBUTE_LIST) is a list of attributes in an MFT entry.
    /// The attributes stored in the list are placeholders for other attributes. Some of these
    /// attributes could not be stored in the MFT entry due to space limitations. The attribute
    /// list attribute can be stored as either a resident (for a small amount of data) and
    /// non-resident MFT attribute.
    /// </summary>
    public class AttributeList : AttributeRecord
    {
        /// <summary>
        /// The attribute list data contains an array of attribute list entries.
        /// </summary>
        public AttributeListEntry[] entries { get; set; }

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

            if (ResidentHeader.ValueLength > maxLength)
                throw new Exception("Error!\n");

            List<AttributeListEntry> entryList = new List<AttributeListEntry>();

            int offsetCpy = offset;
            while(offsetCpy + 26 <= offset + maxLength)
            {
                AttributeListEntry entry = AttributeListEntry.ReadListEntry(data, Math.Min(data.Length - offsetCpy, maxLength), offsetCpy);

                if (entry.AttributeTypeCode == AttributeTypeCode.EndOfAttributes)
                    break;

                entryList.Add(entry);
                offsetCpy += entry.RecordLength;
            }

            entries = entryList.ToArray();
        }
    }
}
