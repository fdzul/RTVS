﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.BrokerServices;
using static Microsoft.R.Host.Client.NativeMethods;

namespace Microsoft.R.Host.Client.Host {
    internal class RemoteCredentialsDecorator : ICredentialsDecorator {
        private readonly IntPtr _applicationWindowHandle;
        private readonly Credentials _credentials = new Credentials();
        private readonly AutoResetEvent _credentialsValidated = new AutoResetEvent(true);
        private readonly string _authority;
        private readonly AsyncReaderWriterLock _lock;
        private bool _credentialsAreValid;

        public RemoteCredentialsDecorator(IntPtr applicationWindowHandle, Uri brokerUri) {
            _applicationWindowHandle = applicationWindowHandle;
            _authority = new UriBuilder { Scheme = brokerUri.Scheme, Host = brokerUri.Host, Port = brokerUri.Port }.ToString();
            _lock = new AsyncReaderWriterLock();
            _credentialsAreValid = true;
        }

        public NetworkCredential GetCredential(Uri uri, string authType) => new NetworkCredential(_credentials.UserName, _credentials.Password);

        public async Task<IDisposable> LockCredentialsAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            Credentials credentials;

            // If there is already a LockCredentialsAsync request for which there hasn't been a validation yet, wait until it completes.
            // This can happen when two sessions are being created concurrently, and we don't want to pop the credential prompt twice -
            // the first prompt should be validated and saved, and then the same credentials will be reused for the second session.
            var token = await _lock.WriterLockAsync(cancellationToken);
            try {
                var showUI = !Volatile.Read(ref _credentialsAreValid);
                SecurityServices.GetUserCredentials(_authority, showUI, _applicationWindowHandle, out credentials);
            } catch (Exception) {
                token.Dispose();
                throw;
            }

            _credentials.UserName = credentials.UserName;
            _credentials.Password = credentials.Password;
            Volatile.Write(ref _credentialsAreValid, true);

            return Disposable.Create(() => {
                CredUIConfirmCredentials(_authority, Volatile.Read(ref _credentialsAreValid));
                token.Dispose();
            });
        }

        public void InvalidateCredentials() {
            Volatile.Write(ref _credentialsAreValid, false);
        }

        public void OnCredentialsValidated(bool isValid) {
            CredUIConfirmCredentials(_authority, isValid);
            _credentialsValidated.Set();
        }
    }
}