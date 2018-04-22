using MikaJvm.JavaFile;
using MikaJvm.JavaFile.ConstantInfos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikaJvm.JavaFile.AttrInfos;

namespace MikaJvm.JavaRTMethodArea
{
    public class JavaMethod
        : JavaMember
    {
        public int MaxStack;
        public int MaxLocal;
        public byte[] Code;
        private int ArgCount = -1;

        public static JavaMethod CreateMethod(JavaClass javaclass, MemberInfo membermethodInfo, Dictionary<int, ConstantInfo> staticCP)
        {
            var result = new JavaMethod();
            result.Class = javaclass;
            result.AccessFlags = membermethodInfo.AccessFlag;
            result.Name = ((UTF8_CP)staticCP[membermethodInfo.Name_Index]).DataString;
            result.Descriptor = ((UTF8_CP)staticCP[membermethodInfo.Desc_index]).DataString;
            if (!result.IsNative() && !result.IsAbstract() && !javaclass.IsInterface())
            {
                var codeattr = (CodeAttr) membermethodInfo.Attribute_Info.First(x => x.GetType() == typeof(CodeAttr));
                result.MaxLocal = codeattr.MaxLocals;
                result.MaxStack = codeattr.MaxStack;
                result.Code = codeattr.ByteCodes;
            }
            return result;
        }

        public int GetArgCount()
        {
            if (this.ArgCount != -1)
                return this.ArgCount;
            var l = Parse(this.Descriptor);
            this.ArgCount = l.Sum(x => x.Item2);
            //是成员方法的时候要+1(this指针)
            if (!this.IsStatcic())
                ArgCount++;
            return ArgCount;
        }

        private List<Tuple<string, int>> Parse(string desc)
        {
            var spi = desc.IndexOf(")");
            var pars = desc.Substring(1, spi - 1);
            var retpar = desc.Substring(spi + 1);
            var reult = new List<Tuple<string, int>>();
            int index = 0;
            while (index < pars.Length)
            {
                var temp = ParseSignal(pars, index);
                reult.Add(new Tuple<string, int>(pars.Substring(index, temp.Item1), temp.Item2));
                index += temp.Item1;
            }
            return reult;
        }

        private Tuple<int, int> ParseSignal(string str, int index)
        {
            if (str[index] == 'B' || str[index] == 'C' || str[index] == 'F' || str[index] == 'I' || str[index] == 'S' ||
                str[index] == 'Z')
            {
                return new Tuple<int, int>(1, 1);
            }
            if (str[index] == 'D' || str[index] == 'J')
            {
                return new Tuple<int, int>(2, 2);
            }
            if (str[index] == 'L')
            {
                int i = 1;
                while (str[i] != ';')
                {
                    i++;
                }
                return new Tuple<int, int>(++i, 1);
            }
            if (str[index] == '[')
            {
                var t = ParseSignal(str, index + 1);
                return new Tuple<int, int>(t.Item1 + 1, 1);
            }
            throw new Exception();
        }

        public bool IsStatcic()
        {
            return ((this.AccessFlags[1] >> 3) & 0x01) == 0x01;
        }

        public bool IsNative()
        {
            return (this.AccessFlags[0] & 0x01) == 0x01;
        }

        public bool IsAbstract()
        {
            return ((this.AccessFlags[0] >> 2) & 0x01) == 0x01;
        }
        
    }
}
