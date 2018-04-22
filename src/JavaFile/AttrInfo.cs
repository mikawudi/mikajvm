using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile
{
    public abstract class AttrInfo
    {
        public UInt16 Name_Index;
        public UInt32 Att_Length;
        public AttrInfo(UInt16 name_index, UInt32 att_length)
        {
            this.Name_Index = name_index;
            this.Att_Length = att_length;
        }
    }
}
