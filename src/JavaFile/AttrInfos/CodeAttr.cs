using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaFile.AttrInfos
{

    public class CodeAttr : AttrInfo
    {
        public UInt16 MaxStack;
        public UInt16 MaxLocals;
        public byte[] ByteCodes;
        public List<ExceptionEntity> EX_table = new List<ExceptionEntity>();
        public List<AttrInfo> ATT_list = new List<AttrInfo>();
        public CodeAttr(UInt16 name_index, UInt32 att_length, byte[] data, JavaClassFile fileParser)
            : base(name_index, att_length)
        {
            this.MaxStack = BitConverter.ToUInt16(data.Take(2).Reverse().ToArray(), 0);
            this.MaxLocals = BitConverter.ToUInt16(data.Skip(2).Take(2).Reverse().ToArray(), 0);
            var codeLength = BitConverter.ToUInt32(data.Skip(4).Take(4).Reverse().ToArray(), 0);
            this.ByteCodes = data.Skip(8).Take((int)codeLength).ToArray();
            var exTableLength = BitConverter.ToUInt16(data.Skip((int)codeLength + 8).Take(2).Reverse().ToArray(), 0);
            int startIndex = (int)codeLength + 10;
            for (int i = 0; i < exTableLength; i++)
            {
                var exEnity = new ExceptionEntity(data.Skip(startIndex).Take(8).ToArray());
                startIndex += 8;
                EX_table.Add(exEnity);
            }
            var attCount = BitConverter.ToUInt16(data.Skip(startIndex).Take(2).Reverse().ToArray(), 0);
            startIndex += 2;
            for (int i = 0; i < attCount; i++)
            {
                var nameIndex = BitConverter.ToUInt16(data.Skip(startIndex).Take(2).Reverse().ToArray(), 0);
                //var type = ((UTF8_CP)jvm.ConstantPools[nameIndex]).DataString;
                startIndex += 2;
                var attlength = BitConverter.ToUInt32(data.Skip(startIndex).Take(4).Reverse().ToArray(), 0);
                startIndex += 4;
                var source = data.Skip(startIndex).Take((int)attlength).ToArray();
                ATT_list.Add(fileParser.ParseAttribute(nameIndex, attlength, source));
                startIndex += (int)attlength;
            }
        }
    }

    public class ExceptionEntity
    {
        public UInt16 Start_PC;
        public UInt16 End_PC;
        public UInt16 Handle_PC;
        public UInt16 Catch_Type;
        public ExceptionEntity(byte[] data)
        {

        }
    }
}
