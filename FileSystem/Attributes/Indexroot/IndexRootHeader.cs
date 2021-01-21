using System;
using System.Collections.Generic;
using FileSystem.Enums;

namespace FileSystem.Attributes.Indexroot
{
    public class IndexRootHeader
    {
        /// <summary>
        ///  Contains the type of the indexed attribute or 0 if none
        /// </summary>
        public AttributeTypeCode IndexedAttributeType { get; set; }
        /// <summary>
        /// Contains a value to indicate the ordering of the index entries.
        /// </summary>
        public CollationType CollationRule { get; set; }
        public uint IndexEntrySize { get; set; }
        public byte IndexEntryNumbOfClusterBlocks { get; set; } // Number of cluster per Index Entry | should this be uint instead of byte ??

        public IndexRootHeader(byte[] data, int offset)
        {
            IndexedAttributeType = (AttributeTypeCode)BitConverter.ToUInt32(data, offset);
            CollationRule = (CollationType)BitConverter.ToUInt32(data, offset + 4);
            IndexEntrySize = BitConverter.ToUInt32(data, offset + 8);
            IndexEntryNumbOfClusterBlocks = data[offset + 12];
        }
    }
}
