using BrookshearMachineCodeGen;
using System.Reflection;

string fileLoc = "./asm.txt";

//ushort test = 0xABCD;
//Console.WriteLine(MicroInstructions.GetByte(test, 1).ToString("X2"));

AssemblerPreProcesser preprocessor = new();
preprocessor.Process(fileLoc);


Assembler assembler = new();
assembler.Assemble(preprocessor.asmText);

//preprocessor.DEBUG_Print();
//Console.WriteLine("---------------------------------");
//assembler.DEBUG_printMachineCode();
assembler.DEBUG_OUTPUT_LIKE_BMMACHINE(5);
Console.WriteLine("---------------------------------");

Machine machine = new();
machine.Memory = assembler.GetMachineCode();
machine.Step();
machine.Step();
//Console.Write("PC: ");
//

machine.DEBUG_OUTPUT_MEM_LIKE_BMMACHINE(5);

