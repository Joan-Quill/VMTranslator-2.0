using VMTranslator2;
using static VMTranslator2.ConsoleWriter;

internal class VMTranslator
{

    public static List<string> fileNames { get; private set; } = new();

    static VMParser parser = new();
    static ASMWriter writer = new();

    public static bool _debug { get; set; } = false;
    public static bool filenameGet;

    static string[] opener = new string[] { "This is an application to convert VM file(s) into a single ASM program. Usage:" };

    static string[] helpText = new string[] { "<filename>.vm                   --Gets file from current directory",
                                              "<relativepath/filename>.vm      --Gets file from directory relative to current directory",
                                              "<absolutepath/filename>.vm      --Gets file from absolute directory",
                                              "<relativepath>                  --Gets files from directory relative to current directory",
                                              "<absolutepath>                  --Gets files from absolute directory",
                                              "HELP                            --Display this information",
                                              "EXIT                            --Close application"};
    
    private static void Main(string[] args)
    {
        filenameGet = false;
        string? input;

        if (args.Length == 0)
        {
            bool isfirstLoop = true;
            
            ConsoleWriter.Write(opener.Concat(helpText).ToArray(), ConsoleCode.MESSAGE);
            Console.Write(">");
            input = Console.ReadLine();
            
            while (!filenameGet)
            {
                if (!isfirstLoop)
                {
                    Console.Write(">");
                    input = Console.ReadLine();
                }

                if (!string.IsNullOrEmpty(input))
                {
                    InputProcessor(input);
                }

                isfirstLoop = false;
            }

        }
        else if (args.Length == 1 && !string.IsNullOrEmpty(args[0]))
        {
            input = args[0];
            InputProcessor(input);
            
        }
        else
        {
            ConsoleWriter.Write(opener.Concat(helpText).ToArray(), ConsoleCode.ERROR);
        }

    }

    public static void BeginOperation(bool isDir)
    {

        if (fileNames.Count == 0)
        {
            ConsoleWriter.Write(new string[] { "Failed to capture file." }, ConsoleCode.ERROR, ConsoleOptions.ConsoleBar);
        }
        else
        {
            foreach (string file in fileNames)
            {
                ConsoleWriter.Write(new string[] { $"Working on {Path.GetFileName(file)}" }, ConsoleCode.MESSAGE, ConsoleOptions.ConsoleBar);
                parser.Parser(file);

                //Debug Parser.Commands list if _debug == true
                if (_debug)
                {
                    string debugstr = "";
                    foreach (CommandObject cmd in parser.Commands)
                    {
                        debugstr += $"{cmd.Command} {cmd.Argument1} {cmd.Argument2}&&&";
                    }
                    string[] strings = debugstr.Split("&&&");

                    ConsoleWriter.Write(strings, ConsoleCode.DEBUG);
                }

                writer.commandObjects.Add(parser.Commands);

            }

            if (isDir)
            {
                try
                {
                    string? outFile = Path.GetDirectoryName(fileNames[0]);
                    if (outFile == null) throw new Exception();

                    string fName = outFile.Split(Path.DirectorySeparatorChar).Last();
                    outFile = string.Concat(outFile, Path.DirectorySeparatorChar, fName, ".asm");

                    ConsoleWriter.Write(new string[] { $"Writing to {Path.GetFileName(outFile)}" }, ConsoleCode.MESSAGE, ConsoleOptions.ConsoleBar);
                    writer.Writer(outFile, true);
                }
                catch
                {
                    ConsoleWriter.Write(new string[] { "Could not generate file." }, ConsoleCode.ERROR);
                    throw;
                }   
                
            }
            else
            {
                string outFile = fileNames[0].Replace(".vm", ".asm");
                ConsoleWriter.Write(new string[] { $"Writing to {Path.GetFileName(outFile)}" }, ConsoleCode.MESSAGE, ConsoleOptions.ConsoleBar);
                writer.Writer(outFile, false);
            }
            
        }
    }

    public static void InputProcessor(string input)
    {

        try
        {
            input = input.Trim();
            if (input.StartsWith(Path.DirectorySeparatorChar)) input = input.Remove(0, 1);
            if (File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + input))
            {
                filenameGet = true;
                fileNames.Add(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + input);
                BeginOperation(false);
            }
            else if (File.Exists(input))
            {
                filenameGet = true;
                fileNames.Add(input);
                BeginOperation(false);
            }
            else if (Directory.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + input))
            {
                filenameGet = true;
                DirectoryGetter(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + input);
                BeginOperation(true);
            }
            else if (Directory.Exists(input))
            {
                filenameGet = true;
                DirectoryGetter(input);
                BeginOperation(true);
            }
            else if (input.ToUpper().Equals("EXIT"))
            {
                return;
            }
            else if (input.ToUpper().Equals("HELP"))
            {
                ConsoleWriter.Write(helpText, ConsoleCode.MESSAGE);
            }
            else
            {
                ConsoleWriter.Write(new string[] { "Invalid command. Type HELP for list of commands." }, ConsoleCode.ERROR);
            }
        }
        catch
        {
            ConsoleWriter.Write(new string[] { "Could not complete operation. Exiting..." }, ConsoleCode.ERROR, ConsoleOptions.Wait | ConsoleOptions.ConsoleBar);
        }
    }

    public static void DirectoryGetter(string input)
    {
        foreach (string file in Directory.GetFiles(input, "*.vm", SearchOption.TopDirectoryOnly))
        {
            fileNames.Add(file);
            ConsoleOptions options = ConsoleOptions.ConsoleBar;
            if (fileNames.IndexOf(file) > 0) options = ConsoleOptions.None;
            ConsoleWriter.Write(new string[] { $"File captured at: {file}" }, ConsoleCode.MESSAGE, options);
        }
    }
}