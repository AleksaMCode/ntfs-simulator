using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem.Enums
{
    [Flags]
    public enum IndexValueFlags : byte // uint instead of byte ??
    {
        /// <summary>
        /// If set the index value contains a sub node Virtual Cluster Number (VCN).
        /// </summary>
        HasSubNodes = 0x01,
        /// <summary>
        /// If set the index value is the last in the index values array.
        /// </summary>
        IsLast = 0x02
    }
}
