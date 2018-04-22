using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile.ConstantInfos
{
    [CPTYPE(CP_TYPE.CONSTANT_Integer)]
    public class Integer_CP : ConstantInfo
    {
        public UInt32 Value;
        public Integer_CP(UInt32 value)
        {
            this.Value = value;
            this.ConsType = CP_TYPE.CONSTANT_Integer;
        }
    }
}
