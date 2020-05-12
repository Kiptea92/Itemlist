﻿using System.Linq;
using RimWorld;
using SirRandoo.ToolkitUtils.Utils;
using ToolkitCore.Utilities;
using TwitchLib.Client.Models.Interfaces;
using TwitchToolkit;
using Verse;
using Verse.AI;

namespace SirRandoo.ToolkitUtils.Commands
{
    public class InsultCommand : CommandBase
    {
        public override void RunCommand(ITwitchMessage twitchMessage)
        {
            var pawn = GetOrFindPawn(twitchMessage.Username);
            var data = Viewers.GetViewer(twitchMessage.Username);

            if (pawn == null)
            {
                twitchMessage.Reply("TKUtils.Responses.NoPawn".Translate());
                return;
            }

            var query = CommandFilter.Parse(twitchMessage.Message).Skip(1).FirstOrFallback("");
            Pawn target = null;

            if (!query.NullOrEmpty())
            {
                if (query.StartsWith("@"))
                {
                    query = query.Substring(1);
                }

                var viewer = Viewers.All.FirstOrDefault(v => v.username.EqualsIgnoreCase(query));

                if (viewer == null)
                {
                    return;
                }

                target = GetOrFindPawn(viewer.username);

                if (target == null)
                {
                    twitchMessage.Reply("TKUtils.Responses.ViewerNotFound".Translate(query));
                    return;
                }
            }

            target ??= Find.ColonistBar.Entries.RandomElement().pawn;
            var job = new Job(JobDefOf.Insult, target);

            if (job.CanBeginNow(pawn))
            {
                data.SetViewerKarma(data.karma - 15);

                pawn.jobs.StartJob(job, JobCondition.InterruptForced);
            }
        }
    }
}
