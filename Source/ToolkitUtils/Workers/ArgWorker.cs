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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using SirRandoo.ToolkitUtils.Helpers;
using SirRandoo.ToolkitUtils.IncidentSettings;
using SirRandoo.ToolkitUtils.Models;
using ToolkitCore.Utilities;
using ToolkitUtils.UX;
using TwitchToolkit;
using Verse;
using Command = TwitchToolkit.Command;

namespace SirRandoo.ToolkitUtils.Workers;

/// <summary>
///     A class for parsing text arguments into concrete objects.
/// </summary>
public class ArgWorker
{
    private readonly Queue<string?> _rawArguments;
    private string? _lastArgument;

    private ArgWorker(IEnumerable<string> rawArguments)
    {
        _rawArguments = new Queue<string?>(rawArguments.Select(a => a.ToToolkit()));
    }

    /// <summary>
    ///     Creates a new <see cref="ArgWorker" /> instance.
    /// </summary>
    /// <param name="rawArguments">The arguments to be parsed</param>
    /// <returns>The <see cref="ArgWorker" /> instance</returns>
    public static ArgWorker CreateInstance(params string[] rawArguments) => new(rawArguments);

    /// <summary>
    ///     Creates a new <see cref="ArgWorker" /> instance.
    /// </summary>
    /// <param name="rawArguments">The arguments to be parsed</param>
    /// <returns>The <see cref="ArgWorker" /> instance</returns>
    public static ArgWorker CreateInstance(IEnumerable<string> rawArguments) => new(rawArguments);

    /// <summary>
    ///     Creates a new <see cref="ArgWorker" /> instance.
    /// </summary>
    /// <param name="input">The raw input to be parsed</param>
    /// <returns>The <see cref="ArgWorker" /> instance</returns>
    public static ArgWorker CreateInstance(string? input) => new(CommandFilter.Parse(input));

    /// <summary>
    ///     Gets the next argument to be parsed.
    /// </summary>
    public string? GetNext()
    {
        if (_rawArguments.Count <= 0)
        {
            return null;
        }

        string? next = _rawArguments.Dequeue();
        _lastArgument = next;

        return next;
    }

    /// <summary>
    ///     The previous argument parsed.
    /// </summary>
    public string? GetLast() => _lastArgument;

    /// <summary>
    ///     Whether there's another argument that can be parsed.
    /// </summary>
    public bool HasNext() => _rawArguments.Count > 0;

    /// <summary>
    ///     Returns the next argument as an integer.
    /// </summary>
    /// <param name="minimum">The minimum value the integer can be</param>
    /// <param name="maximum">The maximum value the integer can be</param>
    /// <returns>The parsed integer clamped to the specified range</returns>
    public int GetNextAsInt(int minimum = 0, int maximum = int.MaxValue)
    {
        string? next = GetNext();

        if (next == null || !int.TryParse(next, out int value))
        {
            return minimum;
        }

        return Math.Max(minimum, Math.Min(value, maximum));
    }

    /// <summary>
    ///     Attempts to get the next argument as an integer.
    /// </summary>
    /// <param name="value">The parsed integer clamped to the specified range</param>
    /// <param name="minimum">The minimum value the integer can be</param>
    /// <param name="maximum">The maximum value the integer can be</param>
    /// <returns>Whether the argument could be parsed as an integer</returns>
    public bool TryGetNextAsInt(out int value, int minimum = 0, int maximum = int.MaxValue)
    {
        string? next = GetNext();

        if (next != null && int.TryParse(next, out value))
        {
            value = Math.Max(minimum, Math.Min(value, maximum));

            return true;
        }

        value = minimum;

        return false;
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="TraitItem" />.
    /// </summary>
    public TraitItem? GetNextAsTrait()
    {
        string? next = GetNext();

        if (next == null || !Data.TryGetTrait(next, out TraitItem trait))
        {
            return null;
        }

        return trait;
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="TraitItem" />
    /// </summary>
    /// <param name="errorCallback">
    ///     A method to call if the argument could
    ///     not be parsed as a <see cref="TraitItem" />. Its sole argument is
    ///     the argument that couldn't be parsed.
    /// </param>
    public TraitItem? GetNextAsTrait(Action<string?> errorCallback)
    {
        TraitItem? trait = GetNextAsTrait();

        if (trait == null)
        {
            errorCallback.Invoke(_lastArgument);
        }

        return trait;
    }

    /// <summary>
    ///     Attempts to get the next argument as a <see cref="TraitItem" />
    /// </summary>
    /// <param name="trait">The parsed <see cref="TraitItem" /></param>
    /// <returns>
    ///     Whether the argument could be parsed as a
    ///     <see cref="TraitItem" />
    /// </returns>
    public bool TryGetNextAsTrait([NotNullWhen(true)] out TraitItem? trait)
    {
        trait = GetNextAsTrait();

        return trait != null;
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="PawnKindItem" />.
    /// </summary>
    public PawnKindItem? GetNextAsPawn()
    {
        string? next = GetNext();

        if (next == null || !Data.TryGetPawnKind(next, out PawnKindItem pawn))
        {
            return null;
        }

        return pawn;
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="PawnKindItem" />.
    /// </summary>
    /// <param name="errorCallback">
    ///     A method to call if the argument could
    ///     not be parsed as a <see cref="PawnKindItem" />. Its sole argument
    ///     is the argument that couldn't be parsed.
    /// </param>
    public PawnKindItem? GetNextAsPawn(Action<string?> errorCallback)
    {
        PawnKindItem? pawn = GetNextAsPawn();

        if (pawn == null)
        {
            errorCallback.Invoke(_lastArgument);
        }

        return pawn;
    }

    /// <summary>
    ///     Attempts to parse the next argument as a
    ///     <see cref="PawnKindItem" />.
    /// </summary>
    /// <param name="pawn">The parsed <see cref="PawnKindItem" /></param>
    /// <returns>
    ///     Whether the argument could be parsed as a
    ///     <see cref="PawnKindItem" />
    /// </returns>
    public bool TryGetNextAsPawn([NotNullWhen(true)] out PawnKindItem? pawn)
    {
        pawn = GetNextAsPawn();

        return pawn != null;
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="Command" />.
    /// </summary>
    public Command? GetNextAsCommand()
    {
        string? next = GetNext();

        if (next == null)
        {
            return null;
        }

        return DefDatabase<Command>.AllDefs.FirstOrDefault(
            c => TkSettings.ToolkitStyleCommands
                ? c.command.StartsWith(next, true, CultureInfo.InvariantCulture)
                : c.command.Equals(next, StringComparison.InvariantCultureIgnoreCase)
        );
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="Command" />
    /// </summary>
    /// <param name="errorCallback">
    ///     A method to call if the argument could
    ///     not be parsed as a <see cref="Command" />. Its sole argument is
    ///     the argument that couldn't be parsed.
    /// </param>
    public Command? GetNextAsCommand(Action<string?> errorCallback)
    {
        Command? command = GetNextAsCommand();

        if (command == null)
        {
            errorCallback.Invoke(_lastArgument);
        }

        return command;
    }

    /// <summary>
    ///     Attempts to parse the next argument as a <see cref="Command" />.
    /// </summary>
    /// <param name="command">The parsed <see cref="Command" /></param>
    /// <returns>
    ///     Whether the argument could be parsed as a
    ///     <see cref="Command" />
    /// </returns>
    public bool TryGetNextAsCommand([NotNullWhen(true)] out Command? command)
    {
        command = GetNextAsCommand();

        return command != null;
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="SkillDef" />.
    /// </summary>
    public SkillDef? GetNextAsSkill()
    {
        string? next = GetNext();

        if (next == null)
        {
            return null;
        }

        return DefDatabase<SkillDef>.AllDefs.FirstOrDefault(
            s => (s.label?.ToToolkit().Equals(next) ?? false) || (s.skillLabel?.ToToolkit().Equals(next, StringComparison.InvariantCultureIgnoreCase) ?? false)
                || s.defName.Equals(next, StringComparison.InvariantCulture)
        );
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="SkillDef" />.
    /// </summary>
    /// <param name="errorCallback">
    ///     A method to call if the argument could
    ///     not be parsed as a <see cref="SkillDef" />. Its sole argument is
    ///     the argument that couldn't be parsed.
    /// </param>
    public SkillDef? GetNextAsSkill(Action<string?> errorCallback)
    {
        SkillDef? skill = GetNextAsSkill();

        if (skill == null)
        {
            errorCallback.Invoke(_lastArgument);
        }

        return skill;
    }

    /// <summary>
    ///     Attempts to parse the next argument as a <see cref="SkillDef" />.
    /// </summary>
    /// <param name="def">The parsed <see cref="SkillDef" /></param>
    /// <returns>
    ///     Whether the argument could be parsed as a
    ///     <see cref="SkillDef" />
    /// </returns>
    public bool TryGetNextAsSkill([NotNullWhen(true)] out SkillDef? def)
    {
        def = GetNextAsSkill();

        return def != null;
    }

    private static ThingItem GetItemRaw(string? input)
    {
        return Data.Items.Find(i => string.Equals(i.DefName, input) || i.Name.Equals(input, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="ItemProxy" />, a
    ///     container containing the main item, the requested quality of the
    ///     item, as well as the requested material of the item.
    /// </summary>
    public ItemProxy? GetNextAsItem()
    {
        string? next = GetNext();

        if (next == null)
        {
            return null;
        }

        if (next.Contains("[") && next.Contains("]"))
        {
            return ProcessMetadata(next);
        }

        return new ItemProxy { Thing = GetItemRaw(next) };
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="ItemProxy" />, a
    ///     container containing the main item, the requested quality of the
    ///     item, as well as the requested material of the item.
    /// </summary>
    /// <param name="errorCallback">
    ///     A method to call if the argument could
    ///     not be parsed as a <see cref="ItemProxy" />. Its sole argument is
    ///     the argument that couldn't be parsed.
    /// </param>
    public ItemProxy? GetNextAsItem(Action<string?> errorCallback)
    {
        ItemProxy? item = GetNextAsItem();

        if (item == null)
        {
            errorCallback.Invoke(_lastArgument);
        }

        return item;
    }

    private static ItemProxy ProcessMetadata(string next)
    {
        var proxy = new ItemProxy();

        string details = next[(next.LastIndexOf('[') + 1)..].TrimEnd(']');
        string? item = next.Replace($"[{details}]", "");
        proxy.Thing = GetItemRaw(item);

        if (proxy.Thing == null)
        {
            proxy.ProcessError = true;

            return proxy;
        }

        foreach (string? segment in details.Split(','))
        {
            if (proxy.Thing.Thing?.race?.Animal == true && TryProcessAnimalMetadata(segment, proxy))
            {
                continue;
            }

            if (TryProcessItemMetadata(segment, proxy))
            {
                continue;
            }

            break;
        }

        return proxy;
    }

    private static bool TryProcessItemMetadata(string? segment, ItemProxy proxy)
    {
        if (Item.Quality && Data.Qualities.TryGetValue(segment, out QualityCategory quality))
        {
            proxy.Quality = quality;

            return true;
        }

        if (Item.Stuff)
        {
            proxy.Stuff = GetItemRaw(segment);
        }

        if (proxy.Stuff != null || proxy.Quality.HasValue)
        {
            return true;
        }

        proxy.ProcessError = true;

        return false;
    }

    private static bool TryProcessAnimalMetadata(string? segment, ItemProxy proxy)
    {
        if (!Item.Gender || !Data.Genders.TryGetValue(segment, out Gender gender))
        {
            return false;
        }

        proxy.Gender = gender;

        return true;
    }

    /// <summary>
    ///     Attempts to parse the next argument as a <see cref="ItemProxy" />,
    ///     a container containing the main item, the requested quality of
    ///     the item, as well as the requested material of the item.
    /// </summary>
    /// <param name="item">The parsed <see cref="ItemProxy" /></param>
    /// <returns>
    ///     Whether the argument could be parsed as a
    ///     <see cref="ItemProxy" />
    /// </returns>
    public bool TryGetNextAsItem([NotNullWhen(true)] out ItemProxy? item)
    {
        item = GetNextAsItem();

        return item != null;
    }

    /// <summary>
    ///     Returns the next argument parsed as a
    ///     <see cref="PawnCapacityDef" />.
    /// </summary>
    public PawnCapacityDef? GetNextAsCapacity()
    {
        string? next = GetNext();

        if (next == null)
        {
            return null;
        }

        return DefDatabase<PawnCapacityDef>.AllDefs.FirstOrDefault(
            c => c.defName.Equals(next) || c.label.ToToolkit().Equals(next, StringComparison.InvariantCultureIgnoreCase)
        );
    }

    /// <summary>
    ///     Returns the next argument parsed as a
    ///     <see cref="PawnCapacityDef" />.
    /// </summary>
    /// <param name="errorCallback">
    ///     A method to call if the argument could
    ///     not be parsed as a <see cref="PawnCapacityDef" />. Its sole
    ///     argument is the argument that couldn't be parsed.
    /// </param>
    public PawnCapacityDef? GetNextAsCapacity(Action<string?> errorCallback)
    {
        PawnCapacityDef? capacityDef = GetNextAsCapacity();

        if (capacityDef == null)
        {
            errorCallback.Invoke(_lastArgument);
        }

        return capacityDef;
    }

    /// <summary>
    ///     Attempts to parse the next argument as a
    ///     <see cref="PawnCapacityDef" />.
    /// </summary>
    /// <param name="capacity">
    ///     The parsed <see cref="PawnCapacityDef" />
    /// </param>
    /// <returns>
    ///     Whether the argument could be parsed as a
    ///     <see cref="PawnCapacityDef" />
    /// </returns>
    public bool TryGetNextAsCapacity([NotNullWhen(true)] out PawnCapacityDef? capacity)
    {
        capacity = GetNextAsCapacity();

        return capacity != null;
    }

    /// <summary>
    ///     Returns the next argument parsed as a <see cref="Viewer" />.
    /// </summary>
    public Viewer? GetNextAsViewer()
    {
        string? next = GetNext();

        if (next == null)
        {
            return null;
        }

        if (next.StartsWith("@"))
        {
            next = next[1..];
        }

        return Viewers.All.Find(v => v.username.Equals(next, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    ///     Returns the next argument parsed as a <see cref="Viewer" />.
    /// </summary>
    /// <param name="errorCallback">
    ///     A method to call if the argument could
    ///     not be parsed as a <see cref="Viewer" />. Its sole argument is the
    ///     argument that couldn't be parsed.
    /// </param>
    public Viewer? GetNextAsViewer(Action<string?> errorCallback)
    {
        Viewer? viewer = GetNextAsViewer();

        if (viewer == null)
        {
            errorCallback.Invoke(_lastArgument);
        }

        return viewer;
    }

    /// <summary>
    ///     Attempts to parse the next argument as a <see cref="Viewer" />.
    /// </summary>
    /// <param name="viewer">The parsed <see cref="Viewer" /></param>
    /// <returns>
    ///     Whether the argument could be parsed as a
    ///     <see cref="Viewer" />
    /// </returns>
    public bool TryGetNextAsViewer([NotNullWhen(true)] out Viewer? viewer)
    {
        viewer = GetNextAsViewer();

        return viewer != null;
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="StatDef" />.
    /// </summary>
    public StatDef? GetNextAsStat()
    {
        string? next = GetNext();

        if (next == null)
        {
            return null;
        }

        return DefDatabase<StatDef>.AllDefs.Where(s => s.showOnHumanlikes && s.showOnPawns)
           .FirstOrDefault(s => s.label.ToToolkit().Equals(next, StringComparison.InvariantCultureIgnoreCase) || s.defName.Equals(next, StringComparison.InvariantCulture));
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="StatDef" />.
    /// </summary>
    /// <param name="errorCallback">
    ///     A method to call if the argument could
    ///     not be parsed as a <see cref="StatDef" />. Its sole argument is
    ///     the argument that couldn't be parsed.
    /// </param>
    public StatDef? GetNextAsStat(Action<string?> errorCallback)
    {
        StatDef? stat = GetNextAsStat();

        if (stat == null)
        {
            errorCallback.Invoke(_lastArgument);
        }

        return stat;
    }

    /// <summary>
    ///     Attempts to parse the next argument as a <see cref="StatDef" />.
    /// </summary>
    /// <param name="stat">The parsed <see cref="StatDef" /></param>
    /// <returns>
    ///     Whether the argument could be parsed as a
    ///     <see cref="StatDef" />
    /// </returns>
    public bool TryGetNextAsStat([NotNullWhen(true)] out StatDef? stat)
    {
        stat = GetNextAsStat();

        return stat != null;
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="ResearchProjectDef" />.
    /// </summary>
    public ResearchProjectDef? GetNextAsResearch()
    {
        string? next = GetNext();

        if (next == null)
        {
            return null;
        }

        return DefDatabase<ResearchProjectDef>.AllDefs.FirstOrDefault(
            p => p.label.ToToolkit().Equals(next, StringComparison.InvariantCultureIgnoreCase) || p.defName.Equals(next, StringComparison.InvariantCulture)
        );
    }

    /// <summary>
    ///     Returns the next argument as a <see cref="ResearchProjectDef" />.
    /// </summary>
    /// <param name="errorCallback">
    ///     A method to call if the argument could
    ///     not be parsed as a <see cref="ResearchProjectDef" />. Its sole
    ///     argument is the argument that couldn't be parsed.
    /// </param>
    public ResearchProjectDef? GetNextAsResearch(Action<string?> errorCallback)
    {
        ResearchProjectDef? proj = GetNextAsResearch();

        if (proj == null)
        {
            errorCallback.Invoke(_lastArgument);
        }

        return proj;
    }

    /// <summary>
    ///     Attempts to parse the next argument as a
    ///     <see cref="ResearchProjectDef" />.
    /// </summary>
    /// <param name="project">
    ///     The parsed <see cref="ResearchProjectDef" />
    /// </param>
    /// <returns>
    ///     Whether the argument could be parsed as a
    ///     <see cref="ResearchProjectDef" />
    /// </returns>
    public bool TryGetNextAsResearch([NotNullWhen(true)] out ResearchProjectDef? project)
    {
        project = GetNextAsResearch();

        return project != null;
    }

    /// <summary>
    ///     Returns all arguments parsed as <see cref="TraitItem" />s.
    /// </summary>
    public IEnumerable<TraitItem> GetAllAsTrait()
    {
        while (HasNext())
        {
            TraitItem? trait = GetNextAsTrait();

            if (trait == null)
            {
                break;
            }

            yield return trait;
        }
    }

    /// <summary>
    ///     Returns all arguments parsed as <see cref="TraitItem" />s.
    /// </summary>
    /// <param name="errorCallback">
    ///     A method to call if an argument could not
    ///     be parsed as a <see cref="TraitItem" />. Its sole argument is the
    ///     argument that couldn't be parsed.
    /// </param>
    /// <returns></returns>
    public IEnumerable<TraitItem> GetAllAsTrait(Action<string?> errorCallback)
    {
        while (HasNext())
        {
            TraitItem? trait = GetNextAsTrait();

            if (trait == null)
            {
                errorCallback.Invoke(_lastArgument);

                break;
            }

            yield return trait;
        }
    }

    /// <summary>
    ///     Returns all arguments parsed as <see cref="ItemProxy" />s, a
    ///     container containing the main item, the requested quality of the
    ///     item, as well as the requested material of the item.
    /// </summary>
    public IEnumerable<ItemProxy> GetAllAsItem()
    {
        while (HasNext())
        {
            ItemProxy? item = GetNextAsItem();

            if (item == null)
            {
                break;
            }

            yield return item;
        }
    }

    /// <summary>
    ///     Returns all arguments parsed as <see cref="ItemProxy" />, a
    ///     container containing the main item, the requested quality of the
    ///     item, as well as the requested material of the item.
    /// </summary>
    /// <param name="errorCallback">
    ///     A method to call if an argument could not
    ///     be parsed as a <see cref="ItemProxy" />. Its sole argument is the
    ///     argument that couldn't be parsed.
    /// </param>
    public IEnumerable<ItemProxy> GetAllAsItem(Action<string?> errorCallback)
    {
        while (HasNext())
        {
            ItemProxy? item = GetNextAsItem();

            if (item == null)
            {
                errorCallback.Invoke(_lastArgument);

                break;
            }

            yield return item;
        }
    }

    /// <summary>
    ///     A class for containing metadata about an item.
    /// </summary>
    public class ItemProxy
    {
        private QualityCategory? _quality;

        /// <summary>
        ///     The main item a viewer specified.
        /// </summary>
        public ThingItem? Thing { get; set; }

        /// <summary>
        ///     The material of <see cref="Thing" />.
        /// </summary>
        public ThingItem Stuff { get; set; }

        /// <summary>
        ///     Whether a specified metadata was invalid for the given item (
        ///     <see cref="Thing" />).
        /// </summary>
        public bool ProcessError { get; set; }

        /// <summary>
        ///     The optional <see cref="QualityCategory" /> of the item.
        /// </summary>
        public QualityCategory? Quality
        {
            get => _quality;
            set
            {
                switch (value)
                {
                    case QualityCategory.Legendary when Item.LegendaryQuality:
                    case QualityCategory.Masterwork when Item.MasterworkQuality:
                    case QualityCategory.Excellent when Item.ExcellentQuality:
                    case QualityCategory.Good when Item.GoodQuality:
                    case QualityCategory.Normal when Item.NormalQuality:
                    case QualityCategory.Poor when Item.PoorQuality:
                    case QualityCategory.Awful when Item.AwfulQuality:
                        _quality = value;

                        break;
                    default:
                        _quality = null;

                        break;
                }
            }
        }

        /// <summary>
        ///     The optional <see cref="Gender" /> of the animal, if the item is
        ///     an animal.
        /// </summary>
        public Gender? Gender { get; set; }

        /// <summary>
        ///     Returns whether or not the specified metadata is valid for the
        ///     specified item.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (Thing == null || ProcessError)
            {
                return false;
            }

            if (!TkSettings.ForceFullItem)
            {
                return true;
            }

            if (Thing.Thing.MadeFromStuff && Stuff == null)
            {
                return false;
            }

            if (Thing.Thing.HasComp(typeof(CompQuality)) && Quality == null)
            {
                return false;
            }

            return true;
        }

        private bool TryGetInvalidSelector([NotNullWhen(true)] out ThingItem? item)
        {
            if (Thing.Cost <= 0 || Thing.Thing == null)
            {
                item = Thing;

                return true;
            }

            if (Stuff != null && (Stuff.Cost <= 0 || Stuff.ItemData?.IsStuffAllowed != true || !Thing.Thing.CanBeStuff(Stuff.Thing)))
            {
                item = Stuff;

                return true;
            }

            item = null;

            return false;
        }

        /// <summary>
        ///     Transforms the <see cref="ItemProxy" /> into a form displayed in
        ///     RimWorld.
        /// </summary>
        /// <param name="plural">Whether <see cref="Thing" /> should be pluralized</param>
        /// <returns>A <see cref="ItemProxy" /> in a form as displayed by RimWorld</returns>
        public string AsString(bool plural = false)
        {
            string? name = (Thing.Thing?.label ?? Thing.Name).ToLowerInvariant();
            string stuff = (Stuff?.Thing?.LabelAsStuff ?? Stuff?.Name)?.ToLowerInvariant() ?? "";

            if (plural)
            {
                name = name.Pluralize();
            }


            return (Quality.HasValue ? $"{stuff} {name} ({RichTextHelper.StripTags(Quality.Value.ToString().ToLowerInvariant())})" : $"{stuff} {name}").Trim();
        }

        /// <summary>
        ///     Attempts to get an error from the specified configuration.
        /// </summary>
        /// <param name="error">The error string from the specified configuration</param>
        /// <returns>Whether there was an error with the configuration</returns>
        public bool TryGetError([NotNullWhen(true)] out string? error)
        {
            if (TryGetInvalidSelector(out ThingItem? item))
            {
                TkUtils.Logger.Debug("Found an invalid selector");

                if (item == Thing)
                {
                    return TryGetThingError(out error);
                }

                if (item == Stuff)
                {
                    return TryGetStuffError(out error);
                }

                error = null;

                return false;
            }

            if (Quality.HasValue && !Thing.Thing.HasComp(typeof(CompQuality)))
            {
                error = "TKUtils.Item.QualityViolation".LocalizeKeyed(Thing.Name);

                return true;
            }

            if (Thing.Thing?.race?.hasGenders == true)
            {
                switch (Thing.Thing.race.hasGenders)
                {
                    case true when Gender is Verse.Gender.None:
                        error = "TKUtils.Item.GenderViolation".LocalizeKeyed(Thing.Name, Gender.ToStringSafe());

                        return true;
                    case false when !(Gender is Verse.Gender.None):
                        error = "TKUtils.Item.GenderViolation".LocalizeKeyed(Thing.Name, Gender.ToStringSafe());

                        break;
                }
            }

            error = null;

            return false;
        }

        [ContractAnnotation("=> true,error:notnull; => false,error:null")]
        private bool TryGetThingError(out string? error)
        {
            if (Thing.Cost <= 0 || Thing.Thing == null)
            {
                error = "TKUtils.Item.Disabled".LocalizeKeyed(Thing.Name);

                return true;
            }

            error = null;

            return false;
        }

        [ContractAnnotation("=> true,error:notnull; => false,error:null")]
        private bool TryGetStuffError(out string? error)
        {
            if (Stuff.Cost <= 0 || Stuff.Thing == null)
            {
                error = "TKUtils.Item.Disabled".LocalizeKeyed(Stuff.Name);

                return true;
            }

            if (!Thing.Thing.MadeFromStuff || !Thing.Thing.CanBeStuff(Stuff.Thing))
            {
                error = "TKUtils.Item.MaterialViolation".LocalizeKeyed(Thing.Name, Stuff.Name);

                return true;
            }

            error = null;

            return false;
        }
    }
}
