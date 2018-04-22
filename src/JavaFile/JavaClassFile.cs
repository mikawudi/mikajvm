using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikaJvm.JavaFile.AttrInfos;
using MikaJvm.JavaFile.ConstantInfos;

namespace MikaJvm.JavaFile
{
    public class JavaClassFile
    {
        public string file;
        public JavaClassFile(string fullpilepath)
        {
            this.file = fullpilepath;
        }

        public string ThisClassName;
        public string SuperclassName;
        public List<string> InterfaceNames;
        private List<string> interfaceNames = new List<string>();
        public List<MemberInfo> FieldInfo = null;
        public List<MemberInfo> MethodInfo = null;
        public byte[] AccessFlags;
        public void Parse()
        {
            var fs = new FileStream(this.file, FileMode.Open);
            ReadMagicNum(fs);
            var minor_version = ReadUint16(fs);
            var major_version = ReadUint16(fs);
            var constant_pool_count = ReadUint16(fs);
            ParseConstPool(constant_pool_count, fs);
            AccessFlags = ReadBytes(fs, 2);
            var thisClass = ReadUint16(fs);
            var temp1 = (Class_CP)ConstantPools[thisClass];
            var temp2 = (UTF8_CP)ConstantPools[temp1.Name_Index];
            this.ThisClassName = temp2.DataString;
            var superClass = ReadUint16(fs);
            if (superClass != 0)
            {
                temp1 = (Class_CP) ConstantPools[superClass];
                temp2 = (UTF8_CP) ConstantPools[temp1.Name_Index];
                this.SuperclassName = temp2.DataString;
            }
            var interfaceCount = ReadUint16(fs);
            List<UInt16> interfaces = new List<ushort>();
            for (int i = 0; i < interfaceCount; i++)
            {
                interfaces.Add(ReadUint16(fs));
            }
            foreach (var interID in interfaces)
            {
                var t = (Class_CP)ConstantPools[interID];
                var t2 = (UTF8_CP)ConstantPools[t.Name_Index];
                interfaceNames.Add(t2.DataString);
            }
            var fieldCount = ReadUint16(fs);
            var fieldInfp = new List<MemberInfo>();
            for (int i = 0; i < fieldCount; i++)
            {
                var temp = ParseFieldInfo(fs);
                fieldInfp.Add(temp);
            }
            this.FieldInfo = fieldInfp;
            var methodCount = ReadUint16(fs);
            var methodInfo = new List<MemberInfo>();
            for (int i = 0; i < methodCount; i++)
            {
                var temp = ParseMethodInfo(fs);
                methodInfo.Add(temp);
            }
            this.MethodInfo = methodInfo;
            var classAttributeCount = ReadUint16(fs);
            var classAttrList = new List<AttrInfo>();
            for (int i = 0; i < classAttributeCount; i++)
            {
                classAttrList.Add(ParseAttribute(fs));
            }
        }
        private MemberInfo ParseFieldInfo(FileStream fs)
        {
            var result = new MemberInfo();
            result.AccessFlag = this.ReadBytes(fs, 2);
            result.Name_Index = this.ReadUint16(fs);
            result.Desc_index = this.ReadUint16(fs);
            result.Attribute_count = this.ReadUint16(fs);
            result.Attribute_Info = new List<AttrInfo>();
            for (int i = 0; i < result.Attribute_count; i++)
            {
                var att = ParseAttribute(fs);
                if (att != null)
                    result.Attribute_Info.Add(att);
            }
            return result;
        }
        private MemberInfo ParseMethodInfo(FileStream fs)
        {
            var result = new MemberInfo();
            result.AccessFlag = this.ReadBytes(fs, 2);
            result.Name_Index = this.ReadUint16(fs);
            result.Desc_index = this.ReadUint16(fs);
            result.Attribute_count = this.ReadUint16(fs);
            result.Attribute_Info = new List<AttrInfo>();
            for (int i = 0; i < result.Attribute_count; i++)
            {
                var att = ParseAttribute(fs);
                if (att != null)
                    result.Attribute_Info.Add(att);
            }
            return result;
        }
        private AttrInfo ParseAttribute(FileStream fs)
        {
            var nameIndex = this.ReadUint16(fs);
            var attLength = this.ReadUint32(fs);
            var data = this.ReadBytes(fs, (int)attLength);
            return ParseAttribute(nameIndex, attLength, data);
        }
        public AttrInfo ParseAttribute(UInt16 name_index, UInt32 attLength, byte[] data)
        {
            var attType = ((UTF8_CP)this.ConstantPools[name_index]).DataString;
            switch (attType)
            {
                case "Code":
                    {
                        var codeatt = new CodeAttr(name_index, attLength, data, this);
                        return codeatt;
                    }
                case "LineNumberTable":
                    {
                        var line = new LineNumAttr(name_index, attLength, data, this);
                        return line;
                    }
                case "ConstantValue":
                    {
                        var constantValue = new ConstanValueAttr(name_index, attLength, BitConverter.ToUInt16(new byte[]{ data[1], data[0] }, 0));
                        return constantValue;
                    }
                default:
                    return null;
            }
        }
        private void ReadMagicNum(FileStream fs)
        {
            byte[] buf = new byte[4];
            fs.Read(buf, 0, 4);
            if (buf[0] != 0xca || buf[1] != 0xfe || buf[2] != 0xba || buf[3] != 0xbe)
                throw new Exception();
        }
        private byte[] tempBuffer = new byte[8];
        public UInt16 ReadUint16(FileStream fs)
        {
            fs.Read(tempBuffer, 0, 2);
            tempBuffer[2] = tempBuffer[0];
            tempBuffer[0] = tempBuffer[1];
            tempBuffer[1] = tempBuffer[2];
            return BitConverter.ToUInt16(tempBuffer, 0);
        }
        public UInt32 ReadUint32(FileStream fs)
        {
            fs.Read(tempBuffer, 0, 4);
            return BitConverter.ToUInt32(tempBuffer.Take(4).Reverse().ToArray(), 0);
        }
        public UInt64 ReadUint64(FileStream fs)
        {
            fs.Read(tempBuffer, 0, 8);
            return BitConverter.ToUInt64(tempBuffer.Take(8).Reverse().ToArray(), 0);
        }
        public float ReadFloat(FileStream fs)
        {
            fs.Read(tempBuffer, 0, 4);
            return BitConverter.ToSingle(tempBuffer.Take(4).Reverse().ToArray(), 0);
        }
        public double ReadDouble(FileStream fs)
        {
            fs.Read(tempBuffer, 0, 8);
            return BitConverter.ToDouble(tempBuffer.Take(8).Reverse().ToArray(), 0);
        }
        public byte[] ReadBytes(FileStream fs, int length)
        {
            var result = new byte[length];
            fs.Read(result, 0, result.Length);
            return result;
        }
        public Dictionary<int, ConstantInfo> ConstantPools = new Dictionary<int, ConstantInfo>();
        private void ParseConstPool(int poolsize, FileStream fs)
        {
            for (int i = 1; i < poolsize; i++)
            {
                var t = ConstantInfo.ParseConstantInfo(fs, this);
                if (t == null)
                    throw new Exception();
                ConstantPools[i] = t;
                if (t.ConsType == CP_TYPE.CONSTANT_Long || t.ConsType == CP_TYPE.CONSTANT_Double)
                    i++;
            }
        }
    }
    
    

}
