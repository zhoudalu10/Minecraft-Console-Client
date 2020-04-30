using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MinecraftClient.Mapping;
using MinecraftClient.Inventory;
using System.Threading;

namespace MinecraftClient.Commands
{
    public class MineBot : Command
    {
        public override string CMDName { get { return "startdig"; } }
        public override string CMDDesc { get { return "customize script"; } }

        public override string Run(McTcpClient handler, string command, Dictionary<string, object> localVars)
        {
            //IMinecraftCom handlerProtocol;
            string[] args = getArgs(command);
            if (args.Length == 3)
            {
                int x = int.Parse(args[0]);
                int y = int.Parse(args[1]);
                int z = int.Parse(args[2]);
                Location start = new Location(x, y, z);
                try
                {
                    ConsoleIO.WriteLine("Calculating path to " + start.ToString());
                    if (!handler.MoveTo(start))
                        return "Failed to compute path to " + start;
                    ConsoleIO.WriteLine("Moving to " + start.ToString());
                    while (handler.path != null && handler.path.Count > 0)
                        Thread.Sleep(100);
                    Thread.Sleep(200);
                    Location tLoc;
                    //第一条
                    tLoc = new Location(x, y, z);
                    for (int i = 1; i < 9; i++)
                    {
                        DigSide( handler, false, tLoc, i);
                        Thread.Sleep(100);
                        MoveTo(handler, tLoc + new Location(-i, 0, 0));
                        Thread.Sleep(200);
                    }
                    MoveTo(handler, tLoc + new Location(-8, 0, 0));
                    Thread.Sleep(100);
                    //一 二转角
                    ChangeDIRup(handler, false, tLoc);
                    //第二条
                    tLoc += new Location(-8, 0, 3);
                    for (int i = 0; i < 9; i++)
                    {
                        DigSide(handler, true, tLoc, i);
                        Thread.Sleep(100);
                        MoveTo(handler, tLoc + new Location(i, 0, 0));
                        Thread.Sleep(200);
                    }
                    MoveTo(handler, tLoc + new Location(8, 0, 0));
                    Thread.Sleep(100);
                    //二 三转角
                    ChangeDIRup(handler, true, tLoc);
                    //第三条
                    tLoc += new Location(8, 0, 3);
                    for (int i = 0; i < 8; i++)
                    {
                        DigSide(handler, false, tLoc, i);
                        Thread.Sleep(100);
                        MoveTo(handler, tLoc + new Location(-i, 0, 0));
                        Thread.Sleep(200);
                    }
                    DigSide(handler, true, tLoc + new Location( -8, 0, 0), 0);
                    //三与下层 交界
                    DigSide(handler, false, tLoc + new Location(-8, -1, 0), 0);
                    Thread.Sleep(100);
                    MoveTo(handler, tLoc + new Location(-8, -1, 0));
                    Thread.Sleep(200);
                    //下层 第一条
                    tLoc += new Location(-8, -1, 0);
                    for (int i = 0; i < 9; i++)
                    {
                        DigSide(handler, true, tLoc, i);
                        Thread.Sleep(100);
                        MoveTo(handler, tLoc + new Location(i, 0, 0));
                        Thread.Sleep(200);
                    }
                    MoveTo(handler, tLoc + new Location(8, 0, 0));
                    Thread.Sleep(100);
                    //下层 一 二转角
                    ChangeDIRdown(handler, true, tLoc);
                    //第二条
                    tLoc += new Location(8, 0, -3);
                    for (int i = 0; i < 9; i++)
                    {
                        DigSide(handler, false, tLoc, i);
                        Thread.Sleep(100);
                        MoveTo(handler, tLoc + new Location(-i, 0, 0));
                        Thread.Sleep(200);
                    }
                    MoveTo(handler, tLoc + new Location(-8, 0, 0));
                    Thread.Sleep(100);
                    //下层 二 三转角
                        ChangeDIRdown(handler, false, tLoc);
                    //第三条
                    tLoc += new Location(-8, 0, -3);
                    for (int i = 0; i < 8; i++)
                    {
                        DigSide(handler, true, tLoc, i);
                        Thread.Sleep(100);
                        MoveTo(handler, tLoc + new Location(i, 0, 0));
                        Thread.Sleep(200);
                    }
                    DigSide(handler, true, tLoc + new Location(8, 0, 0), 0);
                    //下下层 交界
                    DigSide(handler, false, tLoc + new Location(8, -1, 0), 0);
                    Thread.Sleep(100);
                    MoveTo(handler, tLoc + new Location(8, -1, 0));
                    Thread.Sleep(200);
                    return "Done!";
                }
                catch (FormatException) { return CMDDesc; }
            }
            else return CMDDesc;
        }

        private void DigSide(McTcpClient handler, bool DIR, Location location, int i)
        {
            if (DIR)
            {
                BreakBlock(handler, location + new Location(i, 0, 0));
                BreakBlock(handler, location + new Location(i, 0, -1));
                BreakBlock(handler, location + new Location(i, 0, 1));

                FindBucketAndUse(handler, 8, location + new Location(i, -1, 0));
                FindBucketAndUse(handler, 8, location + new Location(i, -1, -1));
                FindBucketAndUse(handler, 8, location + new Location(i, -1, 1));
            }
            else
            {
                BreakBlock(handler, location + new Location(-i, 0, 0));
                BreakBlock(handler, location + new Location(-i, 0, -1));
                BreakBlock(handler, location + new Location(-i, 0, 1));

                FindBucketAndUse(handler, 8, location + new Location(-i, -1, 0));
                FindBucketAndUse(handler, 8, location + new Location(-i, -1, -1));
                FindBucketAndUse(handler, 8, location + new Location(-i, -1, 1));
            }
        }

        //这两个函数需要重写, 大量重复代码
        //挖掘两层为一组, 上层时的转向
        private void ChangeDIRup(McTcpClient handler, bool DIR, Location location)
        {
            if (DIR)
            {
                MoveTo(handler, location + new Location(8, 0, 1));
                Thread.Sleep(200);
                BreakBlock(handler, location + new Location(8, 0, 2));
                BreakBlock(handler, location + new Location(8, 0, 3));
                BreakBlock(handler, location + new Location(8, 0, 4));
                FindBucketAndUse(handler, 8, location + new Location(8, -1, 2));
                FindBucketAndUse(handler, 8, location + new Location(8, -1, 3));
                FindBucketAndUse(handler, 8, location + new Location(8, -1, 4));
                Thread.Sleep(200);
                MoveTo(handler, location + new Location(8, 0, 2));
                Thread.Sleep(200);
                MoveTo(handler, location + new Location(8, 0, 3));
            }
            else
            {
                MoveTo(handler, location + new Location(-8, 0, 1));
                Thread.Sleep(200);
                BreakBlock(handler, location + new Location(-8, 0, 2));
                BreakBlock(handler, location + new Location(-8, 0, 3));
                BreakBlock(handler, location + new Location(-8, 0, 4));
                FindBucketAndUse(handler, 8, location + new Location(-8, -1, 2));
                FindBucketAndUse(handler, 8, location + new Location(-8, -1, 3));
                FindBucketAndUse(handler, 8, location + new Location(-8, -1, 4));
                Thread.Sleep(200);
                MoveTo(handler, location + new Location(-8, 0, 2));
                Thread.Sleep(200);
                MoveTo(handler, location + new Location(-8, 0, 3));
            }
        }

        //下层时的转向
        private void ChangeDIRdown(McTcpClient handler, bool DIR, Location location)
        {
            if (DIR)
            {
                MoveTo(handler, location + new Location(8, 0, -1));
                Thread.Sleep(200);
                BreakBlock(handler, location + new Location(8, 0, -2));
                BreakBlock(handler, location + new Location(8, 0, -3));
                BreakBlock(handler, location + new Location(8, 0, -4));
                FindBucketAndUse(handler, 8, location + new Location(8, -1, -2));
                FindBucketAndUse(handler, 8, location + new Location(8, -1, -3));
                FindBucketAndUse(handler, 8, location + new Location(8, -1, -4));
                Thread.Sleep(200);
                MoveTo(handler, location + new Location(8, 0, -2));
                Thread.Sleep(200);
                MoveTo(handler, location + new Location(8, 0, -3));
            }
            else
            {
                MoveTo(handler, location + new Location(-8, 0, -1));
                Thread.Sleep(200);
                BreakBlock(handler, location + new Location(-8, 0, -2));
                BreakBlock(handler, location + new Location(-8, 0, -3));
                BreakBlock(handler, location + new Location(-8, 0, -4));
                FindBucketAndUse(handler, 8, location + new Location(-8, -1, -2));
                FindBucketAndUse(handler, 8, location + new Location(-8, -1, -3));
                FindBucketAndUse(handler, 8, location + new Location(-8, -1, -4));
                Thread.Sleep(200);
                MoveTo(handler, location + new Location(-8, 0, -2));
                Thread.Sleep(200);
                MoveTo(handler, location + new Location(-8, 0, -3));
            }
        }

        private bool FindBucketAndUse(McTcpClient handler, int hotbar, Location block)
        {
            if (!handler.GetWorld().GetBlock(block).Type.IsLiquid())
                return false;
            ConsoleIO.WriteLine("use bucket at " + block.ToString());
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
                Thread.Sleep(100);
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
                if (!found)
                {
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
            }
            if (found)
            {
                handler.UpdateLocation(handler.GetCurrentLocation(), block);
                Thread.Sleep(200);
                handler.UseItemOnHand();
                Thread.Sleep(500);
                if (handler.GetWorld().GetBlock(block).Type.IsLiquid())
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
                    if (found)
                    {
                        Thread.Sleep(200);
                        handler.UseItemOnHand();
                        Thread.Sleep(200);
                    }
                    if (handler.GetWorld().GetBlock(block).Type.IsLiquid())
                    {
                        handler.PlaceBlock(1, block);
                        ConsoleIO.WriteLine("fail to use bucket, place block instead");
                        Thread.Sleep(100);
                    }
                }
                handler.ChangeSlot(CurrentSlot);
            }
            return found;
        }

        private bool BreakBlock(McTcpClient handler, Location goal)
        {
            if (!handler.GetWorld().GetBlock(goal).Type.IsSolid())
                return false;
            handler.DiggingBlock(0, goal);
            return true;
        }

        private void MoveTo(McTcpClient handler, Location goal)
        {
            if (!handler.GetWorld().GetBlock(goal + new Location(0, -1, 0)).Type.IsSolid())
            {
                handler.PlaceBlock(1, goal + new Location(0, -1, 0));
                Thread.Sleep(100);
            }
            if (handler.GetWorld().GetBlock(goal + new Location(0, 1, 0)).Type.IsSolid())
            {
                BreakBlock(handler, goal + new Location(0, 1, 0));
                Thread.Sleep(100);
            }
            goal += new Location(0.5, 0, 0.5);
            handler.UpdateLocation(goal, goal + new Location(0, 1, 0));
            handler.handler.SendLocationUpdate(goal, true, handler.yaw, handler.pitch);
        }
    }
}

/*
 * ToDo
 * 检测背包快满时将所有物品上架到商店
 * 检测到下层为地狱砖时停止运行
 */