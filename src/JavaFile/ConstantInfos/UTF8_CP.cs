using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile.ConstantInfos
{
    [CPTYPE(CP_TYPE.CONSTANT_Utf8)]
    public class UTF8_CP : ConstantInfo
    {
        public byte[] Data;
        public string DataString;
        public UTF8_CP(byte[] data)
        {
            this.ConsType = CP_TYPE.CONSTANT_Utf8;
            this.Data = data;
            this.DataString = Encoding.UTF8.GetString(data);
        }
    }
}
