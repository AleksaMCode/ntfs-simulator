using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileSystem.Attributes;
using FileSystem.Enums;
using FileSystem.Attributes.Indexroot;

namespace FileSystem
{
    /// <summary>
    /// Header Layout for MFT "FILE" Records
    /// (<a href = "https://www.writeblocked.org/resources/NTFS_CHEAT_SHEETS.pdf" > writeblocked.org NTFS Cheat Sheets </ a >)
    /// </summary>
    public class FileRecordSegmentHeader
    {
        public List<AttributeRecord> attributes = new List<AttributeRecord>();
        //private List<AttributeRecord> externalAttributes;

        public MultiSectorHeader MSH { get; set; }
        public ulong LSN { get; set; }                                  // Metadata transaction journal sequence number. Contains a $LogFile Sequence Number (LSN)
        public ushort SequenceNumber { get; set; }
        public short ReferenceLinkCount { get; set; }                   // Hard Link Count
        public ushort FirstAttributeOffset { get; set; }
        public MFTEntryFlags Flags { get; set; }                        // USHORT Flags;
        public uint UsedEntrySize { get; set; }                         // Used size of file record - Contains the number of bytes of the MFT entry that are in use.
        public uint TotalEntrySize { get; set; }                        // Allocated size of file record - Contains the number of bytes of the MFT entry.
        public MFTSegmentReference BaseFileRecordSegment { get; set; }

        public FileRecordSegmentHeader parent { get; set; }
        public uint UsedFolderSize { get; set; }
        public uint AllocatedFolderSize { get; set; }

        public ushort FirstNextFreeAttributeId { get; set; }            // First available attribute identifier -  USHORT Reserved4;
        public uint MFTNumber { get; set; }                             // MFT Record Number (only since NTFS version 3.1)
        public byte[] UpdateSequenceArray { get; set; }
        public byte[] SequenceArray { get; set; }

        public MFTSegmentReference MftSegmentReference { get; set; }
        
        public void CreateFileRecordHeader(MFTEntryFlags flag, FileRecordSegmentHeader parent)
        {
            MSH = new MultiSectorHeader();
            FirstAttributeOffset = 42;
            Flags = flag;
            if (Flags == MFTEntryFlags.FileRecordSegmentInUse)
            {
                TotalEntrySize = 64_000;
                UsedFolderSize = AllocatedFolderSize = 0;
            }
            else
            {
                UsedEntrySize = TotalEntrySize = 0;
                // UsedEntrySize for folders is size of the Stndfile + FileName file + IndexRoot file
                if (parent == null) // root folder
                    this.parent = null;
                else
                    this.parent = parent;              
            }
            UsedEntrySize = 42;
        }

        public byte[] DataWritter()
        {
            byte[] mftEntryHeader = new byte[42 + ((StandardInformation)attributes.ElementAt(0)).RecordLength + ((FileName)attributes.ElementAt(1)).RecordLength];
            Array.Copy(MSH.DataWritter(), 0, mftEntryHeader, 0, 4);
            Array.Copy(BitConverter.GetBytes(FirstAttributeOffset), 0, mftEntryHeader, 4, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Flags), 0, mftEntryHeader, 6, 2);
            Array.Copy(BitConverter.GetBytes(UsedEntrySize), 0, mftEntryHeader, 8, 4);
            Array.Copy(BitConverter.GetBytes(TotalEntrySize), 0, mftEntryHeader, 12, 4);
            Array.Copy(BitConverter.GetBytes(UsedFolderSize), 0, mftEntryHeader, 16, 4);
            Array.Copy(BitConverter.GetBytes(AllocatedFolderSize), 0, mftEntryHeader, 20, 4);

            Array.Copy(((StandardInformation)attributes.ElementAt(0)).DataWritter(), 0, mftEntryHeader, 24, ((StandardInformation)attributes.ElementAt(0)).RecordLength);
            Array.Copy(((FileName)attributes.ElementAt(1)).DataWritter(), 0, mftEntryHeader, 24, ((FileName)attributes.ElementAt(1)).RecordLength);

            if (attributes.ElementAt(2) is Data && ((Data)attributes.ElementAt(2)).DataBytes != null)
            {
                byte[] tmp = new byte[((Data)attributes.ElementAt(2)).DataBytes.Length];
                Array.Copy(((Data)attributes.ElementAt(2)).DataBytes, 0, tmp, 0, tmp.Length);

                var tmpRet = new byte[mftEntryHeader.Length + tmp.Length];
                mftEntryHeader.CopyTo(tmpRet, 0);
                tmp.CopyTo(tmpRet, mftEntryHeader.Length);
                return tmpRet;
            }

            return mftEntryHeader;
        }


        //public IReadOnlyCollection<AttributeRecord> Attributes
        //{
        //    get
        //    {
        //        return attributes.AsReadOnly();
        //    }
        //}

        /*public IReadOnlyCollection<AttributeRecord> ExternalAttributes
        {
            get
            {
                return externalAttributes.AsReadOnly();
            }
        }*/

        public static uint ReadAllocatedSize(byte[] data, int offset)
        {
            return BitConverter.ToUInt32(data, offset + 28);            // Total Entry Size
        }

        public static FileRecordSegmentHeader ReadFileRecord(byte[] data, int offset, ushort bytesPerSector, uint sectors)
        {
            uint length = bytesPerSector * sectors;
            if (data.Length - offset < length || length < 50 || !(bytesPerSector % 512 == 0 && bytesPerSector > 0) || sectors < 0)
                throw new Exception("Error in parsing the 'FILE_RECORD_SEGMENT_HEADER'!\n");

            FileRecordSegmentHeader record = new FileRecordSegmentHeader();
            //                                                                                                          offset      size
            record.MSH = new MultiSectorHeader(data, offset);                                                           //  0      4+2+2=8
            record.LSN = BitConverter.ToUInt64(data, offset + 8);                                                       //  8         8
            record.SequenceNumber = BitConverter.ToUInt16(data, offset + 16);                                           // 16         2
            record.ReferenceLinkCount = BitConverter.ToInt16(data, offset + 18);                                        // 18         2
            record.FirstAttributeOffset = BitConverter.ToUInt16(data, offset + 20);                                     // 20         2
            record.Flags = (MFTEntryFlags)BitConverter.ToUInt16(data, offset + 22);                                     // 22         2
            record.UsedEntrySize = BitConverter.ToUInt32(data, offset + 24);                                            // 24         4
            record.TotalEntrySize = BitConverter.ToUInt32(data, offset + 32);                                           // 28         4
            record.BaseFileRecordSegment = new MFTSegmentReference(BitConverter.ToUInt64(data, offset + 32));           // 32         8
            record.FirstNextFreeAttributeId = BitConverter.ToUInt16(data, offset + 40);                                 // 40         2
            // Used to align to 4-byte boundary (only since NTFS version 3.1) [offset : 0x2A]                           // 42         2
            record.MFTNumber = BitConverter.ToUInt16(data, offset + 44);                                                // 44         4    

            record.UpdateSequenceArray = new byte[2];
            Array.Copy(data, offset + record.MSH.UpdateSequenceArrayOffset, record.UpdateSequenceArray, 0, 2);

            record.SequenceArray = new byte[record.MSH.SequenceArraySize * 2 - 2];
            Array.Copy(data, offset + record.MSH.UpdateSequenceArrayOffset + 2, record.SequenceArray, 0, record.SequenceArray.Length);

            record.MftSegmentReference = new MFTSegmentReference(record.MFTNumber, record.SequenceNumber);

            NtfsHelper.ApplyUpdateSequenceNumberPatch(data, offset, sectors, bytesPerSector, record.UpdateSequenceArray, record.SequenceArray);

            record.attributes = new List<AttributeRecord>();
            //record.externalAttributes = new List<AttributeRecord>();

            record.ReadAttributes(data, record.UsedEntrySize - record.FirstAttributeOffset, offset + record.FirstAttributeOffset);

            return record;
        }

        private void ReadAttributes(byte[] stream, uint maxLength, int offset)
        {
            int offsetCpy = offset;
            while (true)
            {
                AttributeTypeCode attType = AttributeRecord.GetTypeCode(stream, offsetCpy);
                if (attType == AttributeTypeCode.EndOfAttributes)
                    break;

                uint length = AttributeRecord.GetRecordLength(stream, offsetCpy);

                AttributeRecord attributeRecord = AttributeRecord.ReadSingleAttribute(stream, (int)length, offsetCpy);
                attributeRecord.RecordOwner = MftSegmentReference;
                attributes.Add(attributeRecord);

                offsetCpy += attributeRecord.RecordLength;
            }
        }
    }
}
