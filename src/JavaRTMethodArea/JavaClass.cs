using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikaJvm.JavaFile;
using MikaJvm.JavaFile.AttrInfos;
using MikaJvm.JavaFile.ConstantInfos;
using MikaJvm.JavaRTMethodArea.RTConstantInfo;

namespace MikaJvm.JavaRTMethodArea
{
    public class JavaClass
    {
        private JavaClassFile classFile = null;
        public string ThisClassName;
        public string ThisPackageName;
        public byte[] AccessFlag;
        public string SuperClassName;
        public List<string> InterfaceNames;
        public JavaClass SuperClass;
        public List<JavaClass> Interfaces = new List<JavaClass>();
        public RuntimeConstantPools RTCP;
        public int InstanceSoltCount;
        public int StaticSoltCount;
        //类的的静态变量放在方法区内
        public LocalVars StaticClassVars;

        public List<JavaField> Fields = new List<JavaField>();
        public List<JavaMethod> Methods = new List<JavaMethod>();
        public VmContiner ClassLoader;
        public bool inited = false;

        //反射用的java.lang.class的实例
        public JavaObject ClassObj;

        public JavaClass(JavaClassFile jcf, VmContiner continerLoader)
        {
            classFile = jcf;
            this.ClassLoader = continerLoader;
            Convert();
        }

        private JavaClass(VmContiner continerLoader)
        {
            this.ClassLoader = continerLoader;

        }
        public static JavaClass CreateBaseArrayType(VmContiner continerLoader, string name)
        {
            var superClass = continerLoader.GetJavaClass("java/lang/Object");
            var interfaceList = new List<JavaClass>()
            {
                continerLoader.GetJavaClass("java/lang/Cloneable"),
                continerLoader.GetJavaClass("java/io/Serializable"),
            };
            var result = new JavaClass(continerLoader);
            continerLoader.SetClassRefObject(result);
            //没有类装载器
            result.ThisClassName = name;
            result.SuperClass = superClass;
            result.SuperClassName = superClass.ThisClassName;
            result.Interfaces = interfaceList;
            result.InterfaceNames = interfaceList.Select(x => x.ThisClassName).ToList();
            result.inited = true;
            return result;
        }

        public static JavaClass CreateBasicType(VmContiner continerLoader, string name)
        {
            var result = new JavaClass(continerLoader);
            continerLoader.SetClassRefObject(result);
            result.ThisClassName = name;
            result.inited = true;
            return result;
        }

        private void Convert()
        {
            AccessFlag = this.classFile.AccessFlags;
            SuperClassName = this.classFile.SuperclassName;
            InterfaceNames = this.classFile.InterfaceNames;
            var name = this.classFile.ThisClassName;
            this.ThisClassName = name;
            var hasPack = name.LastIndexOf("/");
            if (hasPack != -1)
            {
                this.ThisPackageName = name.Substring(0, hasPack);
            }
            foreach (var field in this.classFile.FieldInfo)
            {
                Fields.Add(JavaField.CreateFeld(this, field, this.classFile.ConstantPools));
            }
            foreach (var method in this.classFile.MethodInfo)
            {
                Methods.Add(JavaMethod.CreateMethod(this, method, this.classFile.ConstantPools));
            }
            RTCP = new RuntimeConstantPools(this.classFile.ConstantPools, ClassLoader, this);
        }

        public bool IsPublic()
        {
            return (this.AccessFlag[1] & 0x01) == 0x01;
        }

        public bool IsInterface()
        {
            return ((this.AccessFlag[0] >> 1) & 0x01) == 0x01;
        }

        public JavaField LookUpField(string fieldName, string desc)
        {
            var result = this.Fields.FirstOrDefault(x => x.Name == fieldName && x.Descriptor == desc);
            if (result != null)
                return result;
            foreach (var inf in this.Interfaces)
            {
                result = inf.LookUpField(fieldName, desc);
                if (result != null)
                    return result;
            }
            if (this.SuperClass != null)
                return this.SuperClass.LookUpField(fieldName, desc);
            return null;
        }

        public bool IsSubClassOf(JavaClass parClass)
        {
            if (this.SuperClass == parClass)
                return true;
            if (this.Interfaces.Contains(parClass))
                return true;
            foreach (var inf in this.Interfaces)
            {
                if (inf.IsSubClassOf(inf))
                    return true;
            }
            return false;
        }
    }

   
}
