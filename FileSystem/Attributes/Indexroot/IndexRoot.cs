using System;
using System.Collections.Generic;
using FileSystem.Enums;

namespace FileSystem.Attributes.Indexroot
{
    /// <summary>
    /// The index root attribute ($INDEX_ROOT) contains the root of the index tree. It is stored as a resident MFT attribute.
    /// The index root consists of: index root header, index node header & an array of index values.
    /// </summary>
    public class IndexRoot : AttributeRecord
    {
        IndexRootHeader TheIndexRootHeader { get; set; }
        IndexNodeHeader TheIndexNodeHeader { get; set; }
        public IndexValue[] ArrayOfIndexValues { get; set; }
        public List<FileRecordSegmentHeader> Children = new List<FileRecordSegmentHeader>();
        public int numberOfChildren = 0;

        public override AttributeResidentPermition AllowedResidentStates
        {
            get
            {
                return AttributeResidentPermition.Resident; // Resident only!
            }
        }

        internal override void ReadAttributeResident(byte[] data, int maxLength, int offset)
        {
            base.ReadAttributeResident(data, maxLength, offset);

            TheIndexRootHeader = new IndexRootHeader(data, offset);
            TheIndexNodeHeader = new IndexNodeHeader(data, offset);

            List<IndexValue> values = new List<IndexValue>();

            int offsetCpy = offset + 32;
            while (offsetCpy <= offset + TheIndexNodeHeader.IndexNodeSize + 32)
            {
                IndexValue value = IndexValue.ReadData(data, (int)TheIndexNodeHeader.IndexNodeSize - (offsetCpy - offset) + 32, offsetCpy);

                if (value.Flag.HasFlag(IndexValueFlags.IsLast))
                    break;

                values.Add(value);
                offsetCpy += value.IndexValueSize;
            }

            ArrayOfIndexValues = values.ToArray();
        }
    }
}
