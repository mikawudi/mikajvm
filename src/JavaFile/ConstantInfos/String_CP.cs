using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile.ConstantInfos
{
    [CPTYPE(CP_TYPE.CONSTANT_String)]
    public class String_CP : ConstantInfo
    {
        public UInt16 Utf8Str_Index;
        public String_CP(UInt16 utf8str_index)
        {
            this.Utf8Str_Index = utf8str_index;
            this.ConsType = CP_TYPE.CONSTANT_String;
        }
    }
}
