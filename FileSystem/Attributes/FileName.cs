using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FileSystem.Enums;

namespace FileSystem.Attributes
{
    /// <summary>
    /// This attribute is identified as 30 00 00 00, and contains the name of the file, the date and time the file was named,
    /// and the physical and allocated size of the file. This structure definition is valid only for major version 3.
    /// (<a href = "https://docs.microsoft.com/en-us/windows/win32/devnotes/file-name" > Microsoft </ a > &
    /// <a href = "https://www.blackbagtech.com/blog/2017/05/02/master-file-table-basics/" > blackbagtech.com </ a >)
    /// The file name attribute ($FILE_NAME) contains the basic file system information, like the parent file entry, MAC times and filename.
    /// It is stored as a resident MFT attribute.
    /// </summary>
    public class FileName : AttributeRecord
    {
        public MFTSegmentReference ParentDirectory { get; set; }    // A file reference to the directory that indexes to this name.
        
        // UCHAR = byte Reserved[0x38]
        public DateTime CreationTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime MFTChangeTime { get; set; }
        public DateTime AccessTime { get; set; }
        public ulong AllocatedSize { get; set; }
        public ulong RealSize { get; set; }

        public FileAttributes FileFlags { get; set; }

        /// <summary>
        /// The extended data contains:
        /// 1. the reparse point tag(see section Reparse point tag) if the reparse point file attribute flag(FILE_ATTRIBUTE_REPARSE_POINT) is set
        /// 2. the extended attribute data size.        
        /// </summary>
        public uint EASandReaparse { get; set; }                    // EAS & Reparse

        public byte FilenameLength { get; set; }                    // The length of the file name, in Unicode characters. (contains the number of characters with-out the end-of-string character)
        public FileNamespace FilenameNamespace { get; set; }

        /// <summary>
        ///  Long to short name conversion is not present in this project.
        /// </summary>
        public string Filename { get; set; }                        // Name string Contains an UTF-16 little-endian without an end-of-string character.

        public override AttributeResidentPermition AllowedResidentStates
        {
            get
            {
                return AttributeResidentPermition.Resident; // Resident only!
            }
        }

        public void CreateFileNameFile(string name, FileAttributes fatt = FileAttributes.Normal)
        {
            CreationTime = ModifiedTime = AccessTime = MFTChangeTime = DateTime.Now;
            //if (size <= 64_000)
            //    AllocatedSize = (ulong)size;
            //else
            //    throw new Exception("File to large!\n");

            FileFlags = fatt;
            TypeCode = AttributeTypeCode.FILE_NAME;
            FormCode = ResidentFileFlag.Resident;
            Filename = name;
            FilenameLength = (byte)name.Length;
            FilenameNamespace = FileNamespace.Win32;
            //RealSize = (ulong)size;
            RecordLength = (ushort)(58 + Filename.Length); // size of the FILE_NAME part
        }

        public byte[] DataWritter()
        {
            byte[] dataStream = new byte[RecordLength];
            Array.Copy(BitConverter.GetBytes((uint)TypeCode), 0, dataStream, 0, 4);
            Array.Copy(BitConverter.GetBytes(RecordLength), 0, dataStream, 4, 2);
            dataStream[6] = (byte)FormCode;

            NtfsHelper.ToWinFileTime(dataStream, 7, CreationTime);
            NtfsHelper.ToWinFileTime(dataStream, 15, ModifiedTime);
            NtfsHelper.ToWinFileTime(dataStream, 23, AccessTime);
            NtfsHelper.ToWinFileTime(dataStream, 31, MFTChangeTime);

            Array.Copy(BitConverter.GetBytes(AllocatedSize), 0, dataStream, 39, 8);
            Array.Copy(BitConverter.GetBytes(RealSize), 0, dataStream, 47, 8);
            dataStream[55] = (byte)FileFlags;
            dataStream[56] = FilenameLength;
            dataStream[57] = (byte)FilenameNamespace;
            Array.Copy(Encoding.ASCII.GetBytes(Filename), 0, dataStream, 58, Filename.Length);

            return dataStream;
        }

        internal override void ReadAttributeResident(byte[] data, int maxLength, int offset)
        {
            base.ReadAttributeResident(data, maxLength, offset);

            //                                                                                      position
            ParentDirectory = new MFTSegmentReference(BitConverter.ToUInt64(data, offset));     //    176
            CreationTime = NtfsHelper.FromWinFileTime(data, offset + 8);                        //    184 
            ModifiedTime = NtfsHelper.FromWinFileTime(data, offset + 16);                       //    192
            MFTChangeTime = NtfsHelper.FromWinFileTime(data, offset + 24);                      //    200
            AccessTime = NtfsHelper.FromWinFileTime(data, offset + 32);                         //    208
            AllocatedSize = BitConverter.ToUInt64(data, offset + 40);                           //    216
            RealSize = BitConverter.ToUInt64(data, offset + 48);                                //    224
            FileFlags = (FileAttributes)BitConverter.ToInt32(data, offset + 56);                //    232
            EASandReaparse = BitConverter.ToUInt32(data, offset + 60);                          //    236
            FilenameLength = data[offset + 64];                                                 //    240
            FilenameNamespace = (FileNamespace)data[offset + 65];                               //    241

            if (maxLength < 66 + FilenameLength * 2)
                throw new Exception("Error!\n");

            Filename = Encoding.Unicode.GetString(data, offset + 66, FilenameLength * 2);       //    242
        }

        public override int GetSaveLength()
        {
            return base.GetSaveLength() + 66 + Filename.Length;
        }
    }
}
