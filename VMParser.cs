using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static VMTranslator2.CommandObject;
using static VMTranslator2.ConsoleWriter;

namespace VMTranslator2
{
    internal class VMParser
    {
        

        public static string[] ArithmeticCommands = new string[]
        {
            "add", "sub", "neg",
            "and", "not", "eq",
            "gt",  "lt",  "or"
        };

        public List<string> FileContents { get; private set; } = new();
        public List<CommandObject> Commands { get; private set; }

        private StreamReader streamReader;
        private int currentLine;
        private string fileName;

        public void Parser(string fileName)
        {
            
            try
            {
                streamReader = new StreamReader(fileName);
                Commands = new List<CommandObject>();

                this.fileName = fileName;
                string workingLine = "";
                currentLine = 0;

                while (!streamReader.EndOfStream)
                {
                    workingLine = RemoveComments(streamReader.ReadLine()).Trim();

                    if (!string.IsNullOrWhiteSpace(workingLine))
                    {
                        FileContents.Add(workingLine);
                        currentLine = FileContents.Count;
                        MakeCommands(workingLine);
                    }
                
                }

                ConsoleWriter.Write(new string[] { $"Successfully read {Path.GetFileName(fileName)} : {Commands.Count} Commands" }, ConsoleCode.SUCCESS);

            }
            catch
            {
                ConsoleWriter.Write(new string[] { "Unable to parse file. Moving to next operation." }, ConsoleCode.ERROR);
            }

        }

        private static string RemoveComments(string text)
        {
            int pos = text.IndexOf(@"//");

            if (pos != -1)
            {
                text = text.Substring(0, pos);
            }

            return text;
        }

        public void MakeCommands(string line)
        {
            
            string[] segments = line.Split(' ');

            if (segments.Length > 3)
            {
                ConsoleWriter.Write(new string[] { $"Too many argments on line {currentLine} in {Path.GetFileName(fileName)}" }, ConsoleCode.ERROR);
                throw new Exception();
            }
            else
            {

                if (ArithmeticCommands.Contains(segments[0]))
                {
                    Commands.Add(new CommandObject(CommandType.ARITHMETIC, segments[0]));
                }
                else if (segments[0].Equals("return"))
                {
                    Commands.Add(new CommandObject(CommandType.RETURN, segments[0]));
                }
                else
                {
                    switch (segments[0])
                    {
                        case "push":
                            try { Commands.Add(new CommandObject(CommandType.PUSH, segments[1], Int32.Parse(segments[2]))); }
                            catch { ConsoleWriter.Write(new string[] { $"Could not read command on line {currentLine} in {Path.GetFileName(fileName)}" }, ConsoleCode.ERROR); throw; }
                            break;

                        case "pop":
                            try { Commands.Add(new CommandObject(CommandType.POP, segments[1], Int32.Parse(segments[2]))); }
                            catch { ConsoleWriter.Write(new string[] { $"Could not read command on line {currentLine} in {Path.GetFileName(fileName)}" }, ConsoleCode.ERROR); throw; }
                            break;

                        case "label":
                            Commands.Add(new CommandObject(CommandType.LABEL, segments[1]));
                            break;

                        case "if-goto":
                            Commands.Add(new CommandObject(CommandType.IF, segments[1]));
                            break;

                        case "goto":
                            Commands.Add(new CommandObject(CommandType.GOTO, segments[1]));
                            break;

                        case "function":
                            try { Commands.Add(new CommandObject(CommandType.FUNCTION, segments[1], Int32.Parse(segments[2]))); }
                            catch { ConsoleWriter.Write(new string[] { $"Could not read command on line {currentLine} in {Path.GetFileName(fileName)}" }, ConsoleCode.ERROR); throw; }
                            break;

                        case "call":
                            try { Commands.Add(new CommandObject(CommandType.CALL, segments[1], Int32.Parse(segments[2]))); }
                            catch { ConsoleWriter.Write(new string[] { $"Could not read command on line {currentLine} in {Path.GetFileName(fileName)}" }, ConsoleCode.ERROR); throw; }
                            break;

                        default:
                            ConsoleWriter.Write(new string[] { $"Could not read command on line {currentLine} in {Path.GetFileName(fileName)}" }, ConsoleCode.ERROR);
                            throw new Exception();
                            break;
                    }
                }
            }

        }

    }
}
