using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile.ConstantInfos
{
    [CPTYPE(CP_TYPE.CONSTANT_Methodref)]
    public class MethodRef_CP : ConstantInfo
    {
        public UInt16 Class_Index;
        public UInt16 Name_And_Type_Index;
        public MethodRef_CP(UInt16 class_index, UInt16 name_and_type_index)
        {
            this.ConsType = CP_TYPE.CONSTANT_Methodref;
            this.Class_Index = class_index;
            this.Name_And_Type_Index = name_and_type_index;
        }
    }
}
