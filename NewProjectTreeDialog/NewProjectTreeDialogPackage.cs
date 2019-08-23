namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Task = System.Threading.Tasks.Task;
    using Microsoft.VisualStudio.Shell;

#if DEBUG_
#pragma warning disable VSSDK002
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = false)]
#pragma warning restore
#else
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
#endif
#pragma warning disable VSSDK004
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    //[ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids.EmptySolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
#pragma warning restore
    [Guid(NewProjectTreeDialogPackage.PackageGuidString)]
    [ProvideOptionPage(typeof(OptionDialogPage),"Gekka", "NewProjectTreeDialog", 0,0 ,true)]
    public sealed class NewProjectTreeDialogPackage : AsyncPackage
    {
        static NewProjectTreeDialogPackage()
        {
        }

        public NewProjectTreeDialogPackage()
        {
            if (GLOBAL.ServiceProvider == null)
            {
                GLOBAL.ServiceProvider = this;
            }

        }

        /// <summary>  NewProjectTreeDialogPackage GUID string. </summary>
        public const string PackageGuidString = "b938ab7d-0586-45b1-8a2b-aa7032a0b818";

#region Package Members

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff"));
            await base.InitializeAsync(cancellationToken, progress);

            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            GLOBAL.Initialize(this);
            GLOBAL.CommandEvents.BeforeExecute += CommandEvents_BeforeExecute;
       
            var cmd = GLOBAL.DTE.Commands.Item("File.NewProject");
            cmdFileNewProject = new CMD(cmd);

            //BackgroundLoadingのせいでBeforeExecuteをひっかける前にコマンドが実行される可能性がある
            HookDialog();
        }

        CMD cmdFileNewProject;
        class CMD
        {
            public CMD(EnvDTE.Command cmd)
            {
                GLOBAL.Dispatcher.VerifyAccess();
                GUID = cmd.Guid;
                ID = cmd.ID;
            }
            public string GUID;
            public int ID;
        }

        private void CommandEvents_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            if (cmdFileNewProject.GUID == Guid && cmdFileNewProject.ID == ID)
            {
#pragma warning disable VSTHRD001
                ThreadHelper.Generic.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, HookDialog);
#pragma warning restore
            }
        }

        private void HookDialog()
        {
            try
            {
                var odp = (OptionDialogPage)this.GetDialogPage(typeof(OptionDialogPage));
                var tagOpeder = odp.TagTypeOrder;
                var listMode = odp.TemplateListMode;


                var model=new Model.DialogModel();
                //model.CustomProjectTemplatesModel.TagTypeOrder
                model.Initialize((IOption)odp);
            }
            catch (Exception ex)
            {
               Common.ShowError(ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            GLOBAL.CommandEvents.BeforeExecute -= CommandEvents_BeforeExecute;
            base.Dispose(disposing);
        }
#endregion
    }
}
