using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrookshearMachineCodeGen
{
    public static class MicroInstructions
    {
        public static byte GetNibble(ushort CIR, int index)
        {
            if (index == 0)
            {
                return (byte)(CIR >> 12);
            }
            if (index == 1)
            {
                return (byte)((CIR >> 8) & 0x0f);
            }
            if (index == 2)
            {
                return (byte)((CIR >> 4) & 0x00f);
            }
            if (index == 3) 
            {
                return (byte)(CIR & 0x000f);
            }
            return 0;
        }

        public static byte GetByte(ushort CIR, int index)
        {
            if (index == 0)
            {
                return (byte)(CIR >> 8);
            }
            if (index == 1)
            {
                return (byte)(CIR & 0x00ff);
            }
            return 0;
        }

        public static Func<byte,byte,bool> GetTest(byte test_flag)
        {
            switch (test_flag)
            {
                case 0:
                    return Test_EQ;
                case 1:
                    return Test_NE;
                case 2:
                    return Test_GE;
                case 3:
                    return Test_LE;
                case 4:
                    return Test_GT;
                case 5:
                    return Test_LT;
            }
            return Test_NONE;
        }

        public static bool Test_EQ(byte a, byte b)
        {
            return a == b;
        }
        public static bool Test_NE(byte a, byte b)
        {
            return a != b;
        }
        public static bool Test_GE(byte a, byte b)
        {
            return a >= b;
        }
        public static bool Test_LE(byte a, byte b)
        {
            return a <= b;
        }
        public static bool Test_GT(byte a, byte b)
        {
            return a > b;
        }
        public static bool Test_LT(byte a, byte b)
        {
            return a < b;
        }
        private static bool Test_NONE(byte a, byte b)
        {
            return false;
        }
    }
}
