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
using System.Reflection.Emit;
using System.Reflection;
using System.IO;

namespace BrainfuckCompiler
{
    internal class AssemblyEmitter
    {
        public static void Emit(string moduleName, string outFileName, string outputFileName, string program, bool optimize)
        {
            bool hasWriter = false;
            bool hasReader = false;
            AssemblyName assemblyName = new AssemblyName("Brainfuck");
            assemblyName.Version = new Version("1.0.0.0");

            var dir = Path.GetDirectoryName(outputFileName);

            if (String.IsNullOrEmpty(dir))
            {
                dir = Directory.GetCurrentDirectory();
            }

            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save,dir);
            var module = ab.DefineDynamicModule(moduleName, outFileName);
            TypeBuilder tb = module.DefineType("BrainfuckProgram", TypeAttributes.NotPublic);

            var mainMethod = tb.DefineMethod("Main", MethodAttributes.Static | MethodAttributes.HideBySig);
            var ilgen = mainMethod.GetILGenerator();
            //Main
            ilgen.DeclareLocal(typeof(byte[])); // band 0
            ilgen.DeclareLocal(typeof(int)); // p 1

            if (program.Contains(','))
            {
                hasReader = true;
                ilgen.DeclareLocal(typeof(Stream)); //reader 2
            }

            if (program.Contains('.'))
            {
                hasWriter = true;
                ilgen.DeclareLocal(typeof(Stream)); //writer 3
            }

            //allocate 30000 band
            ilgen.Emit(OpCodes.Ldc_I4, 30000);
            ilgen.Emit(OpCodes.Newarr, typeof(byte));
            ilgen.Emit(OpCodes.Stloc_S, 0);
            
            //p=0
            ilgen.Emit(OpCodes.Ldc_I4_0);
            ilgen.Emit(OpCodes.Stloc_1);

            if (hasReader)
            {
                //assign reader
                ilgen.EmitCall(OpCodes.Call, typeof(Console).GetMethod("OpenStandardInput", Type.EmptyTypes), Type.EmptyTypes);
                ilgen.Emit(OpCodes.Stloc_2);
            }

            if (hasWriter)
            {
                //assign writer
                ilgen.EmitCall(OpCodes.Call, typeof(Console).GetMethod("OpenStandardOutput", Type.EmptyTypes), Type.EmptyTypes);
                ilgen.Emit(hasReader ? OpCodes.Stloc_3 : OpCodes.Stloc_2);
            }

            Stack<Label> labelStack = new Stack<Label>();
            Stack<int> bracesPostion = new Stack<int>();
            int j=0;
            int c=0;

            for (int i = 0; i < program.Length; i++)
            {
                switch (program[i])
                {
                    case '>':
                        c = 1;
                        if (optimize)
                        {
                            j = i + 1;
                            
                            while (j < program.Length && program[j] == '>')
                            {
                                c++;
                                j++;
                            }
                            i = j - 1;
                        }
                        ilgen.Emit(OpCodes.Ldloc_1);
                        ilgen.Emit(OpCodes.Ldc_I4,c);
                        ilgen.Emit(OpCodes.Add);
                        ilgen.Emit(OpCodes.Stloc_1);
                        break;
                    case '<':
                        c = 1;
                        if (optimize)
                        {
                            j = i + 1;

                            while (j < program.Length && program[j] == '<')
                            {
                                c++;
                                j++;
                            }
                            i = j - 1;
                        }
                        
                        ilgen.Emit(OpCodes.Ldloc_1);
                        ilgen.Emit(OpCodes.Ldc_I4,c);
                        ilgen.Emit(OpCodes.Sub);
                        ilgen.Emit(OpCodes.Stloc_1);
                        break;
                    case '+':
                        c = 1;
                        if (optimize)
                        {
                            j = i + 1;

                            while (j < program.Length && program[j] == '+')
                            {
                                c++;
                                j++;
                            }
                            i = j - 1;
                        }
                        ilgen.Emit(OpCodes.Ldloc_0);
                        ilgen.Emit(OpCodes.Ldloc_1);
                        ilgen.Emit(OpCodes.Ldelema, typeof(byte));
                        ilgen.Emit(OpCodes.Dup);
                        ilgen.Emit(OpCodes.Ldobj,typeof(byte));
                        ilgen.Emit(OpCodes.Ldc_I4,c);
                        ilgen.Emit(OpCodes.Add);
                        ilgen.Emit(OpCodes.Conv_U1);
                        ilgen.Emit(OpCodes.Stobj, typeof(byte));
                        break;
                    case '-':
                        c = 1;
                        if (optimize)
                        {
                            j = i + 1;

                            while (j < program.Length && program[j] == '-')
                            {
                                c++;
                                j++;
                            }
                            i = j - 1;
                        }
                        ilgen.Emit(OpCodes.Ldloc_0);
                        ilgen.Emit(OpCodes.Ldloc_1);
                        ilgen.Emit(OpCodes.Ldelema, typeof(byte));
                        ilgen.Emit(OpCodes.Dup);
                        ilgen.Emit(OpCodes.Ldobj,typeof(byte));
                        ilgen.Emit(OpCodes.Ldc_I4,c);
                        ilgen.Emit(OpCodes.Sub);
                        ilgen.Emit(OpCodes.Conv_U1);
                        ilgen.Emit(OpCodes.Stobj, typeof(byte));
                        break;
                    case '.':
                        ilgen.Emit(hasReader ? OpCodes.Ldloc_3 : OpCodes.Ldloc_2);
                        ilgen.Emit(OpCodes.Ldloc_0);
                        ilgen.Emit(OpCodes.Ldloc_1);
                        ilgen.Emit(OpCodes.Ldelem_U1);
                        ilgen.EmitCall(OpCodes.Callvirt, typeof(Stream).GetMethod("WriteByte", new Type[] { typeof(byte) }), Type.EmptyTypes);
                        break;
                    case ',':
                        ilgen.Emit(OpCodes.Ldloc_0);
                        ilgen.Emit(OpCodes.Ldloc_1);
                        ilgen.Emit(OpCodes.Ldelema, typeof(byte));
                        ilgen.Emit(OpCodes.Ldloc_2);
                        ilgen.EmitCall(OpCodes.Callvirt, typeof(Stream).GetMethod("ReadByte", Type.EmptyTypes), Type.EmptyTypes);
                        ilgen.Emit(OpCodes.Conv_U1);
                        ilgen.Emit(OpCodes.Stobj, typeof(byte));
                        break;
                    case '[':
                        bracesPostion.Push(i);
                        var farlabel = ilgen.DefineLabel();
                        var closelabel = ilgen.DefineLabel();
                        ilgen.Emit(OpCodes.Br, farlabel);
                        ilgen.MarkLabel(closelabel);
                        labelStack.Push(farlabel);
                        labelStack.Push(closelabel);
                        break;
                    case ']':
                        bracesPostion.Pop();
                        if (!labelStack.Any())
                        {
                            Console.WriteLine("Syntax error: Unbalanced ']' at position {0}.",  i);
                            return;
                        }
                        var _closelabel = labelStack.Pop();
                        var _farlabel = labelStack.Pop();
                        ilgen.MarkLabel(_farlabel);
                        ilgen.Emit(OpCodes.Ldloc_0);
                        ilgen.Emit(OpCodes.Ldloc_1);
                        ilgen.Emit(OpCodes.Ldelem_U1);
                        ilgen.Emit(OpCodes.Brtrue, _closelabel);
                        break;
                    default:
                        Console.WriteLine("Syntax error: Unexpected symbol '{0}' at position {1}.", program[i], i);
                        return;
                }
            }

            if (bracesPostion.Any())
            {
                Console.WriteLine("Syntax error: Unbalanced '[' at position {0}.", bracesPostion.Peek());
                return;
            }


            ilgen.Emit(OpCodes.Ret);
            //end main

            ab.SetEntryPoint(mainMethod);

            tb.CreateType();
            ab.Save(Path.GetFileName(outputFileName));
        }
    }
}
