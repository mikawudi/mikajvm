using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikaJvm.JavaFile;
using MikaJvm.JavaFile.ConstantInfos;

namespace MikaJvm.JavaRTMethodArea.RTConstantInfo
{

    public class RuntimeConstantPools
    {
        public Dictionary<int, RuntimeConstantInfo> dic = new Dictionary<int, RuntimeConstantInfo>();
        private TypeSwitch<RuntimeConstantInfo> swi = null;
        public VmContiner ClassLoader = null;
        /// <summary>
        /// runtimeConstanPool所属的JavaClass
        /// </summary>
        public JavaClass Class;
        public RuntimeConstantPools(Dictionary<int, ConstantInfo> filecp, VmContiner cl, JavaClass jclass)
        {
            this.Class = jclass;
            this.ClassLoader = cl;
            swi = new TypeSwitch<RuntimeConstantInfo>()
                .Case((Integer_CP x) => new Literal<int>((int)x.Value))
                .Case((Double_CP x) => new Literal<double>(x.Value))
                .Case((Float_CP x) => new Literal<float>(x.Value))
                .Case((Long_CP x) => new Literal<long>((long)x.Value))
                .Case((String_CP x) => new Literal<string>(((UTF8_CP)filecp[x.Utf8Str_Index]).DataString))
                .Case((UTF8_CP x) => new Literal<string>(x.DataString))
                .Case((Class_CP x) => new ClassRef(this, ((UTF8_CP)filecp[x.Name_Index]).DataString))
                .Case((FieldRef_CP x) =>
                {
                    var classname = ((UTF8_CP)filecp[((Class_CP)filecp[x.Class_Index]).Name_Index]).DataString;
                    var namedAndTypeRef = (NameAndType_CP)filecp[x.Name_And_Type_Index];
                    return new FieldRef(this, classname, ((UTF8_CP)filecp[namedAndTypeRef.Name_Index]).DataString,
                        ((UTF8_CP)filecp[namedAndTypeRef.Descrptor_Index]).DataString);
                })
                .Case((MethodRef_CP x) =>
                {
                    var classname = ((UTF8_CP)filecp[((Class_CP)filecp[x.Class_Index]).Name_Index]).DataString;
                    var namedAndTypeRef = (NameAndType_CP)filecp[x.Name_And_Type_Index];
                    return new MethodRef(this, classname, ((UTF8_CP)filecp[namedAndTypeRef.Name_Index]).DataString,
                        ((UTF8_CP)filecp[namedAndTypeRef.Descrptor_Index]).DataString);
                })
                .Case((InterfaceMethodref_CP x) =>
                {
                    var classname = ((UTF8_CP)filecp[((Class_CP)filecp[x.Class_Index]).Name_Index]).DataString;
                    var namedAndTypeRef = (NameAndType_CP)filecp[x.Name_And_Type_Index];
                    return new InterfaceRef(this, classname, ((UTF8_CP)filecp[namedAndTypeRef.Name_Index]).DataString,
                        ((UTF8_CP)filecp[namedAndTypeRef.Descrptor_Index]).DataString);
                })
                .Case((NameAndType_CP x) => null);
            foreach (var kvp in filecp)
            {
                dic.Add(kvp.Key, swi.Switch(kvp.Value));
            }
        }
    }
}
