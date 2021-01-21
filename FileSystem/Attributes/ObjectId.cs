using System;
using FileSystem.Enums;

namespace FileSystem.Attributes
{
    /// <summary>
    /// The object identifier attribute ($OBJECT_ID) contains distributed link tracker properties. It is stored as a resident MFT attribute.
    /// The object identifier data is either 16 or 64 bytes of size and consists of:
    /// </summary>
    public class ObjectId : AttributeRecord
    {
        public Guid FileId { get; set; }
        public Guid BirthDroidVolumeId { get; set; }
        public Guid BirthDroidFileId { get; set; }
        public Guid BirthDroidDomainId { get; set; }
        // Droid in this context refers to CDomainRelativeObjId ???

        public override AttributeResidentPermition AllowedResidentStates
        {
            get
            {
                return AttributeResidentPermition.Resident; // Resident only!
            }
        }

        /// <summary>
        /// Trying to parse as much as possible, at least 16 bytes, then check for 32 and 48 after, ending with max. of 64
        /// </summary>
        /// <param name="data"></param>
        /// <param name="maxLength"></param>
        /// <param name="offset"></param>
        internal override void ReadAttributeResident(byte[] data, int maxLength, int offset)
        {
            base.ReadAttributeResident(data, maxLength, offset);

            if (maxLength < 16)
                throw new Exception("$OBJECT_ID need at least 16 bytes available!\n");

            byte[] guidByteArray = new byte[16];

            Array.Copy(data, offset, guidByteArray, 0, 16);
            FileId = new Guid(guidByteArray);

            if (maxLength < 32) // parsing only 16 bytes
                return;

            Array.Copy(data, offset + 16, guidByteArray, 0, 16);
            BirthDroidVolumeId = new Guid(guidByteArray);

            if (maxLength < 48) // parsing only 32 bytes
                return;

            Array.Copy(data, offset + 32, guidByteArray, 0, 16);
            BirthDroidFileId = new Guid(guidByteArray);

            if (maxLength < 64) // parsing only 48 bytes
                return;

            Array.Copy(data, offset + 48, guidByteArray, 0, 16);
            BirthDroidDomainId = new Guid(guidByteArray);
        }
    }
}
