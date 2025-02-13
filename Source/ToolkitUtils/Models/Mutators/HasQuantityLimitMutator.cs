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

using SirRandoo.ToolkitUtils.Interfaces;
using SirRandoo.ToolkitUtils.Models.Tables;
using ToolkitUtils.UX;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitUtils.Models.Mutators;

public class HasQuantityLimitMutator : IMutatorBase<ThingItem>
{
    private string _hasQuantityLimitText;
    private bool _state;

    public int Priority => 1;

    public string Label => "TKUtils.Fields.QuantityLimit".TranslateSimple();

    public void Prepare()
    {
        _hasQuantityLimitText = Label;
    }

    public void Mutate(TableSettingsItem<ThingItem> item)
    {
        if (item.Data.ItemData != null)
        {
            item.Data.ItemData.HasQuantityLimit = _state;
        }
    }

    public void Draw(Rect canvas)
    {
        CheckboxDrawer.DrawCheckbox(canvas, _hasQuantityLimitText, ref _state);
    }
}
