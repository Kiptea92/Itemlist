﻿// MIT License
// 
// Copyright (c) 2022 SirRandoo
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using System.Linq;
using SirRandoo.ToolkitUtils.Interfaces;
using SirRandoo.ToolkitUtils.Models;
using Verse;

namespace SirRandoo.ToolkitUtils;

public static partial class Data
{
    /// <summary>
    ///     The various surgeries available for purchase through the mod's
    ///     surgery store.
    /// </summary>
    public static List<SurgeryItem> Surgeries { get; private set; }

    private static void ValidateSurgeryList()
    {
        Surgeries = new List<SurgeryItem>();

        foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs)
        {
            ISurgeryHandler handler = CompatRegistry.AllSurgeryHandlers.FirstOrDefault(i => i.IsSurgery(recipe));

            if (handler == null)
            {
                continue;
            }

            Surgeries.Add(new SurgeryItem { Surgery = recipe, Handler = handler });
        }
    }
}