using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile.ConstantInfos
{
    [CPTYPE(CP_TYPE.CONSTANT_Fieldref)]
    public class FieldRef_CP : ConstantInfo
    {
        public UInt16 Class_Index;
        public UInt16 Name_And_Type_Index;
        public FieldRef_CP(UInt16 class_index, UInt16 name_and_type_index)
        {
            this.ConsType = CP_TYPE.CONSTANT_Fieldref;
            this.Class_Index = class_index;
            this.Name_And_Type_Index = name_and_type_index;
        }
    }
}
