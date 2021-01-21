using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileSystem
{
    public static class NtfsHelper
    {
        private static readonly long maxFileTime = DateTime.MaxValue.ToFileTimeUtc();

        public static DateTime FromWinFileTime(byte[] data, int offset)
        {
            long fileTime = BitConverter.ToInt64(data, offset);

            if (fileTime >= maxFileTime)
                return DateTime.MaxValue;

            return DateTime.FromFileTimeUtc(fileTime);
        }

        public static void ToWinFileTime(byte[] data, int offset, DateTime dateTime)
        {
            if (dateTime == DateTime.MaxValue)
                LittleEndianConverter.GetBytes(data, offset, long.MaxValue);
            else
            {
                long fileTime = dateTime.ToFileTimeUtc();
                LittleEndianConverter.GetBytes(data, offset, fileTime);
            }
        }
        
        public static void ApplyUpdateSequenceNumberPatch(byte[] data, int offset, uint sectors, ushort bytesPerSector, byte[] usnNumber, byte[] usnData)
        {
            Debug.Assert(data.Length >= offset + sectors * bytesPerSector);
            Debug.Assert(usnNumber.Length == 2);
            Debug.Assert(sectors * 2 <= usnData.Length);

            for (int i = 0; i < sectors; i++)
            {
                // Get pointer to the last two bytes
                int blockOffset = offset + i * bytesPerSector + 510;

                // Check that they match the USN Number
                Debug.Assert(data[blockOffset] == usnNumber[0]);
                Debug.Assert(data[blockOffset + 1] == usnNumber[1]);

                // Patch in new data
                data[blockOffset] = usnData[i * 2];
                data[blockOffset + 1] = usnData[i * 2 + 1];
            }
        }
    }
}
