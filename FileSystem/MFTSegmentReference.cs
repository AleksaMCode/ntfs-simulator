using System;

namespace FileSystem
{
    /// <summary>
    /// Note that there is no associated header file for this structure. <-> MFT_SEGMENT_REFERENCE or FILE_REFERENCE (
    /// <a href = "https://docs.microsoft.com/en-us/windows/win32/devnotes/mft-segment-reference" > Microsoft </ a >)
    /// About IEquatable vs Object.Equals() -  <a href = "https://stackoverflow.com/questions/2734914/whats-the-difference-between-iequatable-and-just-overriding-object-equals" > Stackoverflow </ a >
    /// About IComparable: <a href = "https://docs.microsoft.com/en-us/dotnet/api/system.icomparable-1?view=netframework-4.8" > Microsoft </ a >
    /// More about IEquatable and IComparable - <a href = "https://www.c-sharpcorner.com/UploadFile/80ae1e/icomparable-icomparer-and-iequatable-interfaces-in-C-Sharp/" > c-sharpcorner.com </ a >
    /// </summary>
    public class MFTSegmentReference : IEquatable<MFTSegmentReference>, IComparable<MFTSegmentReference>
    {
        public ulong SegmentNumberLowPart { get; set; }                             // The low part of the segment number.
        public uint SegmentNumberHighPart { get; set; }                             // The high part of the segment number.
        public ushort SequenceNumber { get; set; }                                  // The nonzero sequence number. The value 0 is reserved.

        public MFTSegmentReference(ulong segmentNumberLowPart)
        {
            SequenceNumber = (ushort)(segmentNumberLowPart >> 48);                  // Get the high-order 16 bites
            SegmentNumberHighPart = (uint)(segmentNumberLowPart & 0xFFFFFFFFUL);    // Get the low-order 32 bits

            ushort middle = (ushort)((segmentNumberLowPart >> 32) & 0xFFFFUL);      // Get the 16 bits in-between.

            if (middle != 0)
                throw new Exception("16 bits in-between <<Segment Number High Part>> and <<Sequence Number>> is not 0!\n");

            SegmentNumberLowPart = segmentNumberLowPart;
        }

        public MFTSegmentReference(uint segmentNumberHighPart, ushort sequenceNumber)
        {
            SegmentNumberHighPart = segmentNumberHighPart;
            SequenceNumber = sequenceNumber;
            SegmentNumberLowPart = ((ulong)sequenceNumber << 48) | segmentNumberHighPart;
        }

        public override string ToString()
        {
            return SegmentNumberHighPart + ": " + SequenceNumber;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MFTSegmentReference);
        }

        public bool Equals(MFTSegmentReference other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return SegmentNumberLowPart.GetHashCode();
        }

        public static bool operator ==(MFTSegmentReference first, MFTSegmentReference second)
        {
            if (ReferenceEquals(first, second))
                return true;

            if ((object)first == null || (object)second == null)
                return false;

            return first.SegmentNumberLowPart == second.SegmentNumberLowPart;
        }

        public static bool operator !=(MFTSegmentReference first, MFTSegmentReference second)
        {
            return !(first == second);
        }

        public static bool operator >(MFTSegmentReference first, MFTSegmentReference second)
        {
            if (ReferenceEquals(first, second))
                return false;

            if ((object)first == null)
                throw new NullReferenceException("1st MFTSegmentReference object!\n");
            if ((object)second == null)
                throw new NullReferenceException("2nd MFTSegmentReference object!\n");

            return costumCompare(first, second) > 0;
        }

        public static bool operator <(MFTSegmentReference first, MFTSegmentReference second)
        {
            if (ReferenceEquals(first, second))
                return false;

            if ((object)first == null)
                throw new NullReferenceException("1st MFTSegmentReference object!\n");
            if ((object)second == null)
                throw new NullReferenceException("2nd MFTSegmentReference object!\n");

            return costumCompare(first, second) < 0;
        }

        public static bool operator >=(MFTSegmentReference first, MFTSegmentReference second)
        {
            if (ReferenceEquals(first, second))
                return false;

            if ((object)first == null)
                throw new NullReferenceException("1st MFTSegmentReference object!\n");
            if ((object)second == null)
                throw new NullReferenceException("2nd MFTSegmentReference object!\n");

            return costumCompare(first, second) >= 0;
        }

        public static bool operator <=(MFTSegmentReference first, MFTSegmentReference second)
        {
            if (ReferenceEquals(first, second))
                return false;

            if ((object)first == null)
                throw new NullReferenceException("1st MFTSegmentReference object!\n");
            if ((object)second == null)
                throw new NullReferenceException("2nd MFTSegmentReference object!\n");

            return costumCompare(first, second) <= 0;
        }

        public int CompareTo(MFTSegmentReference other)
        {
            if ((object)other == null)
                return 1;

            return costumCompare(this, other);
        }

        private static int costumCompare(MFTSegmentReference first, MFTSegmentReference second)
        {
            if(first.SegmentNumberHighPart == second.SegmentNumberHighPart)
            {
                if (first.SequenceNumber > second.SequenceNumber)
                    return 1;
                else if (first.SequenceNumber < second.SequenceNumber)
                    return -1;

                return 0; // SegmentNumberHighPart & SequenceNumber are identical
            }

            if (first.SegmentNumberHighPart > second.SegmentNumberHighPart)
                return 1;

            return - 1; // SegmentNumberHighPart < other.SegmentNumberHighPart
        }
    }
}
