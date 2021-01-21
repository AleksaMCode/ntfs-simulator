using System;
using FileSystem.Enums;

namespace FileSystem.Attributes
{
    /// <summary>
    /// Attribute type for storing additional data for the files and directories.
    /// Resident, known to cause problems when non-resident on Windows Vista.
    /// </summary>
    public class LoggedUtilityStream : AttributeRecord
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
