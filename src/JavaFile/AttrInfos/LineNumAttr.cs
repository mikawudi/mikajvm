using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile.AttrInfos
{
    public class LineNumAttr : AttrInfo
    {
        public List<Tuple<UInt16, UInt16>> line_number_table = new List<Tuple<ushort, ushort>>();
        public LineNumAttr(UInt16 name_index, UInt32 att_length, byte[] data, JavaClassFile fileParser)
            : base(name_index, att_length)
        {
            var lineTableCount = BitConverter.ToUInt16(data.Take(2).Reverse().ToArray(), 0);
            var start = 2;
            for (int i = 0; i < lineTableCount; i++)
            {
                var start_pc = BitConverter.ToUInt16(data.Skip(start).Take(2).Reverse().ToArray(), 0);
                start += 2;
                var line_number = BitConverter.ToUInt16(data.Skip(start).Take(2).Reverse().ToArray(), 0);
                start += 2;
                line_number_table.Add(new Tuple<UInt16, UInt16>(start_pc, line_number));
            }
        }
    }
}
