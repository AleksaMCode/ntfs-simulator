using System;
using FileSystem.Enums;

namespace FileSystem.Attributes
{
    /// <summary>
    /// The data stream attribute ($DATA) contains the file data. It can be stored as either a resident (for a small amount of data) and non-resident MFT attribute.
    /// This project only stores resident files!
    /// </summary>
    public class Data : AttributeRecord
    {
        public byte[] DataBytes = null; // DataBytes has all the data of entry because its always Resident.

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

            DataBytes = new byte[ResidentHeader.ValueLength];
            Array.Copy(data, offset, DataBytes, 0, DataBytes.Length);
        }
    }
}