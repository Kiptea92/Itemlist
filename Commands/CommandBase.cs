﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

using TwitchToolkit;
using TwitchToolkit.IRC;
using TwitchToolkit.PawnQueue;

using Verse;

namespace SirRandoo.ToolkitUtils.Commands
{
    public class CommandBase : CommandDriver
    {
        private const int MESSAGE_LIMIT = 500;

        public static Pawn GetPawn(string username)
        {
            var component = Current.Game.GetComponent<GameComponentPawns>();
            var query = component.pawnHistory.Keys
                .Where(k => k.EqualsIgnoreCase(username))
                .Select(p => component.pawnHistory[p]);

            return query.Any() ? query.First() : null;
        }

        public static void SendMessage(string message, bool separateRoom)
        {
            if(message.NullOrEmpty()) return;

            var words = message.Split(new char[] { ' ' }, System.StringSplitOptions.None);
            var builder = new StringBuilder();
            var messages = new List<string>();
            var chars = 0;

            foreach(var word in words)
            {
                if(chars + word.Length <= MESSAGE_LIMIT - 3)
                {
                    builder.Append($"{word} ");
                    chars += word.Length + 1;
                }
                else
                {
                    builder.Append("...");
                    messages.Add(builder.ToString());
                    builder.Clear();
                    chars = 0;
                }
            }

            if(builder.Length > 0)
            {
                messages.Add(builder.ToString());
                builder.Clear();
            }

            if(messages.Count > 0)
            {
                foreach(var m in messages)
                {
                    Toolkit.client.SendMessage(m.Trim(), separateRoom);
                }
            }
        }

        public static void SendMessage(string message, IRCMessage ircMessage)
        {
            SendMessage(message, CommandsHandler.SendToChatroom(ircMessage));
        }
    }
}
