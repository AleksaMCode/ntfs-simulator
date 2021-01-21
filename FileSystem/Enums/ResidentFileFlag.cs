using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem.Enums
{
    public enum ResidentFileFlag : byte // Created for ATTRIBUTE_RECORD_HEADER structure usage.
    {
        Resident = 0,   // When a file's attributes can fit within the MFT file record, they are called resident attributes.
        NonResident = 1 // This project allows me to not implement non resident files!
    }
}
