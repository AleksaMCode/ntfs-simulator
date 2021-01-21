using System;
using System.Collections.Generic;
using FileSystem.Enums;

namespace FileSystem.Attributes.Indexroot
{
    public class IndexNodeHeader
    {
        /// <summary>
        /// The offset is relative from the start of the index node header
        /// </summary>
        public uint IndexValueoffset { get; set; }
        public uint IndexNodeSize { get; set; }
        public uint AllocatedIndexNodeSize { get; set; }
        public IndexNodeFlags Flag { get; set; }

        public IndexNodeHeader(byte[] data, int offset)
        {
            IndexValueoffset = BitConverter.ToUInt32(data, offset + 16);
            IndexNodeSize = BitConverter.ToUInt32(data, offset + 20);
            AllocatedIndexNodeSize = BitConverter.ToUInt32(data, offset + 24);
            Flag = (IndexNodeFlags)data[offset + 28];
        }
    }
}
