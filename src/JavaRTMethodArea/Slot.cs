using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaRTMethodArea
{
    public class Slot
    {
        public JavaObject refObj;
        public byte[] data = new byte[4];
    }
}
