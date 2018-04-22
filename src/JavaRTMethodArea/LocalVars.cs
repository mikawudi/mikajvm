using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikaJvm.JavaRTMethodArea
{
    public class LocalVars
    {
        private Slot[] SoltArray;
        public LocalVars(int length)
        {
            SoltArray = new Slot[length];
            for(int i = 0; i < length; i++)
                SoltArray[i] = new Slot();
        }

        public void SetByte(int index, byte b)
        {
            SoltArray[index].data[0] = b;
        }

        public byte GetByte(int index)
        {
            return SoltArray[index].data[0];
        }

        public void SetChar(int index, char c)
        {
            var b = (byte) c;
            SoltArray[index].data[0] = b;
        }

        public char GetChar(int index)
        {
            return (char)SoltArray[index].data[0];
        }

        public void SetBool(int index, bool b)
        {
            SoltArray[index].data[0] = (byte)(b ? 0x01 : 0x00);
        }

        public bool GetBool(int index)
        {
            return SoltArray[index].data[0] == 0x01;
        }

        public void SetInt(int index, int data)
        {
            var d = BitConverter.GetBytes(data);
            var solt = SoltArray[index];
            d.CopyTo(solt.data, 0);
        }

        public int GetInt(int index)
        {
            return BitConverter.ToInt32(SoltArray[index].data, 0);
        }

        public void SetFloat(int index, float data)
        {
            var d = BitConverter.GetBytes(data);
            d.CopyTo(SoltArray[index].data, 0);
        }

        public float GetFloat(int index)
        {
            return BitConverter.ToSingle(SoltArray[index].data, 0);
        }

        public void SetObj(int index, JavaObject obj)
        {
            SoltArray[index].refObj = obj;
        }

        public JavaObject GetObj(int index)
        {
            return SoltArray[index].refObj;
        }

        public void SetShort(int index, short data)
        {
            var d = BitConverter.GetBytes(data);
            d.CopyTo(SoltArray[index].data, 0);
        }

        public short GetShort(int index)
        {
            return BitConverter.ToInt16(SoltArray[index].data, 0);
        }

        public void SetLong(int index, long data)
        {
            var d = BitConverter.GetBytes(data);
            var s1 = SoltArray[index];
            var s2 = SoltArray[index + 1];
            for (int i = 0; i < 4; i++)
            {
                s1.data[i] = d[i];
                s2.data[i] = d[i + 4];
            }
        }

        public long GetLong(int index)
        {
            var s1 = SoltArray[index];
            var s2 = SoltArray[index + 1];
            var temp = new byte[8];
            for (int i = 0; i < 4; i++)
            {
                temp[i] = s1.data[i];
                temp[i + 4] = s2.data[i];
            }
            return BitConverter.ToInt64(temp, 0);
        }

        public void Setdouble(int index, double data)
        {
            var d = BitConverter.GetBytes(data);
            var s1 = SoltArray[index];
            var s2 = SoltArray[index + 1];
            for (int i = 0; i < 4; i++)
            {
                s1.data[i] = d[i];
                s2.data[i] = d[i + 4];
            }
        }

        public double GetDouble(int index)
        {
            var s1 = SoltArray[index];
            var s2 = SoltArray[index + 1];
            var temp = new byte[8];
            for (int i = 0; i < 4; i++)
            {
                temp[i] = s1.data[i];
                temp[i + 4] = s2.data[i];
            }
            return BitConverter.ToDouble(temp, 0);
        }

        public void SetSlot(int index, Slot slot)
        {
            SoltArray[index].data = (byte[]) slot.data.Clone();
            SoltArray[index].refObj = slot.refObj;
        }
    }
}
