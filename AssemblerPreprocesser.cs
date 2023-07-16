using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BrookshearMachineCodeGen
{
    public class AssemblerPreProcesser
    {
        public string[] asmText { get; private set; }
        Dictionary<string, byte> labels = new Dictionary<string, byte>();
        int DEBUG_STARTLINES;

        public AssemblerPreProcesser() 
        {
            asmText = new string[1];
        }
        public void Process( string fileLoc)
        {
            asmText = File.ReadAllLines(fileLoc);
            DEBUG_STARTLINES = asmText.Length;

            //remove all useless information from the assembly file
            RemoveWhitespace();
            RemoveComments();
            RemoveEmptyLines();
            //find labels to replace in the code
            FindAndRemoveLabelDefinitions();
            ReplaceLabelUse();


        }

        private void RemoveWhitespace()
        {
            for (int i = 0; i < asmText.Length; i++)
            {
                asmText[i] = String.Concat(asmText[i].Where(c => !Char.IsWhiteSpace(c)));
            }
        }

        private void RemoveComments()
        {

            //remove comments
            for (int i = 0; i < asmText.Length; i++)
            {
                string line = asmText[i];
                for (int j = 0; j < line.Length - 1; j++)
                {
                    if (line[j] == '/' && line[j + 1] == '/')
                    {
                        asmText[i] = asmText[i].Remove(j);
                    }
                }
            }
        }

       private void RemoveEmptyLines()
        {
            List<string> lines = new List<string>();
            foreach (string line in asmText)
            {
                if (line.Length > 0)
                {
                    lines.Add(line);
                }
            }
            asmText = lines.ToArray();
        }

        private void FindAndRemoveLabelDefinitions()
        {
            labels = new Dictionary<string, byte>();
            for( int i = 0; i < asmText.Length; i++)
            {
                string line = asmText[i];
                int index = line.IndexOf(':');
                if (index == -1)
                {
                    continue;
                }
                labels.Add(line.Substring(0, index),(byte)(i * 2));
                asmText[i] = line.Substring(index + 1);
            }

        }

        private void ReplaceLabelUse()
        {
            
            for (int i = 0; i < asmText.Length; i++)
            {
                string line = asmText[i];
                foreach (var label in labels)
                {
                    if (line.Contains(label.Key))
                    {
                        asmText[i] = line.Replace(label.Key,label.Value.ToString("X2"));
                        break;
                    }
                }
            }
        }

        public void DEBUG_printLabels()
        {
            Console.WriteLine("---------------------------------------");
            foreach (var label in labels)
            {
                Console.WriteLine($"{label.Key}- {label.Value.ToString("X2")}");
            }
        }

        public void DEBUG_Print()
        {
            foreach (string line in asmText)
            {
                Console.WriteLine(line);
            }

            //Console.WriteLine("---------------------------------------");
            //Console.WriteLine($"lines at start: {DEBUG_STARTLINES}");
            //Console.WriteLine($"lines at end  : {asmText.Length}");
            Console.WriteLine("---------------------------------------");
            foreach (var label in labels)
            {
                Console.WriteLine($"{label.Key}- {label.Value.ToString("X2")}");
            }
        }
    }
}
