using System;
using FileSystem.Enums;

namespace FileSystem.Attributes
{
    public class AttributeGeneric : AttributeRecord
    {
        public byte[] data { get; set; }

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

            this.data = new byte[maxLength];
            Array.Copy(data, offset, this.data, 0, maxLength);
        }
    }
}
