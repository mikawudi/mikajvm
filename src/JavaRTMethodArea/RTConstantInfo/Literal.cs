using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaRTMethodArea.RTConstantInfo
{

    public class Literal<T>
        : RuntimeConstantInfo
    {
        public T Value;

        public Literal(T val)
        {
            this.Value = val;
        }
    }
}
