using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem.Enums
{
    [Flags]
    public enum IndexNodeFlags //?
    {
        SmallIndex = 0x00,
        LargeIndex = 0x01
    }
}
