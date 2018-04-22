using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaRTMethodArea
{
    public class JavaObject
    {
        public JavaClass ObjClass;
        public Slot[] Slots;
        //反射用,指向反射对象的Method Area
        public JavaClass Extension;

        public JavaObject(JavaClass classref)
        {
            this.ObjClass = classref;
            this.Slots = new Slot[classref.InstanceSoltCount];
            for(int i = 0; i < Slots.Length; i++)
                Slots[i] = new Slot();
        }
        //创建数组使用
        public JavaObject(JavaClass classref, int length)
        {
            this.ObjClass = classref;
            this.Slots = new Slot[length];
            for(int i = 0; i < Slots.Length; i++)
                Slots[i] = new Slot();
        }
    }
}
