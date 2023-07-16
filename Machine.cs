using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrookshearMachineCodeGen
{
    public class Machine
    {
        public byte[] Memory;
        public byte[] Registers;
        public byte PC = 0;
        public ushort CIR;

        public bool halted = false;

        public Action Instruction;

        public Machine() 
        {
            Memory = new byte[256];
            Registers = new byte[16];
        }

        public void DEBUG_OUTPUT_MEM_LIKE_BMMACHINE(int limit = 128)
        {
            for (int i = 0; i < limit; i++)
            {
                //Console.WriteLine(i);
                Console.WriteLine($"{(i * 2).ToString("X2")}: {Memory[i * 2].ToString("X2")}{Memory[i * 2 + 1].ToString("X2")}");
            }
        }

        public void Step()
        {
            Fetch();
            Decode();
            Execute();
        }

        public void Fetch()
        {
            CIR = (ushort)((Memory[PC]) << 8);
            CIR += Memory[PC + 1];
            //Console.WriteLine(CIR.ToString("X4"));
            PC += 2;   
        }
        public void Decode()
        {
            byte instruction = MicroInstructions.GetNibble(CIR, 0);
            //Console.WriteLine(instruction.ToString("X2"));
            switch (instruction)
            {
                case 0x00:
                    Instruction = NOP;
                    break;
                case 0x01:
                    Instruction = LoadDirect;
                    break;
                case 0x02:
                    Instruction = LoadImmediate;
                    break;
                case 0x03:
                    Instruction = StoreDirect;
                    break;
                case 0x04:
                    Instruction = Move;
                    break;
                case 0x05:
                    Instruction = ADDI;
                    break;
                case 0x06:
                    Instruction = ADDF;
                    break;
                case 0x07:
                    Instruction = OR;
                    break;
                case 0x08:
                    Instruction = AND;
                    break;
                case 0x09:
                    Instruction = XOR;
                    break;
                case 0x0A:
                    Instruction = ROT;
                    break;
                case 0x0B:
                    Instruction = JUMPEQ;
                    break;
                case 0x0C:
                    Instruction = HALT;
                    break;
                case 0x0D:
                    Instruction = LOAD_RegIndirect;
                    break;
                case 0x0E:
                    Instruction = STORE_RegIndirect;
                    break;
                case 0x0F:
                    Instruction = JUMP_TEST;
                    break;
            }
            //Console.WriteLine(Instruction.Method.Name);
        }

        public void Execute()
        {
            //Console.WriteLine($"EXECUTING INSTRUCTION {Instruction.Method.Name}");
            Instruction();
        }

        private void NOP() //0x00
        {
            return;
        }
        private void LoadDirect() //0x01
        {
            byte r = MicroInstructions.GetNibble(CIR, 1);
            byte address = MicroInstructions.GetByte(CIR, 1);
            Registers[r] = Memory[address];
        }
        private void LoadImmediate() //0x02
        {
            byte r = MicroInstructions.GetNibble(CIR, 1); //0x0r00 to r
            byte data = MicroInstructions.GetByte(CIR, 1); //0x00xy to xy
            Registers[r] = data; 
        }
        private void StoreDirect() //0x03
        {
            byte r = MicroInstructions.GetNibble(CIR, 1);
            byte address = MicroInstructions.GetByte(CIR, 1); 
            Memory[address] = Registers[r];
        }
        private void Move() //0x04
        {
            byte target = MicroInstructions.GetNibble(CIR, 2);
            byte store = MicroInstructions.GetNibble(CIR, 3);
            // Console.WriteLine($"target = {target.ToString("X2")}, store = {store.ToString("X2")}");
            Registers[store] = Registers[target];
        }
        private void ADDI() //0x05
        {
            byte add_reg_1 = MicroInstructions.GetNibble(CIR, 2);
            byte add_reg_2 = MicroInstructions.GetNibble(CIR, 3);
            byte store_reg = MicroInstructions.GetNibble(CIR, 1);
            Registers[store_reg] = (byte)(Registers[add_reg_1] + Registers[add_reg_2]);
        }
        private void ADDF() //0x06
        {
            byte add_reg_1 = MicroInstructions.GetNibble(CIR, 2);
            byte add_reg_2 = MicroInstructions.GetNibble(CIR, 3);
            byte store_reg = MicroInstructions.GetNibble(CIR, 1);
            //TODO: RETURN
        }
        private void OR() //0x07
        {
            byte or_reg_1 = MicroInstructions.GetNibble(CIR, 2);
            byte or_reg_2 = MicroInstructions.GetNibble(CIR, 3);
            byte store_reg = MicroInstructions.GetNibble(CIR, 1);
            Registers[store_reg] = (byte)(Registers[or_reg_1] | Registers[or_reg_2]);
        }
        private void AND() //0x08
        {
            byte and_reg_1 = MicroInstructions.GetNibble(CIR, 2);
            byte and_reg_2 = MicroInstructions.GetNibble(CIR, 3);
            byte store_reg = MicroInstructions.GetNibble(CIR, 1);
            Registers[store_reg] = (byte)(Registers[and_reg_1] & Registers[and_reg_2]);
        }
        private void XOR() //0x09
        {
            byte xor_reg_1 = MicroInstructions.GetNibble(CIR, 2);
            byte xor_reg_2 = MicroInstructions.GetNibble(CIR, 3);
            byte store_reg = MicroInstructions.GetNibble(CIR, 1);
            Registers[store_reg] = (byte)(Registers[xor_reg_1] ^ Registers[xor_reg_2]);
        }
    
        private void ROT() //0x0A
        {
            byte target_reg = MicroInstructions.GetNibble(CIR, 1);
            byte rot_amount = MicroInstructions.GetNibble(CIR, 3);
            byte data = Registers[target_reg];
            Registers[target_reg] = (byte)((data >> rot_amount) | (data << (8 - rot_amount)));
        }
        private void JUMPEQ() //0x0B
        {
            byte compare_reg = MicroInstructions.GetNibble(CIR, 1);
            byte jump_address = MicroInstructions.GetByte(CIR, 1);
            if (Registers[compare_reg] == Registers[0])
            {
                
                PC = jump_address;
            }
        }
        private void HALT() //0x0C
        {
            halted = true;
        }
        private void LOAD_RegIndirect() //0x0D
        {
            byte store_register = MicroInstructions.GetNibble(CIR, 2);
            byte address_register = MicroInstructions.GetNibble(CIR, 3);
            Registers[store_register] = Memory[Registers[address_register]];
        }
        private void STORE_RegIndirect() //0x0E
        {
            byte data_register = MicroInstructions.GetNibble(CIR, 2);
            byte address_register = MicroInstructions.GetNibble(CIR, 3);
            Memory[Registers[address_register]] = Registers[data_register];
        }
        private void JUMP_TEST() //0x0F
        {
            byte compare_register = MicroInstructions.GetNibble(CIR, 1);
            byte jump_register = MicroInstructions.GetNibble(CIR, 3);
            byte test_flag = MicroInstructions.GetNibble(CIR, 2);
            if (MicroInstructions.GetTest(test_flag)(Registers[compare_register], Registers[0]))
            {
                PC = Registers[jump_register];
            }

        }
    }
}

