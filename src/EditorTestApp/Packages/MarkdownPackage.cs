﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Settings;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Application.Packages {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [Name("Markdown Text View Connection Listener")]
    [Order(Before = "Default")]
    internal sealed class TestMdTextViewConnectionListener : MdTextViewConnectionListener {}

    [ExcludeFromCodeCoverage]
    [Export(typeof(IWritableSettingsStorage))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [Name("Markdown Test settings")]
    [Order(Before = "Default")]
    internal sealed class MdSettingsStorage : SettingsStorage { }
}
