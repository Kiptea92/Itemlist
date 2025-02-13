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

using System;
using JetBrains.Annotations;
using TwitchToolkit.Commands;
using Verse;
using Command = TwitchToolkit.Command;

namespace SirRandoo.ToolkitUtils.Windows;

public class CommandCategoryWindow : CategoricalEditorWindow<Command>
{
    /// <inheritdoc/>
    public CommandCategoryWindow() : base("TKUtils.Headers.AllCommands".TranslateSimple())
    {
    }

    /// <inheritdoc/>
    protected override bool VisibleInSearch(Command entry)
    {
        if (entry.command.IndexOf(SearchWidget.filter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        return entry.defName.IndexOf(SearchWidget.filter.Text, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <inheritdoc/>
    protected override bool IsEntryDisabled(Command entry) => !entry.enabled;

    /// <inheritdoc/>
    protected override void OpenEditorFor(Command entry)
    {
        Find.WindowStack.Add(new CommandEditorDialog(entry));
    }

    /// <inheritdoc/>
    protected override void DisableEntry(Command entry)
    {
        entry.enabled = false;

        CommandEditor.SaveCopy(entry);
    }

    /// <inheritdoc/>
    protected override void ResetEntry(Command entry)
    {
        CommandEditor.LoadBackup(entry);
        CommandEditor.SaveCopy(entry);
    }
}