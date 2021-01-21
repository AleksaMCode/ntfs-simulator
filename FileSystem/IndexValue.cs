using System;
using FileSystem.Enums;
using FileSystem.Attributes;

namespace FileSystem
{
    public class IndexValue
    {
        public MFTSegmentReference FileReference { get; set; }
        public ushort IndexValueSize { get; set; }
        public ushort IndexKeyDataSize { get; set; }
        public IndexValueFlags Flag { get; set; }
        public byte[] Stream { get; set; }

        public FileName Child { get; set; }

        public static IndexValue ReadData(byte[] data, int maxLength, int offset)
        {
            IndexValue entry = new IndexValue();

            entry.FileReference = new MFTSegmentReference(BitConverter.ToUInt64(data, offset));
            entry.IndexValueSize = BitConverter.ToUInt16(data, offset + 8);
            entry.IndexKeyDataSize = BitConverter.ToUInt16(data, offset + 10);
            entry.Flag = (IndexValueFlags)data[offset + 12];

            entry.Stream = new byte[entry.IndexKeyDataSize];
            Array.Copy(data, offset + 16, entry.Stream, 0, entry.IndexKeyDataSize);

            if(entry.IndexKeyDataSize > 66)
            {
                entry.Child = new FileName();
                entry.Child.ReadAttributeResident(entry.Stream, entry.IndexKeyDataSize, 0);

                // my "resident header"
                entry.Child.ResidentHeader = new AttributeResidentHeader();
                entry.Child.ResidentHeader.ValueLength = entry.IndexKeyDataSize;
                entry.Child.ResidentHeader.ValueOffset = 0;
            }

            return entry;
        }

        public override string ToString()
        {
            if (Child == null)
                return FileReference.ToString();
            return FileReference + " [" + Child.Filename + "]";
        }
    }
}
