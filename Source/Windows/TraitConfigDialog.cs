﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RimWorld;
using SirRandoo.ToolkitUtils.Helpers;
using SirRandoo.ToolkitUtils.Models;
using TwitchToolkit;
using TwitchToolkit.Incidents;
using UnityEngine;
using Verse;

namespace SirRandoo.ToolkitUtils.Windows
{
    public class TraitConfigDialog : Window
    {
        private readonly KarmaType addKarmaType;
        private readonly List<TraitItem> cache = Data.Traits;
        private readonly KarmaType removeKarmaType;

        private bool control;
        private string currentQuery = "";
        private List<string> expanded = new List<string>();
        private int globalAddCost;
        private int globalRemoveCost;
        private string lastQuery = "";
        private List<TraitItem> results;
        private Vector2 scrollPos = Vector2.zero;
        private bool shift;

        private Sorter sorter = Sorter.Name;
        private SortMode sortMode = SortMode.Ascending;

        private TaggedString titleText;
        // private TaggedString nameHeaderText;
        // private TaggedString priceText;
        // private TaggedString priceHeaderText;

        public TraitConfigDialog()
        {
            GetTranslations();

            doCloseX = true;
            globalRemoveCost = 0;
            forcePause = true;
            onlyOneOfTypeAllowed = true;

            optionalTitle = titleText;
            cache?.SortBy(t => t.Name);

            addKarmaType = DefDatabase<StoreIncidentVariables>.GetNamedSilentFail("AddTrait").karmaType;
            removeKarmaType = DefDatabase<StoreIncidentVariables>.GetNamedSilentFail("RemoveTrait").karmaType;

            if (cache == null)
            {
                TkLogger.Warn("The trait shop is null! You should report this.");
            }
        }

        public override Vector2 InitialSize => new Vector2(640f, Screen.height * 0.85f);

        private void GetTranslations()
        {
            titleText = "TKUtils.TraitStore.Title".Localize();
            // nameHeaderText = "TKUtils.Windows.Store.Headers.Name".Translate();
            // priceText = "TKUtils.Windows.Config.Input.Price.Label".Translate();
            // priceHeaderText = "TKUtils.Windows.Store.Headers.Price".Translate();
            // applyText = "TKUtils.Windows.Config.Buttons.Apply.Label".Translate();
            // searchText = "TKUtils.Windows.Config.Buttons.Search.Label".Translate();
            // resetText = "TKUtils.Windows.Config.Buttons.ResetAll.Label".Translate();
            // enableText = "TKUtils.Windows.Config.Buttons.EnableAll.Label".Translate();
            // disableText = "TKUtils.Windows.Config.Buttons.DisableAll.Label".Translate();
        }

        private void DrawHeader(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            float midpoint = inRect.width / 2f;
            var searchRect = new Rect(inRect.x, inRect.y, inRect.width * 0.3f, Text.LineHeight);
            Rect searchLabel = searchRect.LeftHalf();
            Rect searchField = searchRect.RightHalf();

            Widgets.Label(searchLabel, "TKUtils.Buttons.Search".Localize());
            currentQuery = Widgets.TextField(searchField, currentQuery);

            if (currentQuery.Length > 0 && SettingsHelper.DrawClearButton(searchField))
            {
                currentQuery = "";
            }

            if (currentQuery.NullOrEmpty())
            {
                results = null;
                lastQuery = "";
            }


            TaggedString enableText = "TKUtils.Buttons.EnableAll".Localize();
            TaggedString disableText = "TKUtils.Buttons.DisableAll".Localize();
            TaggedString applyText = "TKUtils.Buttons.Apply".Localize();
            Vector2 enableSize = Text.CalcSize(enableText) * 1.5f;
            Vector2 disableSize = Text.CalcSize(disableText) * 1.5f;
            float maxWidth = Mathf.Max(enableSize.x, disableSize.x);
            var disableRect = new Rect(midpoint, inRect.y + Text.LineHeight, maxWidth, Text.LineHeight);
            var enableRect = new Rect(midpoint, inRect.y, maxWidth, Text.LineHeight);

            if (Widgets.ButtonText(enableRect, globalAddCost > 0 ? applyText : enableText))
            {
                foreach (TraitItem trait in Data.Traits)
                {
                    if (globalAddCost > 0)
                    {
                        trait.CostToAdd = globalAddCost;
                    }
                    else
                    {
                        trait.CanAdd = true;
                        trait.CanRemove = true;
                    }
                }

                if (globalAddCost > 0)
                {
                    globalAddCost = 0;
                }
            }

            if (Widgets.ButtonText(disableRect, globalRemoveCost > 0 ? applyText : disableText))
            {
                foreach (TraitItem trait in Data.Traits)
                {
                    if (globalRemoveCost > 0)
                    {
                        trait.CostToRemove = globalRemoveCost;
                    }
                    else
                    {
                        trait.CanAdd = false;
                        trait.CanRemove = false;
                    }
                }

                if (globalRemoveCost > 0)
                {
                    globalRemoveCost = 0;
                }
            }


            var globalAddRect = new Rect(
                enableRect.x + enableRect.width + 5f,
                enableRect.y,
                midpoint - enableRect.width - 5f,
                Text.LineHeight
            );
            var globalRemoveRect = new Rect(
                disableRect.x + disableRect.width + 5f,
                disableRect.y,
                midpoint - disableRect.width - 5f,
                Text.LineHeight
            );

            var globalAddBuffer = globalAddCost.ToString();
            Widgets.Label(globalAddRect.LeftHalf(), "TKUtils.TraitStore.AddCost".Localize());
            Widgets.TextFieldNumeric(globalAddRect.RightHalf(), ref globalAddCost, ref globalAddBuffer);

            if (globalAddCost > 0 && SettingsHelper.DrawClearButton(globalAddRect.RightHalf()))
            {
                globalAddCost = 0;
            }

            var globalRemoveBuffer = globalRemoveCost.ToString();
            Widgets.Label(globalRemoveRect.LeftHalf(), "TKUtils.TraitStore.RemoveCost".Localize());
            Widgets.TextFieldNumeric(globalRemoveRect.RightHalf(), ref globalRemoveCost, ref globalRemoveBuffer);

            if (globalRemoveCost > 0 && SettingsHelper.DrawClearButton(globalRemoveRect.RightHalf()))
            {
                globalRemoveCost = 0;
            }

            GUI.EndGroup();
        }

        private void DrawExpanded(TraitItem trait, Rect inRect)
        {
            trait.Data ??= new TraitData {KarmaTypeForAdding = addKarmaType, KarmaTypeForRemoving = removeKarmaType};

            var firstColumn = new Rect(inRect.x, inRect.y, (inRect.width / 2f) - 5f, Text.LineHeight);
            var secondColumn = new Rect(
                firstColumn.x + firstColumn.width + 10f,
                firstColumn.y,
                firstColumn.width,
                firstColumn.height
            );
            var listing = new Listing_Standard();

            listing.Begin(firstColumn);
            (Rect addKarmaLabel, Rect addKarmaField) = listing.GetRect(Text.LineHeight).ToForm();

            Widgets.Label(addKarmaLabel, "TKUtils.TraitStore.AddKarmaType".Localize());
            if (Widgets.ButtonText(addKarmaField, nameof(trait.Data.KarmaTypeForAdding)))
            {
                Find.WindowStack.Add(
                    new FloatMenu(
                        StoreDialog.KarmaTypes.Select(
                                k => new FloatMenuOption(nameof(k), () => trait.Data.KarmaTypeForAdding = k)
                            )
                           .ToList()
                    )
                );
            }

            (Rect removeKarmaLabel, Rect removeKarmaField) = listing.GetRect(Text.LineHeight).ToForm();
            Widgets.Label(removeKarmaLabel, "TKUtils.TraitStore.RemoveKarmaType".Localize());
            if (Widgets.ButtonText(removeKarmaField, nameof(trait.Data.KarmaTypeForRemoving)))
            {
                Find.WindowStack.Add(
                    new FloatMenu(
                        StoreDialog.KarmaTypes.Select(
                                k => new FloatMenuOption(nameof(k), () => trait.Data.KarmaTypeForRemoving = k)
                            )
                           .ToList()
                    )
                );
            }

            listing.End();

            listing.Begin(secondColumn);
            listing.CheckboxLabeled("TKUtils.TraitStore.BypassLimit".Localize(), ref trait.Data.CanBypassLimit);

            if (listing.ButtonTextLabeled(
                "TKUtils.TraitStore.ResetName".Localize(),
                "TKUtils.Buttons.Reset".Localize()
            ))
            {
                TraitDef traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(trait.DefName);

                trait.Name =
                    (traitDef?.degreeDatas != null ? traitDef.DataAtDegree(trait.Degree).label : traitDef?.label)
                    ?? trait.Name;
                trait.Data.CustomName = traitDef == null;
            }

            listing.End();
        }

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);

            var listing = new Listing_Standard {maxOneColumn = true};
            var headerRect = new Rect(0f, 0f, inRect.width, Text.LineHeight * 2f);

            DrawHeader(headerRect);
            Widgets.DrawLineHorizontal(inRect.x, Text.LineHeight * 3f, inRect.width);

            var contentArea = new Rect(
                inRect.x,
                Text.LineHeight * 4f,
                inRect.width,
                inRect.height - Text.LineHeight * 4f
            );

            TextAnchor old = Text.Anchor;
            List<TraitItem> effectiveList = results ?? cache;
            int total = effectiveList.Count;
            float maxHeight = (Text.LineHeight * 3f + 2f) * total;
            var viewPort = new Rect(0f, 0f, contentArea.width - 16f, maxHeight);
            var traits = new Rect(0f, 0f, contentArea.width, contentArea.height);

            GUI.BeginGroup(contentArea);
            listing.BeginScrollView(traits, ref scrollPos, ref viewPort);

            for (var index = 0; index < effectiveList.Count; index++)
            {
                TraitItem trait = effectiveList[index];
                Rect lineRect = listing.GetRect(Text.LineHeight * 2f + 2f);
                Rect spacerRect = listing.GetRect(Text.LineHeight);
                var labelRect = new Rect(0f, lineRect.y, lineRect.width * 0.6f, lineRect.height);

                if (!lineRect.IsRegionVisible(traits, scrollPos))
                {
                    continue;
                }

                if (index % 2 == 1)
                {
                    Widgets.DrawLightHighlight(
                        new Rect(
                            lineRect.x,
                            lineRect.y - Text.LineHeight / 2f,
                            lineRect.width,
                            lineRect.height + Text.LineHeight
                        )
                    );
                }

                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, trait.Name.CapitalizeFirst());
                Text.Anchor = old;

                var inputRect = new Rect(
                    labelRect.width + 5f,
                    lineRect.y,
                    lineRect.width - labelRect.width - 35f,
                    labelRect.height
                );

                Widgets.Checkbox(inputRect.x, inputRect.y, ref trait.CanAdd);

                var addRect = new Rect(inputRect.x + 28f, inputRect.y, inputRect.width, Text.LineHeight - 1f);

                if (trait.CanAdd)
                {
                    var addBuffer = trait.CostToAdd.ToString();
                    Widgets.TextFieldNumericLabeled(
                        addRect,
                        "TKUtils.TraitStore.AddCost".Localize(),
                        ref trait.CostToAdd,
                        ref addBuffer
                    );
                }
                else
                {
                    Widgets.Label(addRect.LeftHalf(), "TKUtils.TraitStore.AddCost".Localize());

                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(addRect.RightHalf(), trait.CostToAdd.ToString());

                    Text.Anchor = old;
                }

                var removeRect = new Rect(
                    inputRect.x + 28f,
                    inputRect.BottomHalf().y + 1f,
                    inputRect.width,
                    Text.LineHeight
                );

                Widgets.Checkbox(removeRect.x - 28f, removeRect.y, ref trait.CanRemove);

                if (trait.CanRemove)
                {
                    var removeBuffer = trait.CostToRemove.ToString();
                    Widgets.TextFieldNumericLabeled(
                        removeRect,
                        "TKUtils.TraitStore.RemoveCost".Localize(),
                        ref trait.CostToRemove,
                        ref removeBuffer
                    );
                }
                else
                {
                    Widgets.Label(removeRect.LeftHalf(), "TKUtils.TraitStore.RemoveCost".Localize());

                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(removeRect.RightHalf(), trait.CostToRemove.ToString());

                    Text.Anchor = old;
                }

                Color colorCache = GUI.color;
                GUI.color = Color.gray;
                Widgets.DrawLineHorizontal(spacerRect.x, spacerRect.y + spacerRect.height / 2f, spacerRect.width);
                GUI.color = colorCache;
            }

            GUI.EndGroup();
            listing.EndScrollView(ref viewPort);
            GUI.EndGroup();
        }

        private void Notify__SearchRequested()
        {
            lastQuery = currentQuery;

            results = GetSearchResults();
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();

            if (lastQuery.Equals(currentQuery))
            {
                return;
            }

            if (Time.time % 2 < 1)
            {
                Notify__SearchRequested();
            }
        }

        private List<TraitItem> GetSearchResults()
        {
            string serialized = currentQuery?.ToToolkit();

            if (serialized == null)
            {
                return null;
            }

            return cache.Where(
                    t => t.DefName.ToToolkit().EqualsIgnoreCase(currentQuery.ToToolkit())
                         || t.DefName.ToToolkit().Contains(currentQuery.ToToolkit())
                )
               .ToList();
        }

        public override void PreClose()
        {
            if (TkSettings.Offload)
            {
                Task.Run(
                    () =>
                    {
                        switch (TkSettings.DumpStyle)
                        {
                            case "MultiFile":
                                Data.SaveTraits(Paths.TraitFilePath);
                                return;
                            case "SingleFile":
                                Data.SaveLegacyShop(Paths.LegacyShopDumpFilePath);
                                return;
                        }
                    }
                );
            }
            else
            {
                switch (TkSettings.DumpStyle)
                {
                    case "MultiFile":
                        Data.SaveTraits(Paths.TraitFilePath);
                        return;
                    case "SingleFile":
                        Data.SaveLegacyShop(Paths.LegacyShopDumpFilePath);
                        return;
                }
            }
        }
    }
}
