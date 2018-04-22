using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaRTMethodArea
{

    public abstract class JavaMember
    {
        public byte[] AccessFlags;
        public string Name;
        public string Descriptor;
        public JavaClass Class;
        public bool IsAccessTo(JavaClass targetclass)
        {
            if (this.IsPublic())
            {
                return true;
            }
            if (this.IsProtected())
            {
                if (Class.ThisPackageName == targetclass.ThisPackageName ||
                    Class.IsSubClassOf(targetclass))
                    return true;
            }
            //包私有
            if (!this.IsPrivate())
            {
                if (Class.ThisPackageName == targetclass.ThisPackageName)
                    return true;
            }
            //私有
            else
            {
                if (Class == targetclass)
                    return true;
            }
            return false;
        }


        public bool IsPublic()
        {
            return (this.AccessFlags[1] & 0x01) == 1;
        }

        public bool IsPrivate()
        {
            return ((this.AccessFlags[1] >> 1) & 0x01) == 1;
        }

        public bool IsProtected()
        {
            return ((this.AccessFlags[1] >> 2) & 0x01) == 1;
        }
    }
}
