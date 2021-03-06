﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core;
using Microsoft.Common.Wpf.Imaging;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDeviceCopyAsBitmapCommand : PlotDeviceCommand, IAsyncCommand {
        public PlotDeviceCopyAsBitmapCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent)
            : base(interactiveWorkflow, visualComponent) {
        }

        public CommandStatus Status {
            get {
                if (HasCurrentPlot && !IsInLocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            string filePath = Path.GetTempFileName();
            try {
                await InteractiveWorkflow.Plots.ExportToBitmapAsync(
                    VisualComponent.ActivePlot,
                    "bmp",
                    filePath,
                    VisualComponent.Device.PixelWidth,
                    VisualComponent.Device.PixelHeight,
                    VisualComponent.Device.Resolution);

                InteractiveWorkflow.Shell.DispatchOnUIThread(() => {
                    try {
                        var image = BitmapImageFactory.Load(filePath);
                        Clipboard.SetImage(image);
                    } catch (Exception e) when (!e.IsCriticalException()) {
                        InteractiveWorkflow.Shell.ShowErrorMessage(string.Format(Resources.Plots_CopyToClipboardError, e.Message));
                    } finally {
                        try {
                            File.Delete(filePath);
                        } catch (IOException) {
                        }
                    }
                });
            } catch (RPlotManagerException ex) {
                InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
            } catch (OperationCanceledException) {
            }

            return CommandResult.Executed;
        }
    }
}
