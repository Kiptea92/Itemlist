using System.Collections.Generic;
using System.Linq;
using SirRandoo.ToolkitUtils.Utils;
using TwitchToolkit;
using TwitchToolkit.IRC;
using Verse;

namespace SirRandoo.ToolkitUtils.Commands
{
    public class ResearchCommand : CommandBase
    {
        public override void RunCommand(IRCMessage message)
        {
            if (!CommandsHandler.AllowCommand(message))
            {
                return;
            }

            var query = message.Message.Split(' ').Skip(1).FirstOrDefault();
            ResearchProjectDef target;

            if (query.NullOrEmpty())
            {
                target = Current.Game.researchManager.currentProj;
            }
            else
            {
                target = DefDatabase<ResearchProjectDef>
                    .AllDefsListForReading
                    .FirstOrDefault(p => p.defName.EqualsIgnoreCase(query) || p.label.EqualsIgnoreCase(query));
            }

            if (target == null)
            {
                message.Reply(
                    !query.NullOrEmpty()
                        ? "TKUtils.Responses.Research.QueryInvalid".Translate(query).WithHeader("Research".Translate())
                        : "TKUtils.Responses.Research.None".Translate().WithHeader("Research".Translate())
                );

                return;
            }

            var segments = new List<string>(){
                "TKUtils.Formats.Research.Current".Translate(
                    target.LabelCap.Named("PROJECT"),
                    GenText.ToStringPercent(target.ProgressPercent).Named("PERCENT")
                )
            };

            if (target.prerequisites != null && !target.PrerequisitesCompleted)
            {
                var container = new List<string>();
                var prerequisites = target.prerequisites;

                foreach(var prerequisite in prerequisites)
                {
                    if(prerequisite.IsFinished)
                    {
                        continue;
                    }

                    container.Add(
                        "TKUtils.Formats.Research.Current".Translate(
                            prerequisite.LabelCap.Named("PROJECT"),
                            GenText.ToStringPercent(prerequisite.ProgressPercent).Named("PERCENT")
                        )
                    );
                }

                segments.Add(
                    "TKUtils.Formats.Research.Prerequisites".Translate(
                        string.Join(
                            "TKUtils.Misc.Separators.Inner".Translate(),
                            container
                        ).Named("PREREQUISITES")
                    )
                );
            }

            message.Reply(string.Join("⎮", segments).WithHeader("Research".Translate()));
        }
    }
}
