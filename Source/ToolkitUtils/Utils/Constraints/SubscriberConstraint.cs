﻿// ToolkitUtils
// Copyright (C) 2021  SirRandoo
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using SirRandoo.ToolkitUtils.Helpers;
using TwitchToolkit;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitUtils.Utils
{
    public class SubscriberConstraint : ConstraintBase
    {
        private readonly string labelText;

        public SubscriberConstraint()
        {
            labelText = "TKUtils.PurgeMenu.Sub".Localize().CapitalizeFirst();
        }

        public override void Draw(Rect canvas)
        {
            SettingsHelper.DrawLabel(canvas, labelText);
        }

        public override bool ShouldPurge(Viewer viewer)
        {
            return !viewer.IsSub;
        }
    }
}
