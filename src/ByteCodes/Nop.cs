using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MikaJvm.JavaRTMethodArea;

namespace MikaJvm.ByteCodes
{
    [ByteCodeTag(0x00)]
    public class Nop
        : Instructions
    {
        public Nop(Frame frame)
            : base(frame)
        {
            
        }
        public override void Execute()
        {

        }

        public override int GetOp()
        {
            return 0;
        }
    }

    public abstract class Const<T>
        : Instructions
        where T : struct
    {
        protected T V;
        public Const(Frame frame, T v)
            : base(frame)
        {
            this.V = v;
        }
    }
    public class IConst
        : Const<int>
    {
        public IConst(Frame frame, int i)
            : base(frame, i)
        {
        }

        public override void Execute()
        {
            this.frame.OPStack.Push(new Slot() { data = BitConverter.GetBytes(this.V) });
        }

        public override int GetOp()
        {
            return 0;
        }
    }
}
