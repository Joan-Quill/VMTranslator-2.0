using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static VMTranslator2.ConsoleWriter;
using static VMTranslator2.CommandObject;

namespace VMTranslator2
{
    internal class ASMWriter
    {

        public List<List<CommandObject>> commandObjects { get; private set; } = new();
        private string commandObjName;
        private int commandCount = 0;
        private int commandObjCount;
        private StreamWriter streamWriter;
        private int jumps = 0;
        private int labelCount = 0;
        private string fileName;

        public void Writer(string fileName, bool writeInit)
        {
            try
            {
                this.fileName = fileName;
                streamWriter = new StreamWriter(fileName);
                jumps = 0;

                if (writeInit) SysInit();

                for (int i = 0; i < commandObjects.Count; i++)
                {
                    List<CommandObject> commandObjectList = commandObjects[i];
                    commandObjName = Path.GetFileName(VMTranslator.fileNames[i]).Replace(".vm", "");
                    commandObjCount++;
                    bool _debug = VMTranslator._debug;

                    for (int j = 0; j < commandObjectList.Count; j++)
                    {
                        switch (commandObjectList[j].Command)
                        {
                            case CommandType.PUSH:
                                if (_debug) DebugWriter(commandObjectList[j]);
                                streamWriter.WriteLine($"//Push {commandObjectList[j].Argument1} {commandObjectList[j].Argument2}");
                                PushPopWriter(commandObjectList[j]);
                                break;

                            case CommandType.POP:
                                if (_debug) DebugWriter(commandObjectList[j]);
                                streamWriter.WriteLine($"//Pop {commandObjectList[j].Argument1} {commandObjectList[j].Argument2}");
                                PushPopWriter(commandObjectList[j]);
                                break;

                            case CommandType.ARITHMETIC:
                                if (_debug) DebugWriter(commandObjectList[j]);
                                streamWriter.WriteLine($"//{commandObjectList[j].Argument1}");
                                ArithmeticWriter(commandObjectList[j].Argument1);
                                break;

                            case CommandType.LABEL:
                                if (_debug) DebugWriter(commandObjectList[j]);
                                streamWriter.WriteLine($"//Label {commandObjectList[j].Argument1}");
                                LabelWriter(commandObjectList[j].Argument1);
                                break;

                            case CommandType.GOTO:
                                if (_debug) DebugWriter(commandObjectList[j]);
                                streamWriter.WriteLine($"//Goto {commandObjectList[j].Argument1}");
                                GotoWriter(commandObjectList[j].Argument1);
                                break;

                            case CommandType.IF:
                                if (_debug) DebugWriter(commandObjectList[j]);
                                streamWriter.WriteLine($"//If-Goto {commandObjectList[j].Argument1}");
                                IfWriter(commandObjectList[j].Argument1);
                                break;

                            case CommandType.RETURN:
                                if (_debug) DebugWriter(commandObjectList[j]);
                                streamWriter.WriteLine($"//Return");
                                ReturnWriter();
                                break;

                            case CommandType.FUNCTION:
                                if (_debug) DebugWriter(commandObjectList[j]);
                                streamWriter.WriteLine($"//Function {commandObjectList[j].Argument1} {commandObjectList[j].Argument2}");
                                FunctionWriter(commandObjectList[j].Argument1, commandObjectList[j].Argument2);
                                break;

                            case CommandType.CALL:
                                if (_debug) DebugWriter(commandObjectList[j]);
                                streamWriter.WriteLine($"//Call {commandObjectList[j].Argument1} {commandObjectList[j].Argument2}");
                                CallWriter(commandObjectList[j].Argument1, commandObjectList[j].Argument2);
                                break;

                            default:
                                ConsoleWriter.Write(new string[] { "Could not read CommandType.",
                                                                  $"Line {commandCount} : (Line {commandObjCount} in {commandObjName})" }, ConsoleCode.ERROR);
                                throw new Exception();
                                break;
                        }
                        commandCount++;
                    }

                }

                streamWriter.Close();
                ConsoleWriter.Write(new string[] { $"Successfully wrote to {fileName}" }, ConsoleCode.SUCCESS);
            }
            catch
            {
                ConsoleWriter.Write(new string[] { "Could not generate file. Moving to next operation." }, ConsoleCode.ERROR);
            }
        }

        private void DebugWriter(CommandObject cmd)
        {
            ConsoleWriter.Write(new string[] { $"Writing {cmd.Command} {cmd.Argument1} {cmd.Argument2}" }, ConsoleCode.DEBUG);
        }

        private void PushPopWriter(CommandObject commandObject)
        {
            switch (commandObject.Command)
            {
                case CommandType.PUSH:
                    switch (commandObject.Argument1)
                    {
                        case "constant":
                            streamWriter.Write($"@{commandObject.Argument2}" + "\n" +
                                                "D=A"                        + "\n" +
                                                "@SP"                        + "\n" +
                                                "A=M"                        + "\n" +
                                                "M=D"                        + "\n" +
                                                "@SP"                        + "\n" +
                                                "M=M+1"                      + "\n");
                            break;

                        case "local":
                            PushWriter("LCL", commandObject.Argument2, false);
                            break;

                        case "argument":
                            PushWriter("ARG", commandObject.Argument2, false);
                            break;

                        case "this":
                            PushWriter("THIS", commandObject.Argument2, false);
                            break;

                        case "that":
                            PushWriter("THAT", commandObject.Argument2, false);
                            break;

                        case "temp":
                            streamWriter.Write($"@R{commandObject.Argument2 + 5}" + "\n" +
                                                "D=M"                             + "\n" +
                                                "@SP"                             + "\n" +
                                                "A=M"                             + "\n" +
                                                "M=D"                             + "\n" +
                                                "@SP"                             + "\n" +
                                                "M=M+1"                           + "\n");
                            break;

                        case "pointer":
                            switch (commandObject.Argument2)
                            {
                                case 0:
                                    PushWriter("THIS", commandObject.Argument2, true);
                                    break;

                                case 1:
                                    PushWriter("THAT", commandObject.Argument2, true);
                                    break;

                                default:
                                    ConsoleWriter.Write(new string[] { "Unexpected pointer value.",
                                                                      $"Line {commandCount} : (Line {commandObjCount} in {commandObjName})" }, ConsoleCode.ERROR);
                                    throw new Exception();
                                    break;
                            }
                            break;

                        case "static":
                            streamWriter.Write($"@{Path.GetFileName(commandObjName).Split('.')[0]}_{commandObject.Argument2}" + "\n" +
                                                "D=M"                                                 + "\n" +
                                                "@SP"                                                 + "\n" +
                                                "A=M"                                                 + "\n" +
                                                "M=D"                                                 + "\n" +
                                                "@SP"                                                 + "\n" +
                                                "M=M+1"                                               + "\n");
                            break;

                        default:
                            ConsoleWriter.Write(new string[] { "Expected Push command.",
                                                              $"Line {commandCount} : (Line {commandObjCount} in {commandObjName})" }, ConsoleCode.ERROR);
                            throw new Exception();
                            break;
                    }
                    break;

                case CommandType.POP:
                    switch (commandObject.Argument1)
                    {
                        case "local":
                            PopWriter("LCL", commandObject.Argument2, false);
                            break;

                        case "argument":
                            PopWriter("ARG", commandObject.Argument2, false);
                            break;

                        case "this":
                            PopWriter("THIS", commandObject.Argument2, false);
                            break;

                        case "that":
                            PopWriter("THAT", commandObject.Argument2, false);
                            break;

                        case "temp":
                            streamWriter.Write($"@R{commandObject.Argument2 + 5}" + "\n" +
                                                "D=A"                             + "\n" +
                                                "@R14"                            + "\n" +
                                                "M=D"                             + "\n" +
                                                "@SP"                             + "\n" +
                                                "M=M-1"                           + "\n" +
                                                "A=M"                             + "\n" +
                                                "D=M"                             + "\n" +
                                                "@R14"                            + "\n" +
                                                "A=M"                             + "\n" +
                                                "M=D"                             + "\n");
                            break;

                        case "pointer":
                            switch (commandObject.Argument2)
                            {
                                case 0:
                                    PopWriter("THIS", commandObject.Argument2, true);
                                    break;

                                case 1:
                                    PopWriter("THAT", commandObject.Argument2, true);
                                    break;

                                default:
                                    ConsoleWriter.Write(new string[] { "Unexpected pointer value.",
                                                                      $"Line {commandCount} : (Line {commandObjCount} in {commandObjName})" }, ConsoleCode.ERROR);
                                    throw new Exception();
                                    break;
                            }
                            break;

                        case "static":
                            streamWriter.Write($"@{Path.GetFileName(commandObjName).Split('.')[0]}_{commandObject.Argument2}" + "\n" +
                                                "D=A"                                                 + "\n" +
                                                "@R13"                                                + "\n" +
                                                "M=D"                                                 + "\n" +
                                                "@SP"                                                 + "\n" +
                                                "AM=M-1"                                              + "\n" +
                                                "D=M"                                                 + "\n" +
                                                "@R13"                                                + "\n" +
                                                "A=M"                                                 + "\n" +
                                                "M=D"                                                 + "\n");
                            break;

                        default:
                            ConsoleWriter.Write(new string[] { "Expected Pop command.",
                                                              $"Line {commandCount} : (Line {commandObjCount} in {commandObjName})" }, ConsoleCode.ERROR);
                            throw new Exception();
                            break;
                    }
                    break;

                default:
                    ConsoleWriter.Write(new string[] { "Expected Push or Pop command.",
                                                      $"Line {commandCount} : (Line {commandObjCount} in {commandObjName})" }, ConsoleCode.ERROR);
                    throw new Exception();
                    break;
            }

        }

        private void ArithmeticWriter(string command)
        {
            switch (command)
            {
                case "add":
                    ArithmeticBase();
                    streamWriter.WriteLine("M=M+D");
                    break;

                case "sub":
                    ArithmeticBase();
                    streamWriter.WriteLine("M=M-D");
                    break;

                case "and":
                    ArithmeticBase();
                    streamWriter.WriteLine("M=M&D");
                    break;

                case "or":
                    ArithmeticBase();
                    streamWriter.WriteLine("M=M|D");
                    break;

                case "gt":
                    ArithmeticBase("JLE");
                    jumps++;
                    break;

                case "lt":
                    ArithmeticBase("JGE");
                    jumps++;
                    break;

                case "eq":
                    ArithmeticBase("JNE");
                    jumps++;
                    break;

                case "not":
                    streamWriter.Write("@SP"   + "\n" +
                                       "A=M-1" + "\n" +
                                       "M=!M"  + "\n");
                    break;

                case "neg":
                    streamWriter.Write("D=0"   + "\n" +
                                       "@SP"   + "\n" +
                                       "A=M-1" + "\n" +
                                       "M=D-M" + "\n");
                    break;

                default:
                    ConsoleWriter.Write(new string[] { "Expected arithmetic command.", 
                                                      $"Line {commandCount} : (Line {commandObjCount} in {commandObjName})" }, ConsoleCode.ERROR);
                    throw new Exception();
                    break;

            }
        }

        private void ArithmeticBase(string jumpType = "")
        {
            streamWriter.Write("@SP"    + "\n" +
                               "AM=M-1" + "\n" +
                               "D=M"    + "\n" +
                               "A=A-1"  + "\n");

            if (!string.IsNullOrEmpty(jumpType))
            {
                streamWriter.Write("D=M-D"                      + "\n" +
                                  $"@FALSE_{jumpType}_{jumps}"  + "\n" +
                                  $"D;{jumpType}"               + "\n" +
                                   "@SP"                        + "\n" +
                                   "A=M-1"                      + "\n" +
                                   "M=-1"                       + "\n" +
                                  $"@CONT_{jumpType}_{jumps}"   + "\n" +
                                   "0;JMP"                      + "\n" +
                                  $"(FALSE_{jumpType}_{jumps})" + "\n" +
                                   "@SP"                        + "\n" +
                                   "A=M-1"                      + "\n" +
                                   "M=0"                        + "\n" +
                                  $"(CONT_{jumpType}_{jumps})"  + "\n");
            }
        }

        private void PushWriter(string Arg1, int Arg2, bool isDirect)
        {
            string DirectAddress = isDirect ? "" : $"@{Arg2}" + "\n" +
                                                    "A=D+A"   + "\n" +
                                                    "D=M"     + "\n";

            streamWriter.Write($"@{Arg1}" + "\n" +
                                "D=M"     + "\n" +
                                DirectAddress    +
                                "@SP"     + "\n" +
                                "A=M"     + "\n" +
                                "M=D"     + "\n" +
                                "@SP"     + "\n" +
                                "M=M+1"   + "\n");
        }

        private void PopWriter(string Arg1, int Arg2, bool isDirect)
        {
            string DirectAddress = isDirect ? "D=A\n" : "D=M"     + "\n" +
                                                        $"@{Arg2}" + "\n" +
                                                         "D=D+A"   + "\n";

            streamWriter.Write($"@{Arg1}" + "\n" +
                                DirectAddress    +
                                "@R13"    + "\n" +
                                "M=D"     + "\n" +
                                "@SP"     + "\n" +
                                "AM=M-1"  + "\n" +
                                "D=M"     + "\n" +
                                "@R13"    + "\n" +
                                "A=M"     + "\n" +
                                "M=D"     + "\n");
        }

        private void LabelWriter(string label)
        {
            streamWriter.WriteLine($"({label})");
        }

        private void FunctionWriter(string Arg1, int Arg2)
        {
            streamWriter.WriteLine($"({Arg1})");

            for (int i = 0; i < Arg2; i++)
            {
                PushPopWriter(new CommandObject(CommandType.PUSH, "constant", 0));
            }
        }

        private void GotoWriter(string label)
        {
            streamWriter.Write($"@{label}" + "\n" +
                                "0;JMP"    + "\n");
        }

        private void IfWriter(string label)
        {
            ArithmeticBase();
            streamWriter.Write($"@{label}" + "\n" +
                                "D;JNE"    + "\n");
        }

        private void CallWriter(string Arg1, int Arg2)
        {
            string newLabel = $"RTRN_LABEL_{labelCount++}";

            streamWriter.Write($"@{newLabel}" + "\n" +
                                "D=A"         + "\n" +
                                "@SP"         + "\n" +
                                "A=M"         + "\n" +
                                "M=D"         + "\n" +
                                "@SP"         + "\n" +
                                "M=M+1"       + "\n");

            PushWriter("LCL", 0, true);
            PushWriter("ARG", 0, true);
            PushWriter("THIS", 0, true);
            PushWriter("THAT", 0, true);

            streamWriter.Write("@SP"          + "\n" +
                               "D=M"          + "\n" +
                               "@5"           + "\n" +
                               "D=D-A"        + "\n" +
                              $"@{Arg2}"      + "\n" +
                               "D=D-A"        + "\n" +
                               "@ARG"         + "\n" +
                               "M=D"          + "\n" +
                               "@SP"          + "\n" +
                               "D=M"          + "\n" +
                               "@LCL"         + "\n" +
                               "M=D"          + "\n" +
                              $"@{Arg1}"      + "\n" +
                               "0;JMP"        + "\n" +
                              $"({newLabel})" + "\n");
        }
        
        private void ReturnWriter()
        {
            streamWriter.Write("@LCL"  + "\n" +
                               "D=M"   + "\n" +
                               "@R11"  + "\n" +
                               "M=D"   + "\n" +
                               "@5"    + "\n" +
                               "A=D-A" + "\n" +
                               "D=M"   + "\n" +
                               "@R12"  + "\n" +
                               "M=D"   + "\n");

            PopWriter("ARG", 0, false);

            streamWriter.Write("@ARG"  + "\n" +
                               "D=M"   + "\n" +
                               "@SP"   + "\n" +
                               "M=D+1" + "\n");

            FrameWriter("THAT");
            FrameWriter("THIS");
            FrameWriter("ARG");
            FrameWriter("LCL");

            streamWriter.Write("@R12"  + "\n" +
                               "A=M"   + "\n" +
                               "0;JMP" + "\n");
        }

        private void FrameWriter(string pos)
        {
            streamWriter.Write("@R11"   + "\n" +
                               "D=M-1"  + "\n" +
                               "AM=D"   + "\n" +
                               "D=M"    + "\n" +
                              $"@{pos}" + "\n" +
                               "M=D"    + "\n");
        }
        
        
        private void SysInit()
        {
            streamWriter.Write("@256" + "\n" +
                               "D=A"  + "\n" +
                               "@SP"  + "\n" +
                               "M=D"  + "\n");

            CallWriter("Sys.init", 0);
        }

    }
}
