﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MinecraftClient.ChatBots
{
    class AutoFish : ChatBot
    {
        private static short QUICK_BAR_START = 36;
        private short[] QuickBarItem = new short[9];
        private short Slot = -1;
        private int ProtocolVersion;
        private DateTime LastTime = DateTime.Now;
        private bool Fishing = false;
        private int FishrowEntityId = 0;
        private int FishNumber = 0;
        private int FishRodId;
        private int EntityTypeId;
        private bool CanFishFlag;

        public AutoFish()
        {
            LogToConsole("AutoFish is open and load.");
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void AfterGameJoined()
        {
            LogToConsole("Process will run in 10 seconds...");
            Thread.Sleep(10000);
            LogToConsole("Process start.");
            ProtocolVersion = GetProtocolVersion();
            CanFishFlag = GetVersionFlag();
            FishRodId = GetFishRodId();
            EntityTypeId = GetEntityTypeId();
            base.AfterGameJoined();
        }

        public override void OnHeldItemSlot(short slot)
        {
            Slot = slot;
            AutoSwitchToFishRod();
            base.OnHeldItemSlot(slot);
        }

        public override void OnSetSlot(byte windowId, short slot, short itemId, short itemCount)
        {
            if (windowId == 0 && slot >= QUICK_BAR_START && slot < QUICK_BAR_START + 9)
            {
                slot -= QUICK_BAR_START;
                QuickBarItem[slot] = itemId;
                AutoSwitchToFishRod();
            }

            base.OnSetSlot(windowId, slot, itemId, itemCount);
        }

        public override void Update()
        {
            TimeSpan interval = DateTime.Now - LastTime;
            int time = (int) ((DateTime.Now - LastTime).TotalSeconds);
            if (Fishing)
            {
                if (time > Settings.AutoFish_Timeout)
                {
                    Fishing = false;
                    UseFishRod();
                    LogToConsole("Fishing time out.");
                }
            }
            else
            {
                if (time >= Settings.AutoFish_Delay && CanFish())
                {
                    UseFishRod();
                }
            }

            base.Update();
        }

        public override void OnSpawnEntity(int entityId, short type, Guid UUID, Mapping.Location location)
        {
            if (type == EntityTypeId)
            {
                TimeSpan interval = DateTime.Now - LastTime;
                if (interval.TotalMilliseconds <= 500)
                {
                    FishrowEntityId = entityId;
                    Fishing = true;
                }
            }

            base.OnSpawnEntity(entityId, type, UUID, location);
        }

        public override void OnEntityDestroy(int[] entitys)
        {
            if (Fishing && FishrowEntityId > 0)
            {
                for (int i = 0; i < entitys.Length; i++)
                {
                    if (entitys[i] == FishrowEntityId)
                    {
                        Fishing = false;
                        FishrowEntityId = 0;
                    }
                }
            }

            base.OnEntityDestroy(entitys);
        }

        public override void OnEntityMoveLook(int entityId, short dX, short dY, short dZ)
        {
            if (Fishing && FishrowEntityId == entityId)
            {
                if (dX == 0 && dZ == 0 && dY < -800)
                {
                    UseFishRod();
                    LogToConsole("You've caught " + ++FishNumber + " fish.");
                }
            }

            base.OnEntityMoveLook(entityId, dX, dY, dZ);
        }

        private void UseFishRod()
        {
            LogToConsole("Use fish rod!");
            LastTime = DateTime.Now;
            UseItem(0);
        }

        private bool CanFish()
        {
            if (Slot < 0 || Slot > 8) return false;
            if (CanFishFlag)
            {
                return QuickBarItem[Slot] == FishRodId;
            }

            return false;
        }

        private void AutoSwitchToFishRod()
        {
            if (Slot < 0 || Slot > 8) return;
            if (CanFishFlag)
            {
                if (QuickBarItem[Slot] != FishRodId)
                {
                    for (short i = 0; i < QuickBarItem.Length; i++)
                    {
                        if (QuickBarItem[i] == FishRodId)
                        {
                            HeldItemSlot(i);
                        }
                    }
                }
            }
        }

        private int GetFishRodId()
        {
            switch (ProtocolVersion)
            {
                case 404:
                    return 568;
                case 498:
                    return 622;
                case 575:
                    return 622;
                default:
                    return 0;
            }
        }

        private int GetEntityTypeId()
        {
            switch (ProtocolVersion)
            {
                case 404:
                    return 90;
                case 498:
                    return 101;
                case 575:
                    return 102;
                default:
                    return 0;
            }
        }

        private bool GetVersionFlag()
        {
            switch (ProtocolVersion)
            {
                case 404:
                    return true;
                case 498:
                    return true;
                case 575:
                    return true;
                default:
                    return false;
            }
        }
    }
}