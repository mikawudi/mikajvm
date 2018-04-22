using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile.ConstantInfos
{
    [CPTYPE(CP_TYPE.CONSTANT_NameAndType)]
    public class NameAndType_CP : ConstantInfo
    {
        public UInt16 Name_Index;
        public UInt16 Descrptor_Index;
        public NameAndType_CP(UInt16 name_index, UInt16 desc_index)
        {
            this.ConsType = CP_TYPE.CONSTANT_NameAndType;
            this.Name_Index = name_index;
            this.Descrptor_Index = desc_index;
        }
    }
}
