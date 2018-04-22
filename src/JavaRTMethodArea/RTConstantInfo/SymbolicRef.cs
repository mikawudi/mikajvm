using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaRTMethodArea.RTConstantInfo
{

    public abstract class SymbolicRef : RuntimeConstantInfo
    {
        public RuntimeConstantPools ConstantPool;
        public string ClassName;
        public JavaClass Class;

        public SymbolicRef(RuntimeConstantPools cp, string className)
        {
            this.ConstantPool = cp;
            this.ClassName = className;
        }

        public JavaClass ResoverClass()
        {
            if (this.Class == null)
            {
                //从continer去访问target
                var continer = this.ConstantPool.Class;
                var accessclass = this.ConstantPool.ClassLoader.GetJavaClass(this.ClassName);
                if (!accessclass.IsPublic() && continer.ThisPackageName != accessclass.ThisPackageName)
                {
                    throw new Exception("can't access class");
                    return null;
                }
                this.Class = accessclass;
            }
            return this.Class;
        }
    }
}
