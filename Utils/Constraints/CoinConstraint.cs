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

using System;
using SirRandoo.ToolkitUtils.Helpers;
using ToolkitUtils.UX;
using TwitchToolkit;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitUtils.Utils.Constraints;

public class CoinConstraint : ComparableConstraint
{
    private readonly string _labelText = "TKUtils.PurgeMenu.Coins".Localize().CapitalizeFirst();
    private string _buffer = "0";
    private int _coins;
    private bool _valid = true;

    public override void Draw(Rect canvas)
    {
        (Rect labelRect, Rect fieldRect) = canvas.Split(0.7f);
        (Rect buttonRect, Rect inputRect) = fieldRect.Split(0.25f);

        LabelDrawer.Draw(labelRect, _labelText);
        DrawButton(buttonRect);

        if (FieldDrawer.DrawNumberField(inputRect, out int value, ref _buffer, ref _valid))
        {
            _coins = value;
        }
    }

    public override bool ShouldPurge(Viewer viewer)
    {
        switch (Comparison)
        {
            case ComparisonTypes.Equal:
                return viewer.coins == _coins;
            case ComparisonTypes.Greater:
                return viewer.coins > _coins;
            case ComparisonTypes.Less:
                return viewer.coins < _coins;
            case ComparisonTypes.GreaterEqual:
                return viewer.coins >= _coins;
            case ComparisonTypes.LessEqual:
                return viewer.coins <= _coins;
            default:
                return false;
        }
    }
}
