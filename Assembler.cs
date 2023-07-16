using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BrookshearMachineCodeGen
{
    public class Assembler
    {
        string[] asmText;
        byte[] machineCode;

        public Assembler()
        {
            machineCode = new byte[256];
        }

        public void Assemble(string[] asmTxt)
        {
            asmText = asmTxt;
            WriteInstruction();

        }

        private void WriteInstruction()
        {
            int j = 0;
            for (int i = 0; i < asmText.Length; i++)
            {
                byte opcode = 0;
                byte operand = 0;

                switch (asmText[i])
                {
                    case string line when line.Contains("DATA"):
                        opcode = Instr_DATA(line);
                        operand = 0x00;
                        break;
                    case string line when line.Contains("HALT"):
                        opcode = 0xC0;
                        operand = 0x00;
                        break;
                    case string line when line.Contains("ADDF"):
                        (opcode, operand) = Instr_ADD(line, "F");
                        break;
                    case string line when line.Contains("ADDI"):
                        (opcode, operand) = Instr_ADD(line, "I");
                        break;
                    case string line when line.Contains("NOP"):
                        opcode = 0x0F;
                        operand = 0xFF;
                        break;
                    case string line when line.Contains("JMPLT"):
                        (opcode, operand) = Instr_JUMP_CONDITIONAL(line, "LT");
                        break;
                    case string line when line.Contains("JMPGT"):
                        (opcode, operand) = Instr_JUMP_CONDITIONAL(line, "GT");
                        break;
                    case string line when line.Contains("JMPLE"):
                        (opcode, operand) = Instr_JUMP_CONDITIONAL(line, "LE");
                        break;
                    case string line when line.Contains("JMPGE"):
                        (opcode, operand) = Instr_JUMP_CONDITIONAL(line, "GE");
                        break;
                    case string line when line.Contains("JMPNE"):
                        (opcode, operand) = Instr_JUMP_CONDITIONAL(line, "NE");
                        break;
                    case string line when line.Contains("JMPEQ"):
                        (opcode, operand) = Instr_JUMPEQ(line);
                        break;
                    case string line when line.Contains("JMP"):
                        (opcode, operand) = Instr_JUMP(line);
                        break;
                    case string line when line.Contains("XOR"):
                        (opcode, operand) = Instr_LOGICAL(line, "XOR");
                        break;
                    case string line when line.Contains("AND"):
                        (opcode, operand) = Instr_LOGICAL(line, "AND");
                        break;
                    case string line when line.Contains("ROT"):
                        (opcode, operand) = Instr_ROT(line);
                        break;
                    case string line when line.Contains("MOV"):
                        (opcode, operand) = Instr_MOV(line);
                        break;
                    case string line when line.Contains("OR"):
                        (opcode, operand) = Instr_LOGICAL(line, "OR");
                        break;
                }

                machineCode[j] = opcode;
                machineCode[j + 1] = operand;
                j += 2;
            }
        }

        private (byte, byte) Instr_ADD(string line, string mode)
        {
            byte opcode = 0x00;
            byte operand = 0x00;
            //get first digit of opcode based on if the addition is int or float
            if (mode == "I")
            {
                opcode += 0x50;
            }
            else if (mode == "F")
            {
                opcode += 0x60;
            }
            else
            {
                throw new ArgumentException("Mode must be either F or I");
            }

            line = line.Replace("ADD" + mode, "");
            string[] split = line.Split(',');
            //get registers to be added as a string
            string reg_add_1 = split[0].Substring(1);
            string reg_add_2 = split[1].Split("->")[0].Substring(1);
            //get register to be sent to as a string
            string reg_dest = split[1].Split("->")[1].Substring(1);

            //second opcode argument is register to be sent to
            opcode += (byte)Convert.FromHexString("0" + reg_dest)[0];
            //operand is (reg_add_1)(reg_add_2)
            operand = (byte)Convert.FromHexString(reg_add_1 + reg_add_2)[0];

            return (opcode, operand);
        }

        private (byte, byte) Instr_LOGICAL(string line, string mode)
        {
            byte opcode = 0x00;
            byte operand = 0x00;

            if (mode == "OR")
            {
                opcode += 0x70;
            }
            else if (mode == "AND")
            {
                opcode += 0x80;
            }
            else if (mode == "XOR")
            {
                opcode += 0x90;
            }
            else
            {
                throw new ArgumentException("Mode must be either OR,AND, or XOR");
            }

            line = line.Replace(mode, "");
            string[] split = line.Split(',');
            //get registers to have operation carried out on as string
            string reg_log_1 = split[0].Substring(1);
            string reg_log_2 = split[1].Split("->")[0].Substring(1);
            //get register to be sent to as a string
            string reg_dest = split[1].Split("->")[1].Substring(1);

            //second opcode argument is register to be sent to
            opcode += (byte)Convert.FromHexString("0" + reg_dest)[0];
            //operand is (reg_add_1)(reg_add_2)
            operand = (byte)Convert.FromHexString(reg_log_1 + reg_log_2)[0];

            return (opcode, operand);

        }


        private (byte, byte) Instr_JUMP_CONDITIONAL(string line, string mode)
        {
            byte opcode = 0xF0;
            byte operand = 0x00;


            if (mode == "LT")
            {
                operand = 0x50;
            }
            else if (mode == "GT")
            {
                operand = 0x40;
            }
            else if (mode == "LE")
            {
                operand= 0x30;
            }
            else if (mode == "GE")
            {
                operand = 0x20;
            }
            else if (mode == "NE")
            {
                operand =  0x10;
            }
            else
            {
                throw new ArgumentException("Mode must be LT, GT, GE, LE, NE");
            }

            line = line.Replace($"JMP{mode}R", "");
            string[] arguments = line.Split(",R");
            string addr_reg = arguments[0];
            string comp_reg = arguments[1];
            opcode += (byte)Convert.FromHexString("0" + comp_reg)[0];
            operand += (byte)Convert.FromHexString("0" + addr_reg)[0];

            return (opcode, operand);

        }

        private (byte, byte) Instr_JUMPEQ(string line)
        {
            //TODO: add in opcodes B and F
            byte opcode = 0x00;
            byte operand = 0x00;
            //TODO: add in opcode B
            if (line.Contains("JMPEQR"))
            {
                opcode = 0xF0;
                operand = 0x00;
                line = line.Replace("JMPEQR", "");
                string[] arguments = line.Split(",R");
                string addr_reg = arguments[0];
                string comp_reg = arguments[1];
                opcode += (byte)Convert.FromHexString("0" + comp_reg)[0];
                operand += (byte)Convert.FromHexString("0" + addr_reg)[0];
            }
            else
            {
                opcode = 0xB0;
                line = line.Replace("JMPEQ", "");
                string[] arguments = line.Split(",R");
                string address = arguments[0];
                string register = arguments[1];
                opcode += (byte)Convert.FromHexString("0"+register)[0];

                operand = (byte)Convert.FromHexString(address)[0];
            }
            return (opcode, operand);

        }
        private (byte, byte) Instr_JUMP(string line)
        {
            byte opcode = 0x00;
            byte operand = 0x00;
            //TODO: add in opcode B
            if (line.Contains("JMPR"))
            {
                opcode = 0xF0;
                line = line.Replace("JMPR", "");
                operand = (byte)Convert.FromHexString("0" + line)[0];
                
            }
            else
            {
                opcode = 0xB0;
                line = line.Replace("JMP", "");
                operand = (byte)Convert.FromHexString(line)[0];
            }
            return (opcode, operand);
        }

        private (byte, byte) Instr_ROT(string line)
        {
            byte opcode = 0xA0;
            byte operand = 0x00;
            line = line.Replace("ROT", "");
            
            string reg = line.Split(',')[0].Substring(1);
            string rotations =  line.Split(',')[1];
            //TODO: allow integer arguements here instead of just hex
            opcode += Convert.FromHexString("0" + reg)[0];
            operand += Convert.FromHexString("0" + rotations)[0];

            return (opcode, operand);
        }

        private (byte, byte) Instr_MOV(string line)
        {
            byte opcode = 0x00;
            byte operand = 0x00;
            
            line = line.Replace("MOV", "");
            string[] arguments = line.Split("->");
            
            string source = arguments[0];
            string destination = arguments[1];

            if (source.StartsWith('R')) // MOV Rm ->
            {
                source = source.Substring(1);
                if (destination.StartsWith("R")) //Rm -> Rn
                {
                    opcode = 0x40;
                    operand = Convert.FromHexString(source + "0")[0];
                    operand += Convert.FromHexString("0" + destination.Substring(1))[0];
                }
                else if (destination.StartsWith("[R")) // Rm -> [Rn]
                {
                    opcode = 0xE0;
                    operand = Convert.FromHexString(source + "0")[0];
                    operand += Convert.FromHexString("0" + destination.Substring(2,1))[0];
                }
                else // Rm -> [xy]
                {
                    //TODO: Allow non hex values
                    opcode = 0x30;
                    opcode += Convert.FromHexString("0" + source)[0];

                    destination = destination.Replace("[","").Replace("]", "");
                    operand = Convert.ToByte(destination, 16);

                }
            }
            else if (source.StartsWith("[R")) // MOV [Rm] -> Rn
            {
                opcode = 0xD0;
                operand = Convert.FromHexString(destination.Substring(1) + "0")[0];
                operand += Convert.FromHexString("0" + source.Substring(2, 1))[0];
            }
            else if (source.StartsWith('[')) //MOV [xy] -> Rn
            {
                //TODO: Allow non hex values
                opcode = 0x10;
                opcode += Convert.FromHexString("0" + destination.Substring(1))[0];

                operand = Convert.ToByte(source.Substring(1, 2), 16);
            }
            else // MOV value -> Rn
            {
                //TODO: Allow non hex values
                opcode = 0x20;
                opcode += Convert.FromHexString("0" + destination.Substring(1))[0];

                operand = Convert.ToByte(source, 16);
            }

            return (opcode, operand);
        }

        private byte Instr_DATA(string line)
        { 
            //TODO: allow different types of data
            line = line.Replace("DATA", "");
            return (byte)Convert.FromHexString(line)[0];
        }

        public void DEBUG_printMachineCode()
        {
            for (int i = 0; i < 256; i++)
            {
                Console.WriteLine($"{i.ToString("D3")}: 0x{machineCode[i].ToString("X2")}");
            }
        }

        public void DEBUG_OUTPUT_LIKE_BMMACHINE(int limit = 128)
        {
            for (int i = 0; i < limit; i++)
            {
                //Console.WriteLine(i);
                Console.WriteLine($"{(i * 2).ToString("X2")}: {machineCode[i * 2].ToString("X2")}{machineCode[i * 2 + 1].ToString("X2")}");
            }
        }
        public byte[] GetMachineCode()
        {
            return machineCode;
        }
    }
}
