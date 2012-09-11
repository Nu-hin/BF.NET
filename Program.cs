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

namespace BrainfuckCompiler
{
    internal class Program
    {
        private static string _sourceFile;
        private static string _outFile;
        private static string _moduleName;

        private static void Main(string[] args)
        {
            PrintAbout();
            if (!AnalyzeArguments(args))
            {
                return;
            }
            
            String program = File.ReadAllText(_sourceFile)
                .Replace(Environment.NewLine,"")
                .Replace(" ", "")
                .Replace("\t","");
            AssemblyEmitter.Emit(_moduleName,Path.GetFileName(_outFile), _outFile,program);
        }

        private static bool AnalyzeArguments(string[] args)
        {
            if (!args.Any())
            {
                PrintUsage();
                return false;
            }

            _sourceFile = args.Last();
            _outFile = _sourceFile + ".exe";
            _moduleName = Path.GetFileNameWithoutExtension(_sourceFile);
            //for (int i = 0; i < args.Length; i++)
            //{
            //    switch (args[i])
            //    {
            //        case "/?":
            //        case "/help":
            //            PrintUsage();
            //            return false;
            //        default:
            //            break;
            //    }
            //}
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
