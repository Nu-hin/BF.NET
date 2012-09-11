/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.IO;
using System.Reflection;
using Mono.Options;

namespace BrainfuckCompiler
{
    internal class Program
    {
        private static string _sourceFile;
        private static string _outFile;
        private static string _moduleName;
        private static string _program;
        private static bool _optimizing;

        private static void Main(string[] args)
        {
            PrintAbout();
            if (!AnalyzeArguments(args))
            {
                return;
            }

            if (!String.IsNullOrEmpty(_sourceFile) && !File.Exists(_sourceFile))
            {
                Console.WriteLine("Error: source file {0} is not found." , _sourceFile);
            }


            String program = String.IsNullOrEmpty(_program) ? File.ReadAllText(_sourceFile) : _program;

            program = program
                .Replace(Environment.NewLine,"")
                .Replace(" ", "")
                .Replace("\t","");
            AssemblyEmitter.Emit(_moduleName,Path.GetFileName(_outFile), _outFile,program, _optimizing);
        }

        private static bool AnalyzeArguments(string[] args)
        {
            bool ss = true;
            _optimizing = true;
            OptionSet p = new OptionSet();
            p.Add("h|help|?", "Show this help message and exit.", v => { PrintUsage(); ss = false; });
            p.Add("o|output=", "A {path} to the output file.", v => { _outFile = v; });
            p.Add("p|code=","Raw Brainfuck {code} to compile." , v => { _program = v; });
            p.Add("d", "Disable code optiomization", t => { _optimizing = false; });
            var res = p.Parse(args);

            if (!ss)
            {
                return false;
            }

            _sourceFile = res.FirstOrDefault();
            if (String.IsNullOrEmpty(_sourceFile) && string.IsNullOrEmpty(_program))
            {
                PrintUsage();
                return false;
            }

            if (String.IsNullOrEmpty(_outFile))
            {
                if (!String.IsNullOrEmpty(_sourceFile))
                {
                    _outFile = Path.GetFileNameWithoutExtension(_sourceFile) + ".exe";
                }
                else
                {
                    _outFile = "out.exe";
                }
            }

            _moduleName = Path.GetFileNameWithoutExtension(Path.GetFileName(_outFile));

            return true;
        }

        private static string GetResourceText(string resourceFile)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BrainfuckCompiler."+resourceFile))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static void PrintAbout()
        {
            Console.WriteLine(GetResourceText("about.txt"));
        }

        private static void PrintUsage()
        {
            Console.WriteLine(GetResourceText("usage.txt"));
        }

        private static void CIL()
        {
            Console.WriteLine("Hello, Brainfuck");
            var x = Console.In;
            int z;
            z=x.Read();
        }
    }
}
