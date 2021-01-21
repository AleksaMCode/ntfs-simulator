using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileSystem.Enums; // Used for 'AttributeTypeCode'
using System.Diagnostics;
using FileSystem.Attributes.Indexroot;
using FileSystem.Attributes.EA;

namespace FileSystem.Attributes
{
    /// <summary>
    /// Note that there is no associated header file for this structure. This structure is valid only for version 3 of NTFS volumes. (
    /// <a href = "https://docs.microsoft.com/en-us/windows/win32/devnotes/attribute-record-header" > Microsoft </ a >)
    /// </summary>
    public abstract class AttributeRecord : ISaveObjects // MFT attribute header
    {
        public AttributeTypeCode TypeCode { get; set; } // The attribute type code.

        /// <summary>
        /// Size (or record length)
        /// </summary>
        public ushort RecordLength { get; set; }        // The size of the attribute record, in bytes. This value reflects the required size for the record variant and is always rounded to the nearest quadword boundary.
        public ResidentFileFlag FormCode { get; set; }  // Non-Resdent flag! In my project always set to Resident!
        public byte NameLength { get; set; }            // The size of the optional attribute name, in characters, or 0 if there is no attribute name. The maximum attribute name length is 255 characters.
        public ushort NameOffset { get; set; }          // The offset of the attribute name from the start of the attribute record, in bytes. If the NameLength member is 0, this member is undefined.
        public AttributeFlags Flags { get; set; }

        /// <summary>
        /// Attribute identifier (or instance)
        /// </summary>
        public ushort Instance { get; set; }            // The unique instance for this attribute in the file record.

        public string AttributeName { get; set; }
        public AttributeResidentHeader ResidentHeader { get; set; }
        public MFTSegmentReference RecordOwner { get; set; }
        public abstract AttributeResidentPermition AllowedResidentStates { get; }

        public static AttributeTypeCode GetTypeCode(byte[] data, int offset)
        {
            if (data.Length - offset < 4)
                throw new Exception("Error!\n");

            return (AttributeTypeCode)BitConverter.ToUInt32(data, offset);
        }

        public static ushort GetRecordLength(byte[] data, int offset)
        {
            if (data.Length - offset + 4 < 2)
                throw new Exception("Error!\n");

            return BitConverter.ToUInt16(data, offset + 4);
        }

        private void ReadARHeader(byte[] data, int offset)
        {
            if (data.Length - offset < 16 || (0 > offset && offset > data.Length))
                throw new Exception("Error!\n");

            TypeCode = (AttributeTypeCode)BitConverter.ToUInt32(data, offset);

            if (TypeCode == AttributeTypeCode.EndOfAttributes)
                return;

            RecordLength = BitConverter.ToUInt16(data, offset + 4);
            FormCode = ResidentFileFlag.Resident; /* (ResidentFileFlag)data[offset + 8]; */
            NameLength = data[offset + 9];
            NameOffset = BitConverter.ToUInt16(data, offset + 10);

            if (NameLength == 0)
                AttributeName = string.Empty;
            else
                AttributeName = Encoding.Unicode.GetString(data, offset + NameOffset, NameLength * 2);

            Flags = (AttributeFlags)BitConverter.ToUInt16(data, offset + 12);
            Instance = BitConverter.ToUInt16(data, offset + 14);
        }

        /// <summary>
        /// Checks if the file is Resident. For this project, file can only be Resident.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="maxLength"></param>
        /// <param name="offset"></param>
        internal virtual void ReadAttributeResident(byte[] data, int maxLength, int offset)
        {
            if (FormCode != ResidentFileFlag.Resident || !AllowedResidentStates.HasFlag(AttributeResidentPermition.Resident))
                throw new Exception("File is not Resident!\n");

            Debug.Assert(data.Length - offset >= maxLength);
            Debug.Assert(0 <= offset && offset <= data.Length);
        }

        public static AttributeRecord ReadSingleAttribute(byte[] data, int maxLength, int offset = 0)
        {
            Debug.Assert(data.Length - offset >= maxLength);
            Debug.Assert(0 <= offset && offset <= data.Length);

            AttributeTypeCode TypeCode = GetTypeCode(data, offset);

            if(TypeCode == AttributeTypeCode.EndOfAttributes)
            {
                AttributeRecord tmpAR = new AttributeGeneric();
                tmpAR.ReadARHeader(data, offset);

                return tmpAR;
            }

            AttributeRecord attRecord;

            switch(TypeCode)
            {
                case AttributeTypeCode.STANDARD_INFORMATION:
                    attRecord = new StandardInformation();
                    break;
                case AttributeTypeCode.ATTRIBUTE_LIST:
                    attRecord = new AttributeList();
                    break;
                case AttributeTypeCode.FILE_NAME:
                    attRecord = new FileName();
                    break;
                case AttributeTypeCode.OBJECT_ID:
                    attRecord = new ObjectId();
                    break;
                // To complicated to quickly be implemented. Maybe one day. lol
                // case AttributeTypeCode.SECURITY_DESCRIPTOR:
                //    attRecord = new SecurityDescriptor();
                //    break;
                case AttributeTypeCode.VOLUME_NAME:
                    attRecord = new VolumeName();
                    break;
                case AttributeTypeCode.VOLUME_INFORMATION:
                    attRecord = new VolumeInformation();
                    break;
                case AttributeTypeCode.DATA:
                    attRecord = new Data();
                    break;
                case AttributeTypeCode.INDEX_ROOT:
                    attRecord = new IndexRoot();
                    break;
                // INDEX_ALLOCATION is stored as non resident and this project deals only with resident files
                // case AttributeTypeCode.INDEX_ALLOCATION:
                //    attRecord = new IndexAllocation();
                //    break;
                case AttributeTypeCode.BITMAP:
                    attRecord = new Bitmap();
                    break;
                case AttributeTypeCode.EA_INFORMATION:
                    attRecord = new ExtenedAttributeInformation();
                    break;
                case AttributeTypeCode.EA:
                    attRecord = new ExtenedAttributes();
                    break;
                // PROPERTY_SET needs a pre NTFS 3.0 volume. This is probably obsolete!
                // case AttributeTypeCode.PROPERTY_SET:
                // attRecord = new PropertSet();
                // break;
                case AttributeTypeCode.LOGGED_UTILITY_STREAM:
                    attRecord = new LoggedUtilityStream();
                    break;
                default: // ?? could be a problem
                    attRecord = new AttributeGeneric();
                    break;
            }

            attRecord.ReadARHeader(data, offset);
            if (attRecord.FormCode == ResidentFileFlag.Resident)
            {
                attRecord.ResidentHeader = AttributeResidentHeader.ReadHeader(data, offset + 16);

                int residentBodyOffset = offset + attRecord.ResidentHeader.ValueOffset;
                int length = offset + attRecord.RecordLength - residentBodyOffset;
                attRecord.ReadAttributeResident(data, length, residentBodyOffset);
            }
            else
                throw new Exception("Could not read and process resident flag!\n");

            return attRecord;
        }

        public virtual int GetSaveLength()
        {
            return 41;
        }

        public virtual void Save(byte[] buffer, int offset)
            => throw new NotImplementedException();

    }
}
