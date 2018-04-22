using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile.ConstantInfos
{
    [CPTYPE(CP_TYPE.CONSTANT_Double)]
    public class Double_CP : ConstantInfo
    {
        public double Value;
        public Double_CP(double value)
        {
            this.ConsType = CP_TYPE.CONSTANT_Double;
            this.Value = value;
        }
    }
}
