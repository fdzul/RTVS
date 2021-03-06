﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Editor.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Markdown {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class RmdClassificationTest : IDisposable {
        private static bool _regenerateBaselineFiles = false;

        private readonly IExportProvider _exportProvider;
        private readonly EditorHostMethodFixture _editorHost;
        private readonly EditorAppTestFilesFixture _files;

        public RmdClassificationTest(REditorApplicationMefCatalogFixture catalogFixture, EditorHostMethodFixture editorHost, EditorAppTestFilesFixture files) {
            _exportProvider = catalogFixture.CreateExportProvider();
            _editorHost = editorHost;
            _files = files;
        }

        public void Dispose() {
            _exportProvider.Dispose();
        }

        [CompositeTest]
        [Category.Interactive]
        [InlineData("01.rmd")]
        public async Task RColors(string fileName) {
            string content = _files.LoadDestinationFile(fileName);
            using (var script = await _editorHost.StartScript(_exportProvider, content, fileName, null)) {
                script.DoIdle(500);
                var spans = script.GetClassificationSpans();
                var actual = ClassificationWriter.WriteClassifications(spans);
                VerifyClassifications(fileName, actual);
            }
        }

        public void VerifyClassifications(string testFileName, string actual) {
            var testFilePath = _files.GetDestinationPath(testFileName);
            string baselineFile = testFilePath + ".colors";

            if (_regenerateBaselineFiles) {
                baselineFile = Path.Combine(_files.SourcePath, testFileName) + ".colors";
                TestFiles.UpdateBaseline(baselineFile, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFile, actual);
            }
        }
    }
}
