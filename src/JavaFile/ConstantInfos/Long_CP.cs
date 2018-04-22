using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile.ConstantInfos
{
    [CPTYPE(CP_TYPE.CONSTANT_Long)]
    public class Long_CP : ConstantInfo
    {
        public UInt64 Value;
        public Long_CP(UInt64 value)
        {
            this.ConsType = CP_TYPE.CONSTANT_Long;
            this.Value = value;
        }
    }
}
