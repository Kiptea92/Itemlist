﻿using JetBrains.Annotations;
using SirRandoo.ToolkitUtils.Helpers;
using TwitchToolkit;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitUtils.Utils
{
    [UsedImplicitly]
    public class ModConstraint : ConstraintBase
    {
        private readonly string labelText;

        public ModConstraint()
        {
            labelText = "TKUtils.PurgeMenu.Mod".Localize().CapitalizeFirst();
        }

        public override void Draw(Rect canvas)
        {
            SettingsHelper.DrawLabelAnchored(canvas, labelText, TextAnchor.MiddleLeft);
        }

        public override bool ShouldPurge(Viewer viewer)
        {
            return !viewer.mod;
        }
    }
}
