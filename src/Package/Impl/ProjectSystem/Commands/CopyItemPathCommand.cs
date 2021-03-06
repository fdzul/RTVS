﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#else
using Microsoft.VisualStudio.ProjectSystem;
#endif

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class CopyItemPathCommand : IAsyncCommandGroupHandler {
        private readonly IRInteractiveWorkflowProvider _interactiveWorkflowProvider;

        [ImportingConstructor]
        public CopyItemPathCommand(IRInteractiveWorkflowProvider interactiveWorkflowProvider) {
            _interactiveWorkflowProvider = interactiveWorkflowProvider;
        }

        public Task<CommandStatusResult> GetCommandStatusAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == RPackageCommandId.icmdCopyItemPath && nodes.IsSingleNodePath()) {
                return Task.FromResult(new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported));
            }
            return Task.FromResult(CommandStatusResult.Unhandled);
        }

        public async Task<bool> TryHandleCommandAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            VsAppShell.Current.AssertIsOnMainThread();
            if (commandId != RPackageCommandId.icmdCopyItemPath) {
                return false;
            }

            var path = nodes.GetSingleNodePath();
            var directory = await _interactiveWorkflowProvider.GetOrCreate().RSession.MakeRelativeToRUserDirectoryAsync(path);
            if (!string.IsNullOrEmpty(directory)) {
                directory.CopyToClipboard();
            }

            return true;
        }
    }
}