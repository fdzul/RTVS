﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Package.Registration;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell;
using Microsoft.VisualStudio.R.Languages;
using Microsoft.VisualStudio.R.Package;
using Microsoft.VisualStudio.R.Package.Editors;
using Microsoft.VisualStudio.R.Package.Options.R;
using Microsoft.VisualStudio.R.Package.Options.R.Editor;
using Microsoft.VisualStudio.R.Package.Packages;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Packages.R
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(RGuidList.RPackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideEditorExtension(typeof(REditorFactory), ".r", 0x32, NameResourceID = 106)]
    [ProvideEditorFactory(typeof(REditorFactory), 106, CommonPhysicalViewAttributes = 0x2, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorLogicalView(typeof(REditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    [ProvideLanguageService(typeof(RLanguageService), RContentTypeDefinition.LanguageName, 106, ShowSmartIndent = true)]
    [ProvideLanguageEditorOptionPage(typeof(REditorOptionsDialog), RContentTypeDefinition.LanguageName, "", "Advanced", "#20136")]
    [ProvideProjectFileGenerator(typeof(RProjectFileGenerator), RGuidList.CpsProjectFactoryGuidString, FileExtensions = RContentTypeDefinition.RStudioProjectExtension, DisplayGeneratorFilter = 300)]
    [ProvideCpsProjectFactory(RGuidList.CpsProjectFactoryGuidString, RContentTypeDefinition.LanguageName)]
    [ProvideOptionPage(typeof(RToolsOptionsPage), "R Tools", "Advanced", 20116, 20136, true)]
    [ProvideInteractiveWindow(RGuidList.ReplInteractiveWindowProviderGuidString, Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Bottom, Window = ToolWindowGuids80.Outputwindow, DocumentLikeTool = true)]
    internal sealed class RPackage : BasePackage<RLanguageService>
    {
        public const string OptionsDialogName = "R Tools";

        private Lazy<RInteractiveWindowProvider> _interactiveWindowProvider = new Lazy<Package.Repl.RInteractiveWindowProvider>(() => new RInteractiveWindowProvider());

        protected override void Initialize()
        {
            base.Initialize();

            IComponentModel componentModel = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
            RToolsSettings.VerifyRIsInstalled(componentModel.DefaultExportProvider);
            ReplShortcutSetting.Initialize();

            FunctionIndex.BuildIndexAsync();
        }

        protected override void Dispose(bool disposing)
        {
            //FunctionIndex.SaveIndexAsync();
            ReplShortcutSetting.Close();
            base.Dispose(disposing);
        }

        protected override IEnumerable<IVsEditorFactory> CreateEditorFactories()
        {
            yield return new REditorFactory(this);
        }

        protected override IEnumerable<IVsProjectGenerator> CreateProjectFileGenerators()
        {
            yield return new RProjectFileGenerator();
        }

        protected override IEnumerable<IVsProjectFactory> CreateProjectFactories()
        {
            yield break;
        }

        protected override IEnumerable<MenuCommand> CreateMenuCommands()
        {
            yield return new MenuCommand(
                (sender, args) => _interactiveWindowProvider.Value.Open(instanceId: 0, focus: true),
                new CommandID(RGuidList.RInteractiveCommandSetGuid, 0x0100));
        }

        protected override object GetAutomationObject(string name)
        {
            if (name == RPackage.OptionsDialogName)
            {
                DialogPage page = GetDialogPage(typeof(REditorOptionsDialog));
                return page.AutomationObject;
            }

            return base.GetAutomationObject(name);
        }

        protected override int CreateToolWindow(ref Guid toolWindowType, int id)
        {
            if (toolWindowType == RGuidList.ReplInteractiveWindowProviderGuid)
            {
                IVsInteractiveWindow result = _interactiveWindowProvider.Value.Create(id);
                return result != null ? VSConstants.S_OK : VSConstants.E_FAIL;
            }

            return base.CreateToolWindow(ref toolWindowType, id);
        }
    }
}