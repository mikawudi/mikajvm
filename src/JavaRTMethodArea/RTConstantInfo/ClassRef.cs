using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaRTMethodArea.RTConstantInfo
{

    public class ClassRef : SymbolicRef
    {
        public ClassRef(RuntimeConstantPools cp, string className)
            : base(cp, className)
        {

        }
    }
}
