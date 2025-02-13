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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using SirRandoo.ToolkitUtils.Helpers;
using SirRandoo.ToolkitUtils.Interfaces;
using SirRandoo.ToolkitUtils.Models;
using Verse;

namespace SirRandoo.ToolkitUtils;

[StaticConstructorOnStartup]
internal static class DomainIndexer
{
    internal static readonly MutatorEntry[] Mutators;
    internal static readonly SelectorEntry[] Selectors;

    private static readonly string[] FilteredNamespaceRoots =
    {
        "System",
        "Unity",
        "Steamworks",
        "Verse",
        "RimWorld",
        "Utf8Json",
        "Mono",
        "RestSharp",
        "SimpleJSON",
        "MoonSharp",
        "TwitchLib",
        "Newtonsoft",
        "HugsLib",
        "HarmonyLib",
        "MS",
        "NAudio",
        "TMPro"
    };

    static DomainIndexer()
    {
        var builder = new StringBuilder();
        var mutators = new List<MutatorEntry>();
        var selectors = new List<SelectorEntry>();

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GlobalAssemblyCache)
            {
                continue;
            }

            try
            {
                ProcessAssembly(assembly, mutators, selectors);
            }
            catch (Exception e)
            {
                builder.Append($"  - {assembly.FullName}  |  Reason: {e.GetType().Name}({e.Message})\n");
            }
        }

        if (builder.Length > 0)
        {
            builder.Insert(0, "The following assemblies could not be processed:\n");
            TkUtils.Logger.Warn(builder.ToString());
        }

        Mutators = mutators.ToArray();
        Selectors = selectors.ToArray();
    }

    private static void ProcessAssembly(Assembly assembly, ICollection<MutatorEntry> mutators, ICollection<SelectorEntry> selectors)
    {
        foreach (Type type in assembly.GetTypes())
        {
            if (type.IsInterface || type.IsAbstract || type.GetTypeInfo().IsDefined(typeof(CompilerGeneratedAttribute), true)
                || FilteredNamespaceRoots.Any(r => type.Namespace?.StartsWith(r) == true))
            {
                continue;
            }

            try
            {
                ProcessType(type, mutators, selectors);
            }
            catch (Exception e)
            {
                // We'll ignore an erroring type as it may just be a one-off thing.
                // We won't report the exception a type threw since it's likely a reflection
                // error that may not hold any valuable information.

                TkUtils.Logger.Error($"Could not process type {type.Name}", e);
            }
        }
    }

    private static void ProcessType(Type type, ICollection<MutatorEntry> mutators, ICollection<SelectorEntry> selectors)
    {
        bool isGeneric = type.IsGenericType || type.GetInterfaces().Any(i => i.IsGenericType);


        if (typeof(ICompatibilityProvider).IsAssignableFrom(type))
        {
            CompatRegistry.ProcessType(type);
        }
        else
        {
            switch (isGeneric)
            {
                case true when GameHelper.IsGenericTypeDeep(type, typeof(ISelectorBase<>), false, typeof(IShopItemBase)):
                    selectors.Add(ProcessSelector(type));

                    break;
                case true when GameHelper.IsGenericTypeDeep(type, typeof(IMutatorBase<>), false, typeof(IShopItemBase)):
                    mutators.Add(ProcessMutator(type));

                    break;
            }
        }
    }

    private static SelectorEntry ProcessSelector(Type selector)
    {
        Type selectorBase = typeof(ISelectorBase<>);
        var entry = new SelectorEntry(selector);

        if (GameHelper.IsGenericTypeDeep(selector, selectorBase, false, typeof(ThingItem)))
        {
            entry.Target = EditorTarget.Item;
        }
        else if (GameHelper.IsGenericTypeDeep(selector, selectorBase, false, typeof(TraitItem)))
        {
            entry.Target = EditorTarget.Trait;
        }
        else if (GameHelper.IsGenericTypeDeep(selector, selectorBase, false, typeof(PawnKindItem)))
        {
            entry.Target = EditorTarget.Pawn;
        }
        else if (GameHelper.IsGenericTypeDeep(selector, selectorBase, false, typeof(EventItem)))
        {
            entry.Target = EditorTarget.Event;
        }
        else
        {
            entry.Target = EditorTarget.Any;
        }

        return entry;
    }

    private static MutatorEntry ProcessMutator(Type mutator)
    {
        Type mutatorBase = typeof(IMutatorBase<>);
        var entry = new MutatorEntry(mutator);

        if (GameHelper.IsGenericType(mutator, mutatorBase, false, typeof(ThingItem)))
        {
            entry.Target = EditorTarget.Item;
        }
        else if (GameHelper.IsGenericType(mutator, mutatorBase, false, typeof(TraitItem)))
        {
            entry.Target = EditorTarget.Trait;
        }
        else if (GameHelper.IsGenericType(mutator, mutatorBase, false, typeof(PawnKindItem)))
        {
            entry.Target = EditorTarget.Pawn;
        }
        else if (GameHelper.IsGenericType(mutator, mutatorBase, false, typeof(EventItem)))
        {
            entry.Target = EditorTarget.Event;
        }
        else
        {
            entry.Target = EditorTarget.Any;
        }

        return entry;
    }

    internal enum EditorTarget { Any, Item, Trait, Pawn, Event }

    internal record SelectorEntry(Type Type)
    {
        internal EditorTarget Target { get; set; }
    }

    internal record MutatorEntry(Type Type)
    {
        internal EditorTarget Target { get; set; }
    }
}
