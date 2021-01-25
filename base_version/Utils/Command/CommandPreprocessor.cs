using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class CommandPreprocessor
    {
        public static List<string> Preprocess(string[] lines)
        {
            // List of commands after processing
            List<string> commands = new List<string>();

            // Commands between begin-repeat and end-repeat will be written numberOfIterations times into command list
            List<string> repeatCommands = new List<string>();
            bool repeat = false;
            int numberOfIterations = 0;

            foreach (string line in lines)
            {
                if (line[0].Equals('#')) continue;
                List<string> splitLine = line.Trim().Split(' ').ToList();
                string command = splitLine.ElementAt(0);
                splitLine.RemoveAt(0);
                List<string> arguments = splitLine;

                if (String.Equals(command, "begin-repeat"))
                {
                    if (arguments.Count != 1)
                    {
                        throw new PreprocessingException("begin-repeat expects one argument corresponding to the number of iterations.");
                    }

                    if (repeat)
                    {
                        throw new PreprocessingException("Can't use begin-repeat inside another begin-repeat.");
                    }

                    string argument = arguments.ElementAt(0);
                    try
                    {
                        numberOfIterations = Int32.Parse(argument);
                    }
                    catch (FormatException)
                    {
                        throw new PreprocessingException($"'{argument}' should be an integer.");
                    }

                    repeat = true;
                    repeatCommands = new List<string>();

                }
                else if (String.Equals(command, "end-repeat"))
                {
                    if (repeat == false)
                    {
                        throw new PreprocessingException("Can't use end-repeat without using begin-repeat first.");
                    }
                    if (arguments.Count != 0)
                    {
                        throw new PreprocessingException("end-repeat expects no arguments.");
                    }

                    for (int i = 1; i <= numberOfIterations; i++)
                    {
                        repeatCommands.ForEach(delegate (string repeatCommand)
                        {
                            repeatCommand = repeatCommand.Replace("$i", i.ToString());
                            commands.Add(repeatCommand);
                        });
                    }

                    repeat = false;
                }
                else
                {
                    if (repeat)
                    {
                        repeatCommands.Add(line);
                    }
                    else
                    {
                        commands.Add(line);
                    }
                }
            }

            return commands;
        }
    }

    public class PreprocessingException : System.Exception
    {
        public PreprocessingException() : base() { }

        public PreprocessingException(string message) : base(message) { }
    }
}
