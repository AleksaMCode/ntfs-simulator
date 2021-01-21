using System;
using System.IO; // Using FileAttributes
using FileSystem.Enums;

namespace FileSystem.Attributes
{
    /// <summary>
    /// This attribute is numbered with the MFT record 10 00 00 00. It contains the dates and time attributes for the file along with DOS attributes that describe the file.
    /// Note that there is no associated header file for this structure. (
    /// <a href = "https://docs.microsoft.com/en-us/windows/win32/devnotes/standard-information" > Microsoft </ a > &
    /// <a href = "https://www.blackbagtech.com/blog/2017/05/02/master-file-table-basics/" > blackbagtech.com </ a >)
    /// </summary>
    public class StandardInformation : AttributeRecord
    {
        public DateTime CreationTime { get; set; }
        public DateTime ModifiedTime { get; set; }          // Last modification date and time (also referred to as last written date and time)
        public DateTime MFTChangeTime { get; set; }          // MFT entry last modification date and time
        public DateTime AccessTime { get; set; }
        public FileAttributes DosPermissions { get; set; }
        public uint MaxNumberVersions { get; set; }
        public uint VersionNumber { get; set; }
        public uint ClassId { get; set; }

        // for NTFS 3.0+ version
        public uint OwnerId { get; set; }                   // The identifier of the file owner, from the security index.

        /// <summary>
        /// Contains the entry number in the security ID index ($Secure:$SII)
        /// </summary>
        public uint SecurityId { get; set; }                // The security identifier for the file.
        public ulong QuotaCharge { get; set; }
        public ulong USN { get; set; }                      // Update Sequence Number = USN

        public override AttributeResidentPermition AllowedResidentStates
        {
            get
            {
                return AttributeResidentPermition.Resident; // Resident only!
            }
        }

        public void CreateStandInfoFile(FileAttributes DosPermissions = FileAttributes.Normal)
        {
            CreationTime = ModifiedTime = AccessTime = MFTChangeTime = DateTime.Now;
            this.DosPermissions = DosPermissions;
            TypeCode = AttributeTypeCode.STANDARD_INFORMATION;
            FormCode = ResidentFileFlag.Resident;
            NameLength = 0;

            RecordLength = 41;
        }

        public byte[] DataWritter()
        {
            byte[] dataStream = new byte[RecordLength];
            Array.Copy(BitConverter.GetBytes((uint)TypeCode), 0, dataStream, 0, 4);
            Array.Copy(BitConverter.GetBytes(RecordLength), 0, dataStream, 4, 2);
            dataStream[6] = (byte)FormCode;
            dataStream[7] = NameLength;

            NtfsHelper.ToWinFileTime(dataStream, 8, CreationTime);
            NtfsHelper.ToWinFileTime(dataStream, 16, ModifiedTime);
            NtfsHelper.ToWinFileTime(dataStream, 24, AccessTime);
            NtfsHelper.ToWinFileTime(dataStream, 32, MFTChangeTime);
            dataStream[40] = (byte)DosPermissions;

            return dataStream;
        }

        internal override void ReadAttributeResident(byte[] data, int maxLength, int offset)
        {
            base.ReadAttributeResident(data, maxLength, offset);

            if (maxLength < 48)
                throw new Exception("STANDARD_INFORMATION : max length of data stream is < 48!\n");

            //                                                                                      position
            CreationTime = NtfsHelper.FromWinFileTime(data, offset);                            //     80
            ModifiedTime = NtfsHelper.FromWinFileTime(data, offset + 8);                        //     88
            MFTChangeTime = NtfsHelper.FromWinFileTime(data, offset + 16);                      //     96
            AccessTime = NtfsHelper.FromWinFileTime(data, offset + 24);                         //    104
            DosPermissions = (FileAttributes)BitConverter.ToInt32(data, offset + 32);           //    112

            MaxNumberVersions = BitConverter.ToUInt32(data, offset + 36);                       //    116
            VersionNumber = BitConverter.ToUInt32(data, offset + 40);                           //    120
            ClassId = BitConverter.ToUInt32(data, offset + 44);                                 //    124

            // for NTFS 3.0+ version
            if (FormCode == ResidentFileFlag.Resident && ResidentHeader.ValueLength >= 72)
            {
                if (maxLength < 72)
                    throw new Exception("Max length of a byte stream is less than 72!\n");

                OwnerId = BitConverter.ToUInt32(data, offset + 48);                             //    128
                SecurityId = BitConverter.ToUInt32(data, offset + 52);                          //    132
                QuotaCharge = BitConverter.ToUInt64(data, offset + 56);                         //    136
                USN = BitConverter.ToUInt64(data, offset + 64);                                 //    144
            }
        }

        public override int GetSaveLength()
        {
            if (OwnerId != 0 || SecurityId != 0 || QuotaCharge != 0 || USN != 0)
            {
                return base.GetSaveLength() + 72;
            }

            return base.GetSaveLength() + 48;
        }

        public override void Save(byte[] buffer, int offset)
        {
            base.Save(buffer, offset);

            LittleEndianConverter.GetBytes(buffer, offset, CreationTime);
            LittleEndianConverter.GetBytes(buffer, offset + 8, ModifiedTime);
            LittleEndianConverter.GetBytes(buffer, offset + 16, MFTChangeTime);
            LittleEndianConverter.GetBytes(buffer, offset + 24, AccessTime);
            LittleEndianConverter.GetBytes(buffer, offset + 32, (int)DosPermissions);

            LittleEndianConverter.GetBytes(buffer, offset + 36, MaxNumberVersions);
            LittleEndianConverter.GetBytes(buffer, offset + 40, VersionNumber);
            LittleEndianConverter.GetBytes(buffer, offset + 44, ClassId);

            if (OwnerId != 0 || SecurityId != 0 || QuotaCharge != 0 || USN != 0)
            {
                LittleEndianConverter.GetBytes(buffer, offset + 48, OwnerId);
                LittleEndianConverter.GetBytes(buffer, offset + 52, SecurityId);
                LittleEndianConverter.GetBytes(buffer, offset + 56, QuotaCharge);
                LittleEndianConverter.GetBytes(buffer, offset + 64, USN);
            }
        }


    }
}