﻿using System.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using SirRandoo.ToolkitUtils.Utils;
using TwitchToolkit.Windows;

namespace SirRandoo.ToolkitUtils.Harmony
{
    [HarmonyPatch(typeof(Window_CommandEditor), "PostClose")]
    [UsedImplicitly]
    public static class CommandEditorPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void PostClose()
        {
            if (TkSettings.Offload)
            {
                Task.Run(ShopExpansion.DumpCommands);
            }
            else
            {
                ShopExpansion.DumpCommands();
            }
        }
    }
}
