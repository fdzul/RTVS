﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Wpf.Threading;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Threading {
    public class MainThreadAwaitableTest {
        private readonly IMainThread _mainThread;

        public MainThreadAwaitableTest() {
            _mainThread = new TestMainThread(UIThreadHelper.Instance);
        }

        [Test]
        public void IsCompleleted_BackgroundThread() {
            var awaitable = new MainThreadAwaitable(_mainThread);

            Thread.CurrentThread.ManagedThreadId.Should().NotBe(_mainThread.ThreadId);
            awaitable.GetAwaiter().IsCompleted.Should().Be(false);
        }

        [Test(ThreadType.UI)]
        public void IsCompleleted_DispatcherThread() {
            var awaitable = new MainThreadAwaitable(_mainThread);

            Thread.CurrentThread.ManagedThreadId.Should().Be(_mainThread.ThreadId);
            awaitable.GetAwaiter().IsCompleted.Should().Be(true);
        }

        [Test]
        public async Task Await_BackgroundThread() {
            var awaitable = new MainThreadAwaitable(_mainThread);

            await awaitable;

            Thread.CurrentThread.ManagedThreadId.Should().Be(_mainThread.ThreadId);
            Action a = () => awaitable.GetAwaiter().GetResult();
            a.ShouldNotThrow();
        }

        [Test(ThreadType.UI)]
        public async Task Await_OnDispatcherThread() {
            var awaitable = new MainThreadAwaitable(_mainThread);

            await awaitable;

            Thread.CurrentThread.ManagedThreadId.Should().Be(_mainThread.ThreadId);
            Action a = () => awaitable.GetAwaiter().GetResult();
            a.ShouldNotThrow();
        }

        [Test]
        public void GetResult_ThrowOnBackgroundThread() {
            var awaitable = new MainThreadAwaitable(_mainThread);

            Action a = () => awaitable.GetAwaiter().GetResult();
            a.ShouldThrow<InvalidOperationException>();
        }
        
        private sealed class TestMainThread : IMainThread {
            private readonly UIThreadHelper _threadHelper;

            public TestMainThread(UIThreadHelper threadHelper) {
                _threadHelper = threadHelper;
            }

            public int ThreadId => _threadHelper.Thread.ManagedThreadId;
            public void Post(Action action) => _threadHelper.InvokeAsync(action).DoNotWait();
        }
    }
}
