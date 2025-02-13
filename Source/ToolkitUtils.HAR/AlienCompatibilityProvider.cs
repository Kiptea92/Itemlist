﻿using System.Collections.Generic;
using AlienRace;
using RimWorld;
using SirRandoo.ToolkitUtils.Interfaces;
using Verse;

namespace SirRandoo.ToolkitUtils.HAR;

public record AlienCompatibilityProvider(string ModId = "erdelf.HumanoidAlienRaces") : IAlienCompatibilityProvider
{
    /// <inheritdoc/>
    public bool TryReassignBody(Pawn pawn)
    {
        if (pawn.def is not ThingDef_AlienRace alienRace)
        {
            return false;
        }

        AlienPartGenerator generator = alienRace.alienRace.generalSettings.alienPartGenerator;


        if (generator.bodyTypes.NullOrEmpty() || generator.bodyTypes.Contains(pawn.story.bodyType))
        {
            return true;
        }

        List<BodyTypeDef> bodyTypes = generator.bodyTypes.ListFullCopy();

        if (bodyTypes.Count > 0)
        {
            switch (pawn.gender)
            {
                case Gender.Male:
                    bodyTypes.Remove(BodyTypeDefOf.Female);

                    break;
                case Gender.Female:
                    bodyTypes.Remove(BodyTypeDefOf.Male);

                    break;
            }
        }

        pawn.story.bodyType = bodyTypes.TryRandomElement(out BodyTypeDef newBody) ? newBody : BodyTypeDefOf.Thin;

        return true;
    }

    /// <inheritdoc/>
    public bool IsTraitForced(Pawn pawn, string? defName, int degree)
    {
        if (pawn.def is not ThingDef_AlienRace alienRace || alienRace.alienRace.generalSettings.forcedRaceTraitEntries.NullOrEmpty())
        {
            return false;
        }

        foreach (AlienChanceEntry<TraitWithDegree> entry in alienRace.alienRace.generalSettings.forcedRaceTraitEntries)
        {
            if (string.Equals(entry.entry.def.defName, defName) && entry.entry.degree == degree)
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public bool IsTraitDisallowed(Pawn pawn, string defName, int degree)
    {
        if (pawn.def is not ThingDef_AlienRace alienRace || alienRace.alienRace.generalSettings.disallowedTraits.NullOrEmpty())
        {
            return false;
        }

        foreach (AlienChanceEntry<TraitWithDegree> entry in alienRace.alienRace.generalSettings.disallowedTraits)
        {
            if (string.Equals(entry.entry.def.defName, defName) && entry.entry.degree == degree)
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public bool IsTraitAllowed(Pawn pawn, TraitDef traitDef, int degree = -10) =>
        !IsTraitDisallowed(pawn, traitDef.defName, degree) && !IsTraitForced(pawn, traitDef.defName, degree);
}
