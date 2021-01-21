using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem.Enums
{
    public enum MFTFiles : uint
    {
        MFT = 0,
        MFTMirr = 1,
        LogFile = 2,
        Volume = 3,
        AttrDef = 4,
        RootDir = 5,
        Bitmap = 6,
        Boot = 7,
        BadClus = 8,
        Secure = 9,
        UpCase = 10,
        Extend = 11,
        /* 12 - 23 reserved */
        Quota = 24,
        ObjId = 25,
        Reparse = 26
    }
}
