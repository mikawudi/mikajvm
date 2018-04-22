using MikaJvm.JavaFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MikaJvm.JavaFile.ConstantInfos;
using MikaJvm.JavaRTMethodArea;
using MikaJvm.JavaRTMethodArea.RTConstantInfo;

namespace MikaJvm
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> defaultClassPath = new List<string>()
            {
                System.Environment.CurrentDirectory + "\\",
                "E:\\mikajava\\"
            };
            args = new string[]{ "Test" };
            if (args.Length < 1)
            {
                Console.WriteLine("class文件为必需参数");
                return;
            }
            var cf = args[0];
            if (!cf.EndsWith(".class"))
                cf += ".class";
            defaultClassPath.AddRange(args.Skip(1).ToList());
            var continer = new VmContiner(defaultClassPath, args[0]);
            continer.RegisterNativeFunc("java/lang/System", "arraycopy", "(Ljava/lang/Object;ILjava/lang/Object;II)V", new CopyArray());
            continer.RegisterNativeFunc("java/lang/Class", "getPrimitiveClass", "(Ljava/lang/String;)Ljava/lang/Class;", new GetPrimitiveClass(continer));
            continer.RegisterNativeFunc("Test", "PrintData", "(Ljava/lang/String;)V", new PrintData());
            continer.LoadBasicClass();
            continer.StartParse();
            Console.WriteLine("运行完毕");
            Console.ReadLine();
        }
    }

    public class GetPrimitiveClass
        : INativeCode
    {
        private VmContiner ClassLoader;
        public GetPrimitiveClass(VmContiner cl) => this.ClassLoader = cl;
        public void Work(Frame frame, JThread jthread)
        {
            var jobj = frame.LocalVars.GetObj(0);
            var chararray = jobj.Slots[0].refObj;
            var datas = chararray.Slots.Select(x => x.data[0]).ToArray();
            var str = Encoding.ASCII.GetString(datas);
            var temp = ClassLoader.GetJavaBasicClass(str);
            var privateClassObj = temp.ClassObj;
            jthread.FrameStack.Peek().OPStack.Push(new Slot(){ refObj = privateClassObj});
        }
    }
    public class CopyArray
        : INativeCode
    {
        public void Work(Frame frame, JThread jthread)
        {
            var src = frame.LocalVars.GetObj(0);
            var srcpos = frame.LocalVars.GetInt(1);
            var dest = frame.LocalVars.GetObj(2);
            var destpost = frame.LocalVars.GetInt(3);
            var length = frame.LocalVars.GetInt(4);
            if (src == null || dest == null)
                throw new Exception();
            
            for(int i = 0; i < length; i++)
            {
                dest.Slots[destpost + i].data = (byte[])src.Slots[srcpos + i].data.Clone();
                dest.Slots[destpost + i].refObj = src.Slots[srcpos + i].refObj;
            }
        }
    }

    public class PrintData
        : INativeCode
    {
        public void Work(Frame frame, JThread jthread)
        {
            var strobj = frame.LocalVars.GetObj(0);
            var chars = strobj.Slots[0].refObj;
            Console.WriteLine(Encoding.ASCII.GetString(chars.Slots.Select(x => x.data[0]).ToArray()));
        }
    }

    public interface INativeCode
    {
        void Work(Frame frame, JThread jthread);
    }
    public class VmContiner
    {
        public string JavaCF;
        public List<string> PathList;
        private Dictionary<string, JavaClassFile> classfileDic= new Dictionary<string, JavaClassFile>();
        private Dictionary<string, JavaClass> MethodArea = new Dictionary<string, JavaClass>();
        public VmContiner(List<string> pathList, string javaClassFile)
        {
            this.PathList = pathList;
            this.JavaCF = javaClassFile;
        }
        private Dictionary<string, INativeCode> NativeFuncs = new Dictionary<string, INativeCode>();

        public void RegisterNativeFunc(string cname, string name, string desc, INativeCode navcode)
        {
            var key = cname + "~" + name + "~" + desc;
            if(!NativeFuncs.ContainsKey(key))
                NativeFuncs.Add(key, navcode);
        }

        public void SetClassRefObject(JavaClass cla)
        {
            if (this.MethodArea.TryGetValue("java/lang/Class", out JavaClass resu))
            {
                cla.ClassObj = new JavaObject(resu);
                cla.ClassObj.Extension = cla;
            }
        }

        public INativeCode GetNativeFunc(string cname, string name, string desc)
        {
            var key = cname + "~" + name + "~" + desc;
            return NativeFuncs[key];
        }
        //基础类型和反射用类型的装载
        public void LoadBasicClass()
        {
            var refclass = this.GetJavaClass("java/lang/Class");
            foreach (var cla in this.MethodArea)
            {
                SetClassRefObject(cla.Value);
            }
        }

        public void StartParse()
        {

            var javaclass = GetJavaClass(JavaCF);
            JThread mainThread = new JThread();

            //var jmethod = javaclass.Methods.FirstOrDefault(x => x.Name == "<clinit>");
            var jMainMethod = javaclass.Methods.FirstOrDefault(x => x.Name.Contains("main") && x.IsStatcic());
            //if(jmethod == null)
            //    throw new Exception();
            //mainThread.StartWork(jmethod);
            mainThread.StartWork(jMainMethod);
        }

        public JavaClass GetJavaClass(string fullclassname)
        {
            if (this.MethodArea.ContainsKey(fullclassname))
                return MethodArea[fullclassname];
            //数组类型特殊处理
            if (fullclassname[0] == '[')
            {
                var temp = GetJavaArrayClass(fullclassname);
                if(temp == null)
                    throw new Exception();
                this.MethodArea[fullclassname] = temp;
                return temp;
            }
            //非数组类型
            string fullpath = string.Empty;
            foreach (var temppath in PathList)
            {
                var f = temppath + fullclassname.Replace("/", "\\")+".class";
                if (File.Exists(f))
                {
                    fullpath = f;
                    break;
                }
            }
            if (string.IsNullOrEmpty(fullpath))
                throw new Exception("找不到class文件");
            JavaClass result = null;
            JavaClassFile classFile = null;
            if (!classfileDic.TryGetValue(fullpath, out classFile))
            {
                var javaClassFile = new JavaClassFile(fullpath);
                javaClassFile.Parse();
                classfileDic.Add(fullpath, javaClassFile);
                classFile = javaClassFile;
            }
            result = new JavaClass(classFile, this);
            this.MethodArea[fullclassname] = result;
            resolveSuperClass(result);
            resolveInterfaceList(result);
            Link(result);
            SetClassRefObject(result);
            return result;
        }

        private JavaClass GetJavaArrayClass(string name)
        {
            if (this.MethodArea.TryGetValue(name, out JavaClass resu))
                return resu;
            resu = JavaClass.CreateBaseArrayType(this, name);
            this.MethodArea[name] = resu;
            return resu;
        }

        public JavaClass GetJavaBasicClass(string name)
        {
            if (this.MethodArea.TryGetValue(name, out JavaClass resu))
                return resu;
            resu = JavaClass.CreateBasicType(this, name);
            this.MethodArea[name] = resu;
            return resu;
        }

        private void resolveSuperClass(JavaClass javaclass)
        {
            if (javaclass.SuperClassName == null)
                return;
            if (javaclass.SuperClassName == "java/lang/Object")
                return;
            javaclass.SuperClass = this.GetJavaClass(javaclass.SuperClassName);
        }

        private void resolveInterfaceList(JavaClass javaclass)
        {
            if(javaclass.InterfaceNames == null || javaclass.Interfaces.Count == 0)
                return;
            foreach (var interfaceName in javaclass.InterfaceNames)
            {
                javaclass.Interfaces.Add(this.GetJavaClass(interfaceName));
            }
        }

        private void Link(JavaClass javaclass)
        {
            calcInstanceFieldSlotIds(javaclass);
            calcStaticFieldSlotIds(javaclass);
            allocAndInitStaticVal(javaclass);
        }

        private void calcInstanceFieldSlotIds(JavaClass javaclass)
        {
            var soltID = 0;
            if (javaclass.SuperClass != null)
            {
                soltID = javaclass.SuperClass.InstanceSoltCount;
            }
            foreach (var field in javaclass.Fields.Where(x=>!x.IsStatic()))
            {
                field.SoltIndex = soltID;
                soltID++;
                if (field.IsLongOrDouble())
                    soltID++;
            }
            javaclass.InstanceSoltCount = soltID;
        }

        private void calcStaticFieldSlotIds(JavaClass javaclass)
        {
            var soltID = 0;
            foreach (var field in javaclass.Fields.Where(x=>x.IsStatic()))
            {
                field.SoltIndex = soltID;
                soltID++;
                if (field.IsLongOrDouble())
                    soltID++;
            }
            javaclass.StaticSoltCount = soltID;

        }

        public void allocAndInitStaticVal(JavaClass javaclass)
        {
            javaclass.StaticClassVars = new LocalVars(javaclass.StaticSoltCount);
            foreach (var field in javaclass.Fields.Where(x=>x.IsStatic() && x.IsFinal()))
            {
                if(field.ConstantValue == 0)
                    continue;
                var value = field.Class.RTCP.dic[field.ConstantValue];
                switch (field.Descriptor)
                {
                    case "B":
                    {
                        var t = value as Literal<byte>;
                            javaclass.StaticClassVars.SetByte(field.SoltIndex, t.Value);
                            break;
                    }
                    case "C":
                    {
                        var t = value as Literal<char>;
                        javaclass.StaticClassVars.SetChar(field.SoltIndex, t.Value);
                            break;
                    }
                    case "D":
                    {
                        var t = value as Literal<double>;
                        javaclass.StaticClassVars.Setdouble(field.SoltIndex, t.Value);
                        break;
                    }
                    case "F":
                    {
                        var t = value as Literal<float>;
                        javaclass.StaticClassVars.SetFloat(field.SoltIndex, t.Value);
                        break;
                    }
                    case "I":
                    {
                        var t = value as Literal<int>;
                        javaclass.StaticClassVars.SetInt(field.SoltIndex, t.Value);
                        break;
                    }
                    case "J":
                    {
                        var t = value as Literal<long>;
                        javaclass.StaticClassVars.SetLong(field.SoltIndex, t.Value);
                        break;
                    }
                    case "S":
                    {
                        var t = value as Literal<short>;
                        javaclass.StaticClassVars.SetShort(field.SoltIndex, t.Value);
                        break;
                    }
                    case "Z":
                    {
                        var t = value as Literal<bool>;
                        javaclass.StaticClassVars.SetBool(field.SoltIndex, t.Value);
                        break;
                    }
                    case "Ljava/lang/String;":
                    {
                        //TODO 字符串常量的加载
                        break;
                    }
                }
            }
        }
    }

    
    public class TypeSwitch<V>
    {
        Dictionary<Type, Func<object, V>> matches = new Dictionary<Type, Func<object, V>>();

        public TypeSwitch<V> Case<T>(Func<T, V> action)
        {
            matches.Add(typeof(T), (x) => action((T)x));
            return this;
        }

        public V Switch(object x)
        {
            return matches[x.GetType()](x);
        }
    }

    public class JThread
    {
        public int PC;
        public Stack<Frame> FrameStack;

        public JThread()
        {
            this.PC = 0;
            this.FrameStack = new Stack<Frame>();
        }

        public void StartWork(JavaMethod jMainmethod)
        {
            var fm = new Frame(jMainmethod, this);
            this.FrameStack.Push(fm);
            StartWork();
        }

        public void StartWork()
        {
            while (this.FrameStack.Count > 0)
            {
                var cuFrame = this.FrameStack.Peek();
                this.PC = cuFrame.FramePC;
                var instruction = FetchInst(this.PC, this.FrameStack.Peek());
                cuFrame.FramePC += (instruction.GetOp() + 1);
                instruction.Execute();
            }
        }

        private Instructions FetchInst(int pc, Frame cuframe)
        {
            //TODO GetStatic未完成,需要根据Field的desc来确定GET和push到OPStack中的数据类型
            if (cuframe.Codes[pc] == 0x01)
                return new AConst_NULL(cuframe);
            if (cuframe.Codes[pc] == 0x02)
                return new IConstM1(cuframe);
            if (cuframe.Codes[pc] == 0x03)
                return new IConst_0(cuframe);
            if (cuframe.Codes[pc] == 0x04)
                return new IConst_1(cuframe);
            if (cuframe.Codes[pc] == 0x05)
                return new IConst_2(cuframe);
            if (cuframe.Codes[pc] == 0x06)
                return new IConst_3(cuframe);
            if (cuframe.Codes[pc] == 0x07)
                return new IConst_4(cuframe);
            if (cuframe.Codes[pc] == 0x08)
                return new IConst_5(cuframe);
            if (cuframe.Codes[pc] == 0x09)
                return new LConst_0(cuframe);
            if (cuframe.Codes[pc] == 0x0a)
                return new LConst_1(cuframe);
            if (cuframe.Codes[pc] == 0x0b)
                return new FConst_0(cuframe);
            if (cuframe.Codes[pc] == 0x0c)
                return new FConst_1(cuframe);
            if (cuframe.Codes[pc] == 0x0d)
                return new FConst_2(cuframe);
            if (cuframe.Codes[pc] == 0x0e)
                return new DConst_0(cuframe);
            if (cuframe.Codes[pc] == 0x0f)
                return new DConst_1(cuframe);
            if (cuframe.Codes[pc] == 0x10)
                return new BiPush(cuframe);
            if (cuframe.Codes[pc] == 0x11)
                return new SiPush(cuframe);
            if (cuframe.Codes[pc] == 0x12)
                return new Ldc(cuframe);
            if (cuframe.Codes[pc] == 0x15)
                return new ILoad(cuframe);
            if (cuframe.Codes[pc] == 0x1a)
                return new ILoad_0(cuframe);
            if (cuframe.Codes[pc] == 0x1b)
                return new ILoad_1(cuframe);
            if (cuframe.Codes[pc] == 0x1c)
                return new ILoad_2(cuframe);
            if (cuframe.Codes[pc] == 0x1d)
                return new ILoad_3(cuframe);
            if (cuframe.Codes[pc] == 0x2a)
                return new ALoad_0(cuframe);
            if (cuframe.Codes[pc] == 0x2b)
                return new ALoad_1(cuframe);
            if (cuframe.Codes[pc] == 0x2d)
                return new ALoad_3(cuframe);
            if (cuframe.Codes[pc] == 0x2e)
                return new Iaload(cuframe);
            if (cuframe.Codes[pc] == 0x36)
                return new IStore(cuframe);
            if (cuframe.Codes[pc] == 0x3c)
                return new IStore_1(cuframe);
            if (cuframe.Codes[pc] == 0x3d)
                return new IStore_2(cuframe);
            if (cuframe.Codes[pc] == 0x3e)
                return new IStore_3(cuframe);
            if (cuframe.Codes[pc] == 0x4f)
                return new Iastore(cuframe);
            if (cuframe.Codes[pc] == 0x55)
                return new Castore(cuframe);
            if (cuframe.Codes[pc] == 0x57)
                return new Pop(cuframe);
            if (cuframe.Codes[pc] == 0x59)
                return new Dup(cuframe);
            if (cuframe.Codes[pc] == 0x60)
                return new IAdd(cuframe);
            if (cuframe.Codes[pc] == 0x64)
                return new ISub(cuframe);
            if (cuframe.Codes[pc] == 0x68)
                return new IMul(cuframe);
            if (cuframe.Codes[pc] == 0x84)
                return new Iinc(cuframe);
            if (cuframe.Codes[pc] == 0x9c)
                return new Ifge(cuframe);
            if (cuframe.Codes[pc] == 0x9e)
                return new Ifle(cuframe);
            if (cuframe.Codes[pc] == 0xa0)
                return new Ificmpne(cuframe);
            if (cuframe.Codes[pc] == 0xa1)
                return new Ificmplt(cuframe);
            if (cuframe.Codes[pc] == 0xa3)
                return new Ificmpgt(cuframe);
            if (cuframe.Codes[pc] == 0xa4)
                return new Ificmple(cuframe);
            if (cuframe.Codes[pc] == 0xa7)
                return new Goto(cuframe);
            if (cuframe.Codes[pc] == 0xac)
                return new IReturn(cuframe);
            if (cuframe.Codes[pc] == 0xb0)
                return new AReturn(cuframe);
            if (cuframe.Codes[pc] == 0xb1)
                return new Return(cuframe);
            if (cuframe.Codes[pc] == 0xb2)
                return new GetStatic(cuframe);
            if (cuframe.Codes[pc] == 0xb3)
                return new PushStatic(cuframe);
            if (cuframe.Codes[pc] == 0xb4)
                return new GetField(cuframe);
            if (cuframe.Codes[pc] == 0xb5)
                return new PushField(cuframe);
            if (cuframe.Codes[pc] == 0xb6)
                return new InvokeVirtual(cuframe);
            if (cuframe.Codes[pc] == 0xb7)
                return new InvokeSpecial(cuframe);
            if (cuframe.Codes[pc] == 0xb8)
                return new InvokeStatic(cuframe);
            if (cuframe.Codes[pc] == 0xbb)
                return new New(cuframe);
            if (cuframe.Codes[pc] == 0xbc)
                return new NewArray(cuframe);
            if (cuframe.Codes[pc] == 0xbd)
                return new ANewArray(cuframe);
            if (cuframe.Codes[pc] == 0xbe)
                return new ArrayLength(cuframe);
            if (cuframe.Codes[pc] == 0xc6)
                return new IfNull(cuframe);
            if (cuframe.Codes[pc] == 0xc7)
                return new IfNotNull(cuframe);
            return null;
        }

        public void InitClass(JavaClass cl)
        {
            cl.inited = true;
            var clinit = cl.Methods.FirstOrDefault(x => x.Name == "<clinit>");
            if (clinit != null)
            {
                this.FrameStack.Push(new Frame(clinit, this));
            }
            if (!cl.IsInterface() && cl.SuperClass != null && !cl.SuperClass.inited)
            {
                InitClass(cl.SuperClass);
            }
        }

        public void InvokeMethod(JavaMethod javaMethod)
        {
            //hack
            if (javaMethod.IsNative())
            {
                if (javaMethod.Name == "registerNatives")
                {
                    return;
                }
                var nativer = this.FrameStack.Peek().MethodInfo.Class.ClassLoader.GetNativeFunc(javaMethod.Class.ThisClassName,
                    javaMethod.Name, javaMethod.Descriptor);
                var argcounts = javaMethod.GetArgCount();
                var nf = new Frame(javaMethod, this);
                nf.LocalVars = new LocalVars(argcounts);
                for (int i = argcounts - 1; i >= 0; i--)
                {
                    nf.LocalVars.SetSlot(i, this.FrameStack.Peek().OPStack.Pop());
                }
                nativer.Work(nf, this);
                return;
            }

            var newFrame = new Frame(javaMethod, this);
            //复制参数
            var argcount = javaMethod.GetArgCount();
            for (int i = argcount - 1; i >= 0; i--)
            {
                newFrame.LocalVars.SetSlot(i, this.FrameStack.Peek().OPStack.Pop());
            }
            this.FrameStack.Push(newFrame);
        }
    }

    public class Frame
    {
        public JavaMethod MethodInfo;
        public LocalVars LocalVars;
        public Stack<Slot> OPStack;
        public JThread jthread;
        public byte[] Codes;
        public int FramePC;

        public Frame(JavaMethod method, JThread jthread)
        {
            this.jthread = jthread;
            this.MethodInfo = method;
            this.Codes = this.MethodInfo.Code;
            this.OPStack = new Stack<Slot>();
            this.LocalVars = new LocalVars(method.MaxLocal);
            this.FramePC = 0;
        }

        public void RevertPC()
        {
            this.FramePC = this.jthread.PC;
        }
    }

    public abstract class Instructions
    {
        protected Frame frame;

        public Instructions(Frame f)
        {
            this.frame = f;
        }
        public abstract int GetOp();
        public abstract void Execute();
    }

    public class GetStatic
        : Instructions
    {
        public GetStatic(Frame f) : base(f)
        {
        }

        private UInt16 index = 0;
        public override void Execute()
        {
            var fieldref = frame.MethodInfo.Class.RTCP.dic[index] as FieldRef;
            var field = fieldref.ResoverField();
            if(!field.IsStatic())
                throw new Exception();
            if (!field.Class.inited)
            {
                this.frame.jthread.InitClass(field.Class);
                this.frame.RevertPC();
                return;
            }
            //TODO 根据类型判断GET值和PUSH值
            var desc = field.Descriptor;
            switch (desc)
            {
                case "I":
                    var i = field.Class.StaticClassVars.GetInt(field.SoltIndex);
                    this.frame.OPStack.Push(new Slot(){ data = BitConverter.GetBytes(i)});
                    break;
                //默认情况为引用类型
                default:
                    var refobj = field.Class.StaticClassVars.GetObj(field.SoltIndex);
                    this.frame.OPStack.Push(new Slot() { refObj = refobj });
                    break;
            }
            
        }

        public override int GetOp()
        {
            this.index = BitConverter.ToUInt16(new byte[] {frame.Codes[frame.jthread.PC + 2], frame.Codes[frame.jthread.PC + 1]}, 0);
            return 2;
        }
    }

    public class PushStatic
        : Instructions
    {
        private UInt16 index = 0;
        private Slot slot;

        public PushStatic(Frame fram)
            : base(fram)
        {
        }

        public override void Execute()
        {
            var fieldref = frame.MethodInfo.Class.RTCP.dic[index] as FieldRef;
            var field = fieldref.ResoverField();
            if (!field.IsStatic())
                throw new Exception();
            if (field.Descriptor == "I")
            {
                field.Class.StaticClassVars.SetInt(field.SoltIndex, BitConverter.ToInt32(slot.data, 0));
                return;
            }
            if (field.Descriptor.StartsWith("["))
            {
                field.Class.StaticClassVars.SetObj(field.SoltIndex, this.slot.refObj);
                return;
            }
            if (field.Descriptor.StartsWith("L"))
            {
                field.Class.StaticClassVars.SetObj(field.SoltIndex, this.slot.refObj);
                return;
            }
            if (field.Descriptor == "Z")
            {
                field.Class.StaticClassVars.SetBool(field.SoltIndex, this.slot.data[0] == 0x01);
                return;
            }
            throw new Exception();
        }

        public override int GetOp()
        {
            this.index = BitConverter.ToUInt16(new byte[] { frame.Codes[frame.jthread.PC + 2], frame.Codes[frame.jthread.PC + 1] }, 0);
            slot = this.frame.OPStack.Pop();
            return 2;
        }
    }

    public class SiPush
        : Instructions
    {
        private short s;
        public SiPush(Frame fram)
            : base(fram)
        {
        }

        public override void Execute()
        {
            this.frame.OPStack.Push(new Slot(){ data = BitConverter.GetBytes((int)this.s)});
        }

        public override int GetOp()
        {
            this.s = BitConverter.ToInt16(new byte[] { this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1] }, 0);
            return 2;
        }
    }

    public class BiPush
        : Instructions
    {
        private byte opval;
        public BiPush(Frame fram)
            : base(fram)
        {
            
        }
        public override void Execute()
        {
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes((int)this.opval) });
        }

        public override int GetOp()
        {
            this.opval = this.frame.Codes[this.frame.jthread.PC + 1];
            return 1;
        }
    }

    public class Return
        : Instructions
    {
        public Return(Frame frame)
            : base(frame)
        {
            
        }
        public override void Execute()
        {
            var tempf = frame.jthread.FrameStack.Pop();
            if(tempf != frame)
                throw new Exception();
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class New
        : Instructions
    {
        private UInt16 index;
        public New(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            var classInfo = this.frame.MethodInfo.Class.RTCP.dic[this.index] as ClassRef;
            var cla = classInfo.ResoverClass();
            if (!cla.inited)
            {
                this.frame.jthread.InitClass(cla);
                this.frame.RevertPC();
                return;
            }
            JavaObject jobj = new JavaObject(cla);
            this.frame.OPStack.Push(new Slot(){ refObj = jobj });
        }

        public override int GetOp()
        {
            this.index = BitConverter.ToUInt16(new byte[] {this.frame.Codes[this.frame.FramePC + 2], this.frame.Codes[this.frame.FramePC + 1]}, 0);
            return 2;
        }
    }

    public class InvokeSpecial
        : Instructions
    {
        private UInt16 index;
        public InvokeSpecial(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            var constructor = this.frame.MethodInfo.Class.RTCP.dic[this.index] as MethodRef;
            var c = constructor.ResoverClass();
            var d = constructor.ResoverMethod();
            if (d.Name == "<init>" && c != d.Class)
            {
                throw new Exception();
            }
            if(d.IsStatcic())
                throw new Exception();
            //获取不到this对象.....因为函数参数在栈的上层
            //var thisobj = this.frame.OPStack.Pop().refObj;
            //todo 访问安全判断

            this.frame.jthread.InvokeMethod(d);
        }

        public override int GetOp()
        {
            this.index = BitConverter.ToUInt16(new byte[] { this.frame.Codes[this.frame.FramePC + 2], this.frame.Codes[this.frame.FramePC + 1] }, 0);
            return 2;
        }
    }

    public class InvokeStatic
        : Instructions
    {
        private UInt16 index;
        public InvokeStatic(Frame frame)
            : base(frame)
        {
            
        }

        public override void Execute()
        {
            var s = this.frame.MethodInfo.Class.RTCP.dic[index] as MethodRef;
            var c = s.ResoverClass();
            if (!c.inited)
            {
                this.frame.jthread.InitClass(c);
                this.frame.RevertPC();
                return;
            }
            var staticMethod = s.ResoverMethod();
            this.frame.jthread.InvokeMethod(staticMethod);
        }

        public override int GetOp()
        {
            this.index = BitConverter.ToUInt16(new byte[] { this.frame.Codes[this.frame.FramePC + 2], this.frame.Codes[this.frame.FramePC + 1] }, 0);
            return 2;
        }
    }

    public class Dup
        : Instructions
    {
        public Dup(Frame frame)
            : base(frame)
        {
            
        }

        public override void Execute()
        {
            var t = this.frame.OPStack.Peek();
            this.frame.OPStack.Push(new Slot(){ data = (byte[])t.data.Clone(), refObj = t.refObj});
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class ALoad_0
        : Instructions
    {
        public ALoad_0(Frame frame)
            : base(frame)
        {

        }

        public override void Execute()
        {
            var temp = this.frame.LocalVars.GetObj(0);
            this.frame.OPStack.Push(new Slot(){ refObj = temp});
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class ALoad_3
        : Instructions
    {
        public ALoad_3(Frame frame)
            : base(frame)
        {

        }

        public override void Execute()
        {
            var temp = this.frame.LocalVars.GetObj(3);
            this.frame.OPStack.Push(new Slot() { refObj = temp });
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class ILoad_0
        : Instructions
    {
        public ILoad_0(Frame frame)
            : base(frame)
        {
            
        }

        public override void Execute()
        {
            var temp = this.frame.LocalVars.GetInt(0);
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(temp) });
        }

        public override int GetOp()
        {
            return 0;
        }
    }
    public class ILoad_1
        : Instructions
    {
        public ILoad_1(Frame frame)
            : base(frame)
        {

        }

        public override void Execute()
        {
            var temp = this.frame.LocalVars.GetInt(1);
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(temp) });
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class ILoad_2
        : Instructions
    {
        public ILoad_2(Frame frame)
            : base(frame)
        {

        }

        public override void Execute()
        {
            var temp = this.frame.LocalVars.GetInt(2);
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(temp) });
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class ILoad_3
        : Instructions
    {
        public ILoad_3(Frame frame)
            : base(frame)
        {

        }

        public override void Execute()
        {
            var temp = this.frame.LocalVars.GetInt(3);
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(temp) });
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class ANewArray
        : Instructions
    {
        private UInt16 index;
        public ANewArray(Frame frame)
            : base(frame)
        {
            
        }
        public override void Execute()
        {
            var leng = BitConverter.ToUInt32(this.frame.OPStack.Pop().data, 0);
            var t = this.frame.MethodInfo.Class.RTCP.dic[index] as ClassRef;
            var arrTypeClass = t.ResoverClass();
            var arrClassName = "[L" + arrTypeClass.ThisClassName + ";";
            var arrClass = this.frame.MethodInfo.Class.ClassLoader.GetJavaClass(arrClassName);
            var d = new JavaObject(arrClass, (int)leng);
            this.frame.OPStack.Push(new Slot(){ refObj = d});

        }

        public override int GetOp()
        {
            this.index = BitConverter.ToUInt16(new byte[]{this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1]}, 0);
            return 2;
        }
    }

    public class NewArray
        : Instructions
    {
        private byte type;

        public NewArray(Frame frame)
            : base(frame)
        {
            
        }
        public override void Execute()
        {
            var classname = "[" + GetTypeName();
            var arrayClass = this.frame.MethodInfo.Class.ClassLoader.GetJavaClass(classname);
            var count = this.frame.OPStack.Pop();
            var cotval = BitConverter.ToUInt32(count.data, 0);
            var s = new Slot() {refObj = new JavaObject(arrayClass, (int)cotval)};
            this.frame.OPStack.Push(s);
        }

        private string GetTypeName()
        {
            switch (this.type)
            {
                case 0x4:
                    return "Z";
                case 0x05:
                    return "C";
                case 0x06:
                    return "F";
                case 0x07:
                    return "D";
                case 0x08:
                    return "B";
                case 0x09:
                    return "S";
                case 0x0a:
                    return "I";
                case 0x0b:
                    return "J";
            }
            throw new Exception();
        }

        public override int GetOp()
        {
            this.type = this.frame.Codes[this.frame.jthread.PC + 1];
            return 1;
        }
    }

    public class PushField
        : Instructions
    {
        private UInt16 index;
        public PushField(Frame frame)
            : base(frame)
        {
            
        }
        public override void Execute()
        {
            var value = this.frame.OPStack.Pop();
            var thisobject = this.frame.OPStack.Pop().refObj;
            var constantpool = this.frame.MethodInfo.Class.RTCP;
            var fr = constantpool.dic[index] as FieldRef;
            var field = fr.ResoverField();
            var objval = thisobject.Slots[field.SoltIndex];
            objval.refObj = value.refObj;
            objval.data = (byte[])value.data.Clone();
        }

        public override int GetOp()
        {
            this.index = BitConverter.ToUInt16(
                new byte[] {this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1]},
                0);
            return 2;
        }
    }

    public class GetField
        : Instructions
    {
        private UInt16 index;
        public GetField(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            var fieldRef = this.frame.MethodInfo.Class.RTCP.dic[this.index] as FieldRef;
            var instancefield = fieldRef.ResoverField();
            if(instancefield.IsStatic())
                throw new Exception();
            var instance = this.frame.OPStack.Pop().refObj;
            if(instance == null)
                throw new Exception();
            var solt = instance.Slots[instancefield.SoltIndex];
            this.frame.OPStack.Push(new Slot(){ data = (byte[])solt.data.Clone(), refObj = solt.refObj});
        }

        public override int GetOp()
        {
            this.index = BitConverter.ToUInt16(new byte[] { this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1] }, 0);
            return 2;
        }
    }

    public class Ldc
        : Instructions
    {
        private byte index; 
        public Ldc(Frame frame)
            : base(frame)
        {
            
        }
        public override void Execute()
        {
            var temp = this.frame.MethodInfo.Class.RTCP.dic[this.index];
            //Console.WriteLine(temp.GetType().Name);
            if (temp.GetType() == typeof(Literal<string>))
            {
                var strclass = this.frame.MethodInfo.Class.ClassLoader.GetJavaClass("java/lang/String");
                if (!strclass.inited)
                {
                    var strclinit = strclass.Methods.FirstOrDefault(x => x.Name == "<clinit>");
                    this.frame.jthread.InitClass(strclass);
                    this.frame.RevertPC();
                    return;
                }
                //todo string的clinit
                var field = strclass.Fields.FirstOrDefault(x => x.Name == "value" && x.Descriptor == "[C");
                var strobj = new JavaObject(strclass);
                var strbytearrays = Encoding.UTF8.GetBytes(((Literal<string>) temp).Value);
                var javacarrObj = new JavaObject(this.frame.MethodInfo.Class.ClassLoader.GetJavaClass("[C"), strbytearrays.Length);
                for (int i = 0; i < javacarrObj.Slots.Count(); i++)
                    javacarrObj.Slots[i].data[0] = strbytearrays[i];
                strobj.Slots[field.SoltIndex].refObj = javacarrObj;
                this.frame.OPStack.Push(new Slot(){ refObj = strobj });
                return;
            }
            if(temp.GetType() == typeof(Literal<int>))
            {
                var val = (Literal<int>)temp;
                var data = new Slot() { data = BitConverter.GetBytes(val.Value) };
                this.frame.OPStack.Push(data);
                return;
            }
            throw new Exception();
        }

        public override int GetOp()
        {
            this.index = this.frame.Codes[this.frame.FramePC + 1];
            return 1;
        }
    }

    public class IConstM1
        : Instructions
    {
        public IConstM1(Frame frame)
            : base(frame)
        {

        }
        public override void Execute()
        {
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(-1) });
        }

        public override int GetOp()
        {
            return 0;
        }
    }
    public class IConst_0
        : Instructions
    {
        public IConst_0(Frame frame)
            : base(frame)
        {
            
        }
        public override void Execute()
        {
            this.frame.OPStack.Push(new Slot(){ data = BitConverter.GetBytes(0)});
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class IConst_1
        : Instructions
    {
        public IConst_1(Frame frame)
            : base(frame)
        {

        }
        public override void Execute()
        {
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(1) });
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class IConst_2
        : Instructions
    {
        public IConst_2(Frame frame)
            : base(frame)
        {

        }
        public override void Execute()
        {
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(2) });
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class IConst_3
        : Instructions
    {
        public IConst_3(Frame frame)
            : base(frame)
        {

        }
        public override void Execute()
        {
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(3) });
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class IConst_4
        : Instructions
    {
        public IConst_4(Frame frame)
            : base(frame)
        {

        }
        public override void Execute()
        {
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(4) });
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class IConst_5
        : Instructions
    {
        public IConst_5(Frame frame)
            : base(frame)
        {

        }
        public override void Execute()
        {
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(5) });
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class AConst_NULL
        : Instructions
    {
        public AConst_NULL(Frame frame)
            : base(frame)
        {
            
        }
        public override void Execute()
        { 
            this.frame.OPStack.Push(new Slot(){ refObj = null});
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class InvokeVirtual
        : Instructions
    {
        private UInt16 index;
        public InvokeVirtual(Frame frame)
            : base(frame)
        {
            
        }
        public override void Execute()
        {
            var methodref = this.frame.MethodInfo.Class.RTCP.dic[this.index] as MethodRef;
            var method = methodref.ResoverMethod();
            //todo 访问级校验,methodlookup
            this.frame.jthread.InvokeMethod(method);
        }

        public override int GetOp()
        {
            this.index = BitConverter.ToUInt16(
                new byte[] {this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1]},
                0);
            return 2;
        }
    }

    public class ALoad_1
        : Instructions
    {
        public ALoad_1(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            var refval = this.frame.LocalVars.GetObj(1);
            this.frame.OPStack.Push(new Slot(){ refObj = refval});
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class IfNull
        : Instructions
    {
        private int offset;
        public IfNull(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            var val = this.frame.OPStack.Pop();
            if (val.refObj == null)
            {
                this.frame.FramePC = this.frame.jthread.PC + this.offset;
            }
        }

        public override int GetOp()
        {
            this.offset = (int)BitConverter.ToUInt16(
                new byte[] {this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1]},
                0);
            return 2;
        }
    }

    public class IfNotNull
        : Instructions
    {
        private int offset;
        public IfNotNull(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            var val = this.frame.OPStack.Pop();
            if (val.refObj != null)
            {
                this.frame.FramePC = this.frame.jthread.PC + this.offset;
            }
        }

        public override int GetOp()
        {
            this.offset = (int)BitConverter.ToUInt16(
                new byte[] { this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1] },
                0);
            return 2;
        }
    }

    public class Ifle
        : Instructions
    {
        private int offset;
        public Ifle(Frame frame)
            : base(frame)
        {
            
        }
        public override void Execute()
        {
            var d = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            if (d <= 0)
            {
                this.frame.FramePC = this.frame.jthread.PC + this.offset;
            }
        }

        public override int GetOp()
        {
            this.offset = (int)BitConverter.ToUInt16(
                new byte[] { this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1] },
                0);
            return 2;
        }
    }

    public class Ifge
        : Instructions
    {
        private int offset;
        public Ifge(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            var d = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            if (d >= 0)
            {
                this.frame.FramePC = this.frame.jthread.PC + this.offset;
            }
        }

        public override int GetOp()
        {
            this.offset = (int)BitConverter.ToUInt16(
                new byte[] { this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1] },
                0);
            return 2;
        }
    }

    public class Ificmplt
        : Instructions
    {
        private int offset;
        public Ificmplt(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            var right = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            var left = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            if (left < right)
            {
                this.frame.FramePC = this.frame.jthread.PC + this.offset;
            }
        }

        public override int GetOp()
        {
            this.offset = (int)BitConverter.ToUInt16(
                new byte[] { this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1] },
                0);
            return 2;
        }
    }

    public class Ificmple
        : Instructions
    {
        private int offset;
        public Ificmple(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            var right = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            var left = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            if (left <= right)
            {
                this.frame.FramePC = this.frame.jthread.PC + this.offset;
            }
        }

        public override int GetOp()
        {
            this.offset = (int)BitConverter.ToUInt16(
                new byte[] { this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1] },
                0);
            return 2;
        }
    }

    public class Ificmpgt
        : Instructions
    {
        private int offset;
        public Ificmpgt(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            var right = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            var left = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            if (left > right)
            {
                this.frame.FramePC = this.frame.jthread.PC + this.offset;
            }
        }

        public override int GetOp()
        {
            this.offset = (int)BitConverter.ToUInt16(
                new byte[] { this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1] },
                0);
            return 2;
        }
    }

    public class Ificmpne
        : Instructions
    {
        private int offset;
        public Ificmpne(Frame frame)
            : base(frame) { }
        public override void Execute()
        {
            var right = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            var left = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            if (left != right)
            {
                this.frame.FramePC = this.frame.jthread.PC + this.offset;
            }
        }

        public override int GetOp()
        {
            this.offset = (int)BitConverter.ToUInt16(
                new byte[] { this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1] },
                0);
            return 2;
        }
    }
    public class ArrayLength
        : Instructions
    {
        public ArrayLength(Frame frame)
            : base(frame)
        {

        }

        public override void Execute()
        {
            var t = this.frame.OPStack.Pop();
            var jarray = t.refObj;
            this.frame.OPStack.Push(new Slot(){ data = BitConverter.GetBytes(jarray.Slots.Length)});
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class IReturn
        : Instructions
    {
        public IReturn(Frame frame)
            : base(frame)
        {

        }

        public override void Execute()
        {
            var d = this.frame.OPStack.Pop().data;
            this.frame.jthread.FrameStack.Pop();
            this.frame.jthread.FrameStack.Peek().OPStack.Push(new Slot(){ data = (byte[])d.Clone()});
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class IStore
        : InvokeStatic
    {
        private int index;
        public IStore(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            this.frame.LocalVars.SetInt(index, BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0));
        }

        public override int GetOp()
        {
            this.index = frame.Codes[frame.jthread.PC + 1];
            return 1;
        }
    }

    public class IStore_1
        : Instructions
    {
        public IStore_1(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            this.frame.LocalVars.SetInt(1, BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0));
        }

        public override int GetOp()
        {
            return 0;
        }
    }
    public class IStore_2
        : Instructions
    {
        public IStore_2(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            this.frame.LocalVars.SetInt(2, BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0));
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class IStore_3
        : Instructions
    {
        public IStore_3(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            this.frame.LocalVars.SetInt(3, BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0));
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class IMul
        : Instructions
    {
        public IMul(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            var v2 = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            var v1 = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            var resu = v1 * v2;
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(resu) });
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class IAdd
        : Instructions
    {
        public IAdd(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            var v2 = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            var v1 = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            var resu = v1 + v2;
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(resu) } );
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class ISub
        : Instructions
    {
        public ISub(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            var v2 = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            var v1 = BitConverter.ToInt32(this.frame.OPStack.Pop().data, 0);
            var resu = v1 - v2;
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(resu) });
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class ILoad
        : Instructions
    {
        private byte index;
        public ILoad(Frame frame)
            : base(frame)
        {
            
        }
        public override void Execute()
        {
            var temp = this.frame.LocalVars.GetInt(this.index);
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(temp) });
        }

        public override int GetOp()
        {
            this.index = this.frame.Codes[this.frame.jthread.PC + 1];
            return 1;
        }
    }
    public class Pop
        : Instructions
    {

        public Pop(Frame frame)
            : base(frame) { }
        public override void Execute()
        {
            this.frame.OPStack.Pop();
        }

        public override int GetOp()
        {
            return 0;
        }
    }
    public class AReturn
        : Instructions
    {
        public AReturn(Frame frame)
            : base(frame)
        {

        }
        public override void Execute()
        {
            var curf = this.frame.jthread.FrameStack.Pop();
            if (curf != this.frame)
                throw new Exception();
            var temp = curf.OPStack.Pop();
            this.frame.jthread.FrameStack.Peek().OPStack.Push(new Slot() { refObj = temp.refObj });
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class Castore
        : Instructions
    {
        public Castore(Frame frame) 
            : base(frame)
        {

        }

        public override void Execute()
        {
            var val = this.frame.OPStack.Pop().data[0];
            var index = BitConverter.ToInt32(frame.OPStack.Pop().data, 0);
            var array = frame.OPStack.Pop().refObj;
            array.Slots[index].data[0] = val;
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class Iastore
        : Instructions
    {
        public Iastore(Frame frame)
            : base(frame)
        {
            
        }
        public override void Execute()
        {
            var val = (byte[])this.frame.OPStack.Pop().data.Clone();
            var index = BitConverter.ToInt32(frame.OPStack.Pop().data, 0);
            var array = frame.OPStack.Pop().refObj;
            array.Slots[index].data = val;
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class Iaload
        : Instructions
    {
        public Iaload(Frame frame)
            : base(frame)
        {

        }
        public override void Execute()
        {
            var index = BitConverter.ToInt32(frame.OPStack.Pop().data, 0);
            var array = frame.OPStack.Pop().refObj;
            var val = (byte[])array.Slots[index].data.Clone();
            frame.OPStack.Push(new Slot(){ data = val});
        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public class Iinc
        : Instructions
    {
        private int index;
        public int consti;
        public Iinc(Frame frame)
            : base(frame)
        {
            
        }
        public override void Execute()
        {
            var val = this.frame.LocalVars.GetInt(index);
            val += consti;
            this.frame.LocalVars.SetInt(index, val);
        }

        public override int GetOp()
        {
            index = this.frame.Codes[this.frame.jthread.PC + 1];
            consti = this.frame.Codes[this.frame.jthread.PC + 2];
            return 2;
        }
    }

    public class Goto
        : Instructions
    {
        private int offset;
        public Goto(Frame frame)
            : base(frame)
        {
        }

        public override void Execute()
        {
            this.frame.FramePC = this.frame.jthread.PC + this.offset;
        }

        public override int GetOp()
        {
            this.offset = (int)BitConverter.ToInt16(
                new byte[] { this.frame.Codes[this.frame.jthread.PC + 2], this.frame.Codes[this.frame.jthread.PC + 1] },
                0);
            return 2;
        }
    }
}
