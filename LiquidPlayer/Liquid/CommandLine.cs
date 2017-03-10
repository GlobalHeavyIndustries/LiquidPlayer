using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class CommandLine : Object
    {
        protected string commandLine;
        protected int[] flags;
        protected List<string> arguments;

        public static int NewCommandLine(string commandLine, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.CommandLine);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new CommandLine(id, commandLine);

            return id;
        }

        public CommandLine(int id, string commandLine)
            : base(id)
        {
            this.commandLine = commandLine;
            this.flags = new int[256];
            this.arguments = new List<string>();

            commandLine += (char)0;

            flags.Fill(-1);

            var index = 0;

            while (true)
            {
                var ch = commandLine[index];

                if (ch == 0)
                {
                    break;
                }

                if (ch == ' ')
                {
                    index++;
                }
                else if (ch == '-')
                {
                    index++;
                    ch = commandLine[index];

                    if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'))
                    {
                        var flag = ch;
                        var data = "";

                        index++;
                        ch = commandLine[index];

                        while (ch >= '0' && ch <= '9')
                        {
                            data += ch;

                            index++;
                            ch = commandLine[index];
                        }

                        if (data == "")
                        {
                            flags[flag] = 1;
                        }
                        else
                        {
                            flags[flag] = Convert.ToInt32(data);
                        }
                    }
                }
                else if (ch == '"')
                {
                    var data = "";

                    while (true)
                    {
                        index++;
                        ch = commandLine[index];

                        if (ch == 0)
                        {
                            break;
                        }
                        else if (ch == '"')
                        {
                            index++;
                            break;
                        }

                        data += ch;
                    }

                    arguments.Add(data);
                }
                else if (ch >= 33 && ch <= 127)
                {
                    var data = "" + ch;

                    while(true)
                    {
                        index++;
                        ch = commandLine[index];

                        if (ch < 33 || ch > 127)
                        {
                            break;
                        }

                        data += ch;
                    }

                    arguments.Add(data);
                }
                else
                {
                    Throw(ExceptionCode.Denied);
                    return;
                }
            }
        }

        public override string ToString()
        {
            return $"CommandLine: \"{commandLine}\"";
        }

        public int GetArgumentCount()
        {
            return arguments.Count;
        }

        public string GetArgument(int index)
        {
            if (index < 0 || index >= arguments.Count)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return "";
            }

            return arguments[index];
        }

        public int GetArguments()
        {
            var listId = List.NewList((int)LiquidClass.String, objectId);

            if (listId == 0)
            {
                Throw(ExceptionCode.OutOfMemory);
                return 0;
            }

            var list = objectManager[listId].LiquidObject as List;

            for (var index = 0; index < arguments.Count; index++)
            {
                var id = NewString(objectId, arguments[index]);

                list.Add(id);

                FreeString(objectId, id);
            }

            return listId;
        }

        public int GetSwitch(int index)
        {
            if (index < 65 || (index > 90 && index < 97) || index > 122)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return 0;
            }

            return flags[index];
        }

        public override void shutdown()
        {
            flags = null;
            arguments = null;

            base.shutdown();
        }
    }
}
