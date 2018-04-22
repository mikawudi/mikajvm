using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile
{
    public class MemberInfo
    {
        public byte[] AccessFlag;
        public UInt16 Name_Index;
        public UInt16 Desc_index;
        public UInt16 Attribute_count;
        public List<AttrInfo> Attribute_Info;
    }
}
