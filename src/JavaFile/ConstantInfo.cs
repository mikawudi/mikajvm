using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikaJvm.JavaFile.ConstantInfos;

namespace MikaJvm.JavaFile
{

    public abstract class ConstantInfo
    {
        public CP_TYPE ConsType;
        public static ConstantInfo ParseConstantInfo(FileStream fs, JavaClassFile fileparser)
        {
            var type = fs.ReadByte();
            switch ((CP_TYPE)type)
            {
                case CP_TYPE.CONSTANT_Methodref:
                    {
                        var class_index = fileparser.ReadUint16(fs);
                        var name_and_type_index = fileparser.ReadUint16(fs);
                        return new MethodRef_CP(class_index, name_and_type_index);
                    }
                case CP_TYPE.CONSTANT_Fieldref:
                    {
                        var class_index = fileparser.ReadUint16(fs);
                        var name_and_type_index = fileparser.ReadUint16(fs);
                        return new FieldRef_CP(class_index, name_and_type_index);
                    }
                case CP_TYPE.CONSTANT_InterfaceMethodref:
                    {
                        var class_index = fileparser.ReadUint16(fs);
                        var name_and_type_index = fileparser.ReadUint16(fs);
                        return new InterfaceMethodref_CP(class_index, name_and_type_index);
                    }
                case CP_TYPE.CONSTANT_String:
                    {
                        var utf8_index = fileparser.ReadUint16(fs);
                        return new String_CP(utf8_index);
                    }
                case CP_TYPE.CONSTANT_Class:
                    {
                        var utf8_index = fileparser.ReadUint16(fs);
                        return new Class_CP(utf8_index);
                    }
                case CP_TYPE.CONSTANT_Utf8:
                    {
                        var length = fileparser.ReadUint16(fs);
                        var data = fileparser.ReadBytes(fs, length);
                        return new UTF8_CP(data);
                    }
                case CP_TYPE.CONSTANT_NameAndType:
                    {
                        var name_index = fileparser.ReadUint16(fs);
                        var desc_index = fileparser.ReadUint16(fs);
                        return new NameAndType_CP(name_index, desc_index);
                    }
                case CP_TYPE.CONSTANT_Long:
                    {
                        return new Long_CP(fileparser.ReadUint64(fs));
                    }
                case CP_TYPE.CONSTANT_Double:
                    {
                        return new Double_CP(fileparser.ReadDouble(fs));
                    }
                case CP_TYPE.CONSTANT_Integer:
                    {
                        return new Integer_CP(fileparser.ReadUint32(fs));
                    }
                case CP_TYPE.CONSTANT_Float:
                    {
                        return new Float_CP(fileparser.ReadFloat(fs));
                    }
                default:
                    return null;
            }
        }
    }
    public class CPTYPEAttribute : Attribute
    {
        public CP_TYPE TypeValue;
        public CPTYPEAttribute(CP_TYPE type)
        {
            this.TypeValue = type;
        }
    }
}
