using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile.AttrInfos
{
    public class ConstanValueAttr : AttrInfo
    {
        public UInt16 Constantvalue_Index;

        public ConstanValueAttr(UInt16 name_index, UInt32 att_lengt, UInt16 constantvalue_index)
            : base(name_index, att_lengt)
        {
            this.Constantvalue_Index = constantvalue_index;
        }
    }
}
