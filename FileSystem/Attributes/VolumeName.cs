using System;
using System.Text;
using FileSystem.Enums;

namespace FileSystem.Attributes
{
    /// <summary>
    /// The volume name attribute ($VOLUME_NAME) contains the name of the volume. It is stored as a resident MFT attribute.
    /// </summary>
    public class VolumeName : AttributeRecord
    {
        /// <summary>
        /// The volume name attribute is used in the $Volume metadata file MFT entry.
        /// Contains an UTF-16 little-endian without an end-of-string character.
        /// </summary>
        public string Name { get; set; }

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

            Name = Encoding.Unicode.GetString(data, offset, (int)ResidentHeader.ValueLength);
        }
    }
}