using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile.ConstantInfos
{
    [CPTYPE(CP_TYPE.CONSTANT_Class)]
    public class Class_CP : ConstantInfo
    {
        //CONSTANT_UTF8
        public UInt16 Name_Index;
        public Class_CP(UInt16 name_index)
        {
            this.ConsType = CP_TYPE.CONSTANT_Class;
            this.Name_Index = name_index;
        }
    }
}
