using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MinecraftClient.Commands;

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
        private int ContinuousUseFishrod = 0;
        private int SpawnEntityDelay = 500;

        public AutoFish()
        {
            LogToConsole("AutoFish is open and load.");
        }

        public override void Initialize()
        {
            if (Settings.AutoFish_Mode == 1)
            {
                SpawnEntityDelay = 3000;
                Settings.AutoFish_Delay = 3;
            }

            base.Initialize();
        }

        public override void AfterGameJoined()
        {
            ContinuousUseFishrod = 0;
            ProtocolVersion = GetProtocolVersion();
            CanFishFlag = GetVersionFlag();
            if (!CanFishFlag)
            {
                LogToConsole("AutoFish does not support this version.");
                UnloadBot();
            }

            FishRodId = GetFishRodId();
            EntityTypeId = GetEntityTypeId();
            if (Settings.AutoFish_Command.Length > 0)
            {
                SendText(Settings.AutoFish_Command);
            }

            Fishing = false;
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
                    ContinuousUseFishrod++;
                    if (ContinuousUseFishrod > 15)
                    {
                        LogToConsole("Server is too busy or there is no water to fish, exit the client.");
                        DisconnectAndExit();
                    }
                }
            }

            base.Update();
        }

        public override void GetText(string text)
        {
            if (Settings.AutoFish_Message.Length > 0)
            {
                if (text.Contains(Settings.AutoFish_Message))
                {
                    Relogin();
                }
            }

            base.GetText(text);
        }

        public override void OnSpawnEntity(int entityId, short type, Guid UUID, Mapping.Location location)
        {
            if (!Fishing)
            {
                if (type == EntityTypeId)
                {
                    TimeSpan interval = DateTime.Now - LastTime;
                    if (interval.TotalMilliseconds <= SpawnEntityDelay)
                    {
                        FishrowEntityId = entityId;
                        Fishing = true;
                    }
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
                    ContinuousUseFishrod = 0;
                    if (Settings.AutoFish_Amount > 0)
                    {
                        if (FishNumber % Settings.AutoFish_Amount == 0)
                        {
                            Relogin();
                        }
                    }
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
                case 578:
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
                case 578:
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
                case 578:
                    return true;
                default:
                    return false;
            }
        }

        private void Relogin()
        {
            LogToConsole("Relogin process is running.");
            ReconnectToTheServer(1, 0);
        }
    }
}