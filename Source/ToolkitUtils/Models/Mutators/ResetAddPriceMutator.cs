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

using JetBrains.Annotations;
using SirRandoo.ToolkitUtils.Helpers;
using SirRandoo.ToolkitUtils.Interfaces;
using UnityEngine;

namespace SirRandoo.ToolkitUtils.Models
{
    public class ResetAddPriceMutator : IMutatorBase<TraitItem>
    {
        private string resetPriceText;
        public int Priority => 10;

        public void Prepare()
        {
            resetPriceText = "TKUtils.EditorMutator.ResetAddPrice".Localize();
        }

        public void Draw(Rect canvas)
        {
            SettingsHelper.DrawColoredLabel(canvas, resetPriceText, new Color(1f, 0.53f, 0.76f));
        }

        public void Mutate([NotNull] TableSettingsItem<TraitItem> item)
        {
            item.Data.ResetAddPrice();
        }
    }
}
