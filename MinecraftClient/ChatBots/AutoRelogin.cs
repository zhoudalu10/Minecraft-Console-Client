﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinecraftClient.ChatBots
{
    class AutoRelogin : ChatBot
    {
        private int TryCount = 0;
        private bool Open = false;
        private System.Timers.Timer Timer = new System.Timers.Timer();

        public AutoRelogin()
        {
            LogToConsole("This is open and load.");
        }

        public override void Initialize()
        {
            Timer.Enabled = false;
            Timer.Stop();
            Timer.Interval = Settings.AutoRelogin_Delay * 1000;
            Timer.AutoReset = true;
            Timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerHandler);
        }

        public override void AfterGameJoined()
        {
            Timer.Enabled = false;
            Timer.Stop();
            Open = true;
            TryCount = 0;
            if (Settings.AutoRelogin_Command.Length > 0)
            {
                SendText(Settings.AutoRelogin_Command);
            }
        }

        public override bool OnDisconnect(DisconnectReason reason, string message)
        {
            if (Open)
            {
                TryCount = 0;
                Timer.Enabled = true;
                Timer.Start();
                LogToConsole("AutoRelogin is run, Please wait " + Settings.AutoRelogin_Delay + "s.");
            }

            return base.OnDisconnect(reason, message);
        }

        protected void TimerHandler(object source, System.Timers.ElapsedEventArgs e)
        {
            TryCount += 1;
            if (Settings.AutoRelogin_Retries != -1 && TryCount > Settings.AutoRelogin_Retries)
            {
                LogToConsole("After " + (TryCount - 1) + " attempts, not login, done.");
                Timer.Enabled = false;
                Timer.Stop();
            }
            else
            {
                LogToConsole(TryCount + " attempt to connect");
                ReconnectToTheServer(1, 0);
            }
        }
    }
}