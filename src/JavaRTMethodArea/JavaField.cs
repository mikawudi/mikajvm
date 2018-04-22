using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikaJvm.JavaFile;
using MikaJvm.JavaFile.AttrInfos;
using MikaJvm.JavaFile.ConstantInfos;

namespace MikaJvm.JavaRTMethodArea
{

    public class JavaField
        : JavaMember
    {
        public int SoltIndex = 0;
        public UInt16 ConstantValue;
        public static JavaField CreateFeld(JavaClass javaclass, MemberInfo memberfileInfo, Dictionary<int, ConstantInfo> staticCP)
        {
            var result = new JavaField();
            result.Class = javaclass;
            result.AccessFlags = memberfileInfo.AccessFlag;
            result.Name = ((UTF8_CP)staticCP[memberfileInfo.Name_Index]).DataString;
            result.Descriptor = ((UTF8_CP)staticCP[memberfileInfo.Desc_index]).DataString;
            if(result.IsStatic() && result.IsFinal())
            {
                var finalfieldConstantVal = memberfileInfo.Attribute_Info.FirstOrDefault(x => x.GetType() == typeof(ConstanValueAttr)) as ConstanValueAttr;
                if (finalfieldConstantVal != null)
                    result.ConstantValue = finalfieldConstantVal.Constantvalue_Index;
                else
                    result.ConstantValue = 0;

            }
            return result;
        }

        public bool IsStatic()
        {
            return ((this.AccessFlags[1] >> 3) & 0x01) == 1;
        }

        public bool IsFinal()
        {
            return ((this.AccessFlags[1] >> 4) & 0x01) == 1;
        }


        public bool IsLongOrDouble()
        {
            if (this.Descriptor == "J" || this.Descriptor == "D")
                return true;
            return false;
        }
    }
}
