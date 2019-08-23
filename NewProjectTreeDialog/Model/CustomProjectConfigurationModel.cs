namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.Model
{
    using System;

    /// <summary>
    /// 2ページ目
    /// </summary>
    class CustomProjectConfigurationModel : NPDViewChildModelBase
    {
        public override string ViewModelTypeName { get; } = "ProjectConfigurationViewModel";

        public override bool Initialize(IOption option, System.Windows.Controls.ContentControl npdview)
        {
            //こっちは元のnpdview.DataContextがテンプレート選択毎に作り直されるっぽい
            return base.Initialize(option, npdview);
        }

        protected override void OnOriginalSelectedViewModelChanged()
        {
            base.OnOriginalSelectedViewModelChanged();
            this.IsEnabled = IsSelected;
#pragma warning disable VSTHRD001
            GLOBAL.Dispatcher.BeginInvoke((Action)this.UpdateTexts, System.Windows.Threading.DispatcherPriority.Input);
#pragma warning restore
        }
        /// <summary>ViewにあるTextを取り込む</summary>
        /// <param name="npdview"></param>
        protected override bool GetTexts(System.Windows.Controls.ContentControl npdview)
        {
            this.ProjectNameLabelText = _ProjectNameLabelText ?? GetText(npdview, "projectNameLabel");
            this.LocationLabelText = _LocationLabelText ?? GetText(npdview, "locationLabel");
            this.SolutionNameLabelText = _SolutionNameLabelText ?? GetText(npdview, "solutionNameLabel");
            this.SameDirLabelText = _SameDirLabelText ?? GetText(npdview, "slnProjInSameDirChk");
            this.FrameworkLabelText = _FrameworkLabelText ?? GetText(npdview, "fxVersionLabel");

            return true;
        }

        public string ProjectNameLabelText
        {
            get { return _ProjectNameLabelText ?? "Project Name"; }
            private set { if (_ProjectNameLabelText != value) { _ProjectNameLabelText = value; OnPropertyChanged(nameof(ProjectNameLabelText)); } }
        }
        private string _ProjectNameLabelText;

        public string LocationLabelText
        {
            get { return _LocationLabelText ?? "Location"; }
            private set { if (_LocationLabelText != value) { _LocationLabelText = value; OnPropertyChanged(nameof(LocationLabelText)); } }
        }
        private string _LocationLabelText;

        public string SolutionNameLabelText
        {
            get { return _SolutionNameLabelText ?? "Solution Name"; }
            private set { if (_SolutionNameLabelText != value) { _SolutionNameLabelText = value; OnPropertyChanged(nameof(SolutionNameLabelText)); } }
        }
        private string _SolutionNameLabelText;

        public string SameDirLabelText
        {
            get { return _SameDirLabelText ?? ""; }
            private set { if (_SameDirLabelText != value) { _SameDirLabelText = value; OnPropertyChanged(nameof(SameDirLabelText)); } }
        }
        private string _SameDirLabelText;

        public string FrameworkLabelText
        {
            get { return _FrameworkLabelText ?? "Framework"; }
            private set { if (_FrameworkLabelText != value) { _FrameworkLabelText = value; OnPropertyChanged(nameof(FrameworkLabelText)); } }
        }
        private string _FrameworkLabelText;


        /// <summary></summary>
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { if (_IsEnabled != value) { _IsEnabled = value; OnPropertyChanged(nameof(IsEnabled)); } }
        }
        private bool _IsEnabled = false;


    }
}
