using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaRTMethodArea.RTConstantInfo
{

    public class FieldRef : MemberRef
    {
        public JavaField Field;

        public FieldRef(RuntimeConstantPools cp, string className, string name, string desc)
            : base(cp, className, name, desc)
        {

        }

        public JavaField ResoverField()
        {
            if (Field == null)
            {
                var targetclass = this.ResoverClass();
                var temp = targetclass.LookUpField(this.Name, this.Desc);
                if(!temp.IsAccessTo(targetclass))
                    throw new Exception();
                Field = temp;
            }
            return Field;
        }
    }
}
