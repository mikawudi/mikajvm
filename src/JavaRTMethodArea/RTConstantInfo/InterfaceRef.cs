using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaRTMethodArea.RTConstantInfo
{

    public class InterfaceRef : MemberRef
    {
        public JavaMethod Method;
        public InterfaceRef(RuntimeConstantPools cp, string className, string name, string desc)
            : base(cp, className, name, desc)
        {

        }

        public JavaMethod ResoverMethod()
        {
            if (this.Method != null)
                return this.Method;
            var d = this.ConstantPool.Class;
            var c = this.ResoverClass();
            if(!c.IsInterface())
                throw new Exception();
            var result = FindMethodInInterface(c);
            if(result == null)
                throw new Exception();
            if(!result.IsAccessTo(d))
                throw new Exception();
            this.Method = result;
            return result;
        }


        private JavaMethod FindMethodInInterface(JavaClass cl)
        {
            var findedmethod = cl.Methods.FirstOrDefault(x => x.Name == this.Name && x.Descriptor == this.Desc);
            if (findedmethod == null)
            {
                foreach (var supinte in cl.Interfaces)
                {
                    findedmethod = FindMethodInInterface(supinte);
                    if (findedmethod != null)
                        return findedmethod;
                }
            }
            return null;
        }
    }
}
