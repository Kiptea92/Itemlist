﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using SirRandoo.ToolkitUtils.Helpers;
using SirRandoo.ToolkitUtils.Utils;
using TwitchLib.Client.Models.Interfaces;
using Verse;

namespace SirRandoo.ToolkitUtils.Commands
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)]
    public class PawnStory : CommandBase
    {
        public override void RunCommand(ITwitchMessage twitchMessage)
        {
            if (!PurchaseHelper.TryGetPawn(twitchMessage.Username, out Pawn pawn))
            {
                twitchMessage.Reply("TKUtils.NoPawn".Localize().WithHeader("TabCharacter".Localize()));
                return;
            }

            var parts = new List<string>
            {
                $"{"Backstory".Localize()}: {pawn.story.AllBackstories.Select(b => b.title.CapitalizeFirst()).SectionJoin()}"
            };

            if (!pawn.story.title.NullOrEmpty())
            {
                parts.Add(pawn.story.TitleCap);
            }

            bool isRoyal = pawn.royalty?.MostSeniorTitle != null;
            switch (pawn.gender)
            {
                case Gender.Female:
                    parts.Add(
                        (isRoyal ? ResponseHelper.PrincessGlyph : ResponseHelper.FemaleGlyph).AltText(
                            "Female".Localize().CapitalizeFirst()
                        )
                    );
                    break;
                case Gender.Male:
                    parts.Add(
                        (isRoyal ? ResponseHelper.PrinceGlyph : ResponseHelper.MaleGlyph).AltText(
                            "Male".Localize().CapitalizeFirst()
                        )
                    );
                    break;
                case Gender.None:
                    parts.Add(
                        (isRoyal ? ResponseHelper.CrownGlyph : ResponseHelper.GenderlessGlyph).AltText(
                            "NoneLower".Localize().CapitalizeFirst()
                        )
                    );
                    break;
                default:
                    parts.Add(isRoyal ? ResponseHelper.CrownGlyph : "");
                    break;
            }

            WorkTags workTags = pawn.story.DisabledWorkTagsBackstoryAndTraits;

            if (workTags != WorkTags.None)
            {
                string[] filteredTags = pawn.story.DisabledWorkTagsBackstoryAndTraits.GetAllSelectedItems<WorkTags>()
                   .Where(t => t != WorkTags.None)
                   .Select(t => t.LabelTranslated().CapitalizeFirst())
                   .ToArray();

                parts.Add($"{"IncapableOf".Localize()}: {filteredTags.SectionJoin()}");
            }

            List<Trait> traits = pawn.story.traits.allTraits;

            if (traits.Count > 0)
            {
                parts.Add(
                    $"{"Traits".Localize()}: {traits.Select(t => Unrichify.StripTags(t.LabelCap)).SectionJoin()}"
                );
            }

            twitchMessage.Reply(parts.GroupedJoin().WithHeader("TabCharacter".Localize()));
        }
    }
}
