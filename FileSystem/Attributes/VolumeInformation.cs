using System;
using FileSystem.Enums;

namespace FileSystem.Attributes
{
    /// <summary>
    /// The volume information attribute($VOLUME_INFORMATION) contains the name of the volume.It is stored as a resident MFT attribute.
    /// The volume information data is 12 bytes of size.
    /// The volume information attribute is used in the $Volume metadata file MFT entry.
    /// </summary>
    public class VolumeInformation : AttributeRecord
    {
        /// <summary>
        /// Unknown usage ???
        /// </summary>
        public ulong Reserved { get; set; }
        public byte MajorVersionNumber { get; set; }
        public byte MinorVersionNumber { get; set; }
        public VolumeInformationFlags VolumeFlag { get; set; }

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

            Reserved = BitConverter.ToUInt64(data, offset);
            MajorVersionNumber = data[offset + 8];
            MinorVersionNumber = data[offset + 9];
            VolumeFlag = (VolumeInformationFlags)BitConverter.ToUInt16(data, offset + 10);
        }
    }
}
