using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMTranslator2
{
    internal class CommandObject
    {
        public enum CommandType
        {
            ARITHMETIC,
            PUSH,
            POP,
            LABEL,
            GOTO,
            IF,
            FUNCTION,
            RETURN,
            CALL
        }

        public CommandType Command { get; private set; }
        public string Argument1 { get; private set; }
        public int Argument2 { get; private set; }

        public CommandObject(CommandType cmd, string arg1 = "", int arg2 = -1)
        {
            Command = cmd;
            Argument1 = arg1;
            Argument2 = arg2;
        }
    }
}
