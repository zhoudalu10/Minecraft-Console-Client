using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinecraftClient.ChatBots
{
    class AutoExit : ChatBot
    {
        public AutoExit()
        {
            LogToConsole("This is open and load.");
        }

        public override void OnUpdateHealth(float health, int food, float foodSaturation)
        {
            if (Settings.AutoExit_Health > 0)
            {
                if (health < Settings.AutoExit_Health)
                {
                    DisconnectAndExit();
                }
            }

            base.OnUpdateHealth(health, food, foodSaturation);
        }

        public override void GetText(string text)
        {
            if (Settings.AutoExit_Message.Length > 0)
            {
                if (text.Contains(Settings.AutoExit_Message))
                {
                    DisconnectAndExit();
                }
            }

            base.GetText(text);
        }
    }
}