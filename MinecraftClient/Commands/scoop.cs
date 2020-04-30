using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MinecraftClient.Mapping;
using MinecraftClient.Inventory;
using System.Threading;

namespace MinecraftClient.Commands
{
    public class Scoop : Command
    {
        public override string CMDName { get { return "scoop"; } }
        public override string CMDDesc { get { return "use bucket"; } }

        public override string Run(McTcpClient handler, string command, Dictionary<string, object> localVars)
        {
            string[] args = getArgs(command);
            if (args.Length == 3 || args.Length == 4)
            {
                try
                {
                    //hotbar 0-8
                    int hotbar;
                    int x, y, z;
                    if (args.Length == 4)
                    {
                        hotbar = int.Parse(args[0]);
                        x = int.Parse(args[1]);
                        y = int.Parse(args[2]);
                        z = int.Parse(args[3]);
                    }
                    else
                    {
                        hotbar = 8;
                        x = int.Parse(args[0]);
                        y = int.Parse(args[1]);
                        z = int.Parse(args[2]);
                    }
                    Location goal = new Location(x, y, z);
                    if (!handler.GetWorld().GetBlock(goal).Type.IsLiquid())
                        return "Can't bucket solid block or air";
                    if (FindBucketAndUse(handler, hotbar, goal))
                        return "Use bucket at " + goal;
                    return "Failed to use bucket at " + goal;
                }
                catch (FormatException) { return CMDDesc; }
            }
            else return CMDDesc;
        }

        public bool FindBucketAndUse(McTcpClient handler,int hotbar, Location block)
        {
            Container container = handler.GetPlayerInventory();
            Container inventory = new Container(container.ID, container.Type, container.Title, container.Items);
            bool found = false;
            byte CurrentSlot = handler.GetCurrentSlot();
            if (inventory.Items.ContainsKey(CurrentSlot + 36) && inventory.Items[CurrentSlot + 36].Type == ItemType.Bucket)
            {
                found = true;
            }
            else
            {
                for (int i = 36; i <= 44; i++)
                {
                    if (!inventory.Items.ContainsKey(i)) continue;
                    if (inventory.Items[i].Type == ItemType.Bucket)
                    {
                        int slot = i - 36;
                        handler.ChangeSlot((short)slot);
                        found = true;
                        break;
                    }
                }
                for (int i = 9; i <= 35; i++)
                {
                    if (!inventory.Items.ContainsKey(i)) continue;
                    if (inventory.Items[i].Type == ItemType.Bucket)
                    {
                        handler.ClickWindowSlot(0, i, hotbar, 2);
                        handler.ChangeSlot((short)hotbar);
                        found = true;
                        break;
                    }
                }
            }
            if (found)
            {
                handler.UpdateLocation(handler.GetCurrentLocation(), block);
                handler.UseItemOnHand();
                Thread.Sleep(100);
                handler.ChangeSlot((short)CurrentSlot);
            }
            return found;
        }
    }
}
