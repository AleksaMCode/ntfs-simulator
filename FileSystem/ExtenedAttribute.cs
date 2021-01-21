using System;
using System.Text;
using FileSystem.Enums;

namespace FileSystem
{
    public class ExtenedAttribute
    {
        /// <summary>
        /// Offset to next extended attribute entry or Size. The offset is relative from the start of the extended attribute data
        /// </summary>
        public int OffsetToNxtAttributeEntry { get; set; }
        public EAFlags Flag { get; set; }
        /// <summary>
        /// Number of characters of the extended attribute name.
        /// </summary>
        public byte NameLength { get; set; }
        public ushort ValueSize { get; set; }
        public string Name { get; set; }
        public byte[] Value { get; set; }


        public static int GetSize(byte[] data, int offset)
        {
            return BitConverter.ToInt32(data, offset);
        }

        public static ExtenedAttribute ReadData(byte[] data, int maxLength, int offset)
        {
            ExtenedAttribute ea = new ExtenedAttribute();

            ea.OffsetToNxtAttributeEntry = BitConverter.ToInt32(data, offset);
            ea.Flag = (EAFlags)data[offset + 4];
            ea.NameLength = data[offset + 5];
            ea.ValueSize = BitConverter.ToUInt16(data, offset + 6);
            ea.Name = Encoding.ASCII.GetString(data, offset + 8, ea.NameLength);
            ea.Value = new byte[ea.ValueSize];
            Array.Copy(data, offset + 8 + ea.NameLength, ea.Value, 0, ea.ValueSize);

            return ea;
        }
    }
}
