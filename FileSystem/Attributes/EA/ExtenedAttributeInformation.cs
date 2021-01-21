using System;
using FileSystem.Enums;

namespace FileSystem.Attributes.EA
{
    /// <summary>
    /// The (HPFS) extended attribute information ($EA_INFORMATION) contains information about the extended attribute ($EA).
    /// The extended attribute information data is 8 bytes of size.
    /// </summary>
    public class ExtenedAttributeInformation : AttributeRecord
    {
        /// <summary>
        /// Size of an extended attribute entry.
        /// </summary>
        public ushort SizeOfEAEntry { get; set; }
        public ushort NumberOfNeedEA { get; set; }
        public uint SizeOfEAData { get; set; }

        public override AttributeResidentPermition AllowedResidentStates
        {
            get
            {
                return AttributeResidentPermition.Resident; // Resident only!
            }
        }

        internal override void ReadAttributeResident(byte[] data, int maxLength, int offset)
        {
            base.ReadAttributeResident(data, maxLength, offset);

            SizeOfEAEntry = BitConverter.ToUInt16(data, offset);
            NumberOfNeedEA = BitConverter.ToUInt16(data, offset + 2);
            SizeOfEAData = BitConverter.ToUInt32(data, offset + 4);
        }
    }
}
