using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MinecraftClient.Mapping;

namespace MinecraftClient.Commands
{
    public class DigBlock : Command
    {
        public override string CMDName { get { return "dig"; } }
        public override string CMDDesc { get { return "命令提示sdasdasdasd"; } }

        public override string Run(McTcpClient handler, string command, Dictionary<string, object> localVars)
        {
            string[] args = getArgs(command);
            if (args.Length == 3 || args.Length == 4)
            {
                try
                {
                    int status;
                    int x, y, z;
                    if (args.Length == 4)
                    {
                        status = int.Parse(args[0]);
                        x = int.Parse(args[1]);
                        y = int.Parse(args[2]);
                        z = int.Parse(args[3]);
                    }
                    else
                    { 
                        status = 0;
                        x = int.Parse(args[0]);
                        y = int.Parse(args[1]);
                        z = int.Parse(args[2]);
                    }
                    Location goal = new Location(x, y, z);
                    if (handler.DiggingBlock(status, goal)) 
                        return "Dig block at " + goal;
                    return "Failed to dig block at " + goal;
                }
                catch (FormatException) { return CMDDesc; }
            }
            else return CMDDesc;
        }
    }
}
