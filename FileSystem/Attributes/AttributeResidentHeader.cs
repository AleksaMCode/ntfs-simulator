using System;

namespace FileSystem.Attributes
{
    public class AttributeResidentHeader : ISaveObjects
    {
        /// <summary>
        /// Data size (or value length)
        /// </summary>
        public uint ValueLength { get; set; }                   // The size of the attribute value, in bytes.

        /// <summary>
        /// Data offset (or value size)
        /// </summary>
        public ushort ValueOffset { get; set; }                 // The offset to the value from the start of the attribute record, in bytes.

        /*
        /// <summary>
        /// Indexed flag 1 bit + padding which contains an empty byte = total 2 bytes
        /// indexed flag ?
        /// </summary>
        public byte[] Reserved { get; set; }/*= new byte[2];
        */

        public static AttributeResidentHeader ReadHeader(byte[] data, int offset = 0)
        {
            if (data.Length - offset < 6 || offset < 0)
                throw new Exception("Error!\n");

            AttributeResidentHeader resident = new AttributeResidentHeader();

            resident.ValueLength = BitConverter.ToUInt32(data, offset);
            resident.ValueOffset = BitConverter.ToUInt16(data, offset + 4);

            /*resident.Reserved = new byte[2];
            resident.Reserved[2] = data[offset + 7];*/

            return resident;
        }

        public int GetSaveLength()
        {
            return 8;
        }

        public void Save(byte[] stream, int offset)
        {
            if (stream.Length - offset < 8)
                throw new Exception("Error!\n");

            LittleEndianConverter.GetBytes(stream, offset, ValueLength);
            LittleEndianConverter.GetBytes(stream, offset + 4, ValueOffset);
        }
    }
}
