using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.ByteCodes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ByteCodeTag
        : Attribute
    {
        public byte Tag { get; private set; }

        public ByteCodeTag(byte tag)
        {
            this.Tag = tag;
        }
    }
}
