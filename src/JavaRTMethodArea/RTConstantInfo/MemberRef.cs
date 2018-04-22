using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaRTMethodArea.RTConstantInfo
{

    public abstract class MemberRef : SymbolicRef
    {
        public string Name;
        public string Desc;
        public MemberRef(RuntimeConstantPools cp, string className, string name, string desc)
            : base(cp, className)
        {
            this.Name = name;
            this.Desc = desc;
        }
    }
}
