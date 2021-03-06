// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Threading;

namespace Microsoft.R.Components.ConnectionManager.ViewModel {
    public interface IConnectionViewModel : IConnectionInfo, INotifyPropertyChanged {
        Uri Id { get; }

        bool IsActive { get; set; }
        bool IsEditing { get; set; }
        bool IsConnected { get; set; }
        CancellationTokenSource TestingConnectionCts { get; set; }
        bool IsTestConnectionSucceeded { get; set; }
        string TestConnectionFailedText { get; set; }

        string SaveButtonTooltip { get; }
        bool IsRemote { get; }
        bool IsValid { get; }
        bool HasChanges { get; }
        
        void Reset();
        string ConnectionTooltip { get; }
    }
}