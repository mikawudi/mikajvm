using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaRTMethodArea.RTConstantInfo
{

    public class MethodRef : MemberRef
    {
        public JavaMethod Method;
        public MethodRef(RuntimeConstantPools cp, string className, string name, string desc)
            : base(cp, className, name, desc)
        {

        }

        public JavaMethod ResoverMethod()
        {
            if (this.Method != null)
                return Method;
            var d = this.ConstantPool.Class;
            var c = this.ResoverClass();
            //从d访问c的方法
            if(c.IsInterface())
                throw new Exception();
            //先从继承链中找
            var target = c;
            JavaMethod findedMethod = null;
            while (target != null)
            {
                findedMethod = c.Methods.FirstOrDefault(x => x.Name == this.Name && x.Descriptor == this.Desc);
                if (findedMethod == null)
                    target = target.SuperClass;
                else
                    break;
            }
            if (findedMethod == null)
            {
                foreach (var inte in c.Interfaces)
                {
                    findedMethod = FindMethodInInterface(inte);
                    if(findedMethod != null)
                        break;
                }
            }
            if(findedMethod == null)
                throw new Exception();
            if(!findedMethod.IsAccessTo(d))
                throw new Exception();
            Method = findedMethod;
            return findedMethod;
        }

        private JavaMethod FindMethodInInterface(JavaClass inte)
        {
            var result = inte.Methods.FirstOrDefault(x => x.Name == this.Name && x.Descriptor == this.Desc);
            if (result != null)
                return result;
            foreach (var parinte in inte.Interfaces)
            {
                result = FindMethodInInterface(parinte);
                if(result != null)
                    break;
            }
            return result;
        }
    }
}
