using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile.ConstantInfos
{
    [CPTYPE(CP_TYPE.CONSTANT_Float)]
    public class Float_CP : ConstantInfo
    {
        public float Value;
        public Float_CP(float value)
        {
            this.ConsType = CP_TYPE.CONSTANT_Float;
            this.Value = value;
        }
    }
}
