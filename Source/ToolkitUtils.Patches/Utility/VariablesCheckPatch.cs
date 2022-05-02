﻿// ToolkitUtils
// Copyright (C) 2022  SirRandoo
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

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using SirRandoo.ToolkitUtils.Helpers;
using TwitchToolkit.Store;

namespace SirRandoo.ToolkitUtils.Harmony
{
    internal static partial class PurchaseHandlerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CheckIfViewerIsInVariableCommandList")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        private static bool CheckIfViewerIsInVariableCommandListPrefix([NotNull] string username, ref bool __result)
        {
            if (!Purchase_Handler.viewerNamesDoingVariableCommands.Contains(username.ToLower()))
            {
                __result = false;

                return false;
            }

            __result = true;
            MessageHelper.ReplyToUser(username, "TKUtils.PausedExtended".LocalizeKeyed(CommandDefOf.UnstickMe.command));

            return false;
        }
    }
}
