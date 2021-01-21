using System;
using System.Collections.Generic;
using FileSystem.Enums;

namespace FileSystem.Attributes.EA
{
    public class ExtenedAttributes : AttributeRecord
    {
        public ExtenedAttribute[] attributes { get; set; }

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

            List<ExtenedAttribute> eaList = new List<ExtenedAttribute>();

            int offsetCpy = offset;
            while (offsetCpy + 8 <= offset + maxLength)
            {
                if (ExtenedAttribute.GetSize(data, offsetCpy) < 0)
                    break;

                ExtenedAttribute attribute = ExtenedAttribute.ReadData(data, (int)ResidentHeader.ValueLength, offsetCpy);
                eaList.Add(attribute);
                offsetCpy += attribute.ValueSize;
            }

            attributes = eaList.ToArray();
        }
    }
}
