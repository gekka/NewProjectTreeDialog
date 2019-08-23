namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.Model
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    abstract class NPDViewChildModelBase : ModelBase, IDisposable
    {
        public abstract string ViewModelTypeName { get; }

        public virtual bool Initialize(IOption option, System.Windows.Controls.ContentControl npdview)
        {
            this.NPDView = npdview;
            IsSelected = IsNpdviewTargetContent(npdview);
            if (IsSelected)
            {
                this.OriginalViewModel = npdview.Content;
            }

            _iNotify = NPDView.DataContext as System.ComponentModel.INotifyPropertyChanged;
            if (_iNotify != null)
            {
                _iNotify.PropertyChanged += _npdviewDataContext_PropertyChanged;
            }
            return true;
        }

        /// <summary></summary>
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { if (_IsSelected != value) { _IsSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
        }
        private bool _IsSelected;

        public object GetNPD_SelectedViewModel()
        {
            var npdviewDataContext = this.NPDView?.DataContext;
            if (npdviewDataContext == null)
            {
                return false;
            }

            var selectedViewModel = npdviewDataContext
                .GetType()
                .GetProperty("SelectedViewModel")?
                .GetValue(npdviewDataContext);
            return selectedViewModel;
        }

        private bool IsNpdviewTargetContent(System.Windows.Controls.ContentControl npdview)
        {
            return npdview?.Content?.GetType().Name == ViewModelTypeName;
        }

        protected bool IsNpdviewTargetContent()
        {
            return this.IsNpdviewTargetContent(this.NPDView);
        }

        private void _npdviewDataContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedViewModel")
            {
                OnOriginalSelectedViewModelChanged();
            }
        }

        protected virtual void OnOriginalSelectedViewModelChanged()
        {
            var svm = GetNPD_SelectedViewModel();

            IsSelected = svm?.GetType().Name == ViewModelTypeName;
            if (_IsSelected)
            {
                this.OriginalViewModel = svm;
            }
        }

        protected System.Windows.Controls.ContentControl NPDView { get; private set; }
        private System.ComponentModel.INotifyPropertyChanged _iNotify;

        /// <summary></summary>
        public object OriginalViewModel
        {
            get
            {
                return _OriginalViewModel;
            }
            set
            {
                if (_OriginalViewModel != value)
                {
                    _OriginalViewModel = value;
                    OnPropertyChanged(nameof(OriginalViewModel));
                }
            }
        }
        private object _OriginalViewModel;

        /// <summary>ViewにあるTextを更新</summary>
        public void UpdateTexts()
        {
            GetTexts(this.NPDView);
        }

        protected abstract bool GetTexts(System.Windows.Controls.ContentControl npdview);

        protected static string GetText(System.Windows.Controls.ContentControl npdview, string name)
        {
            var d = ControlFinder.FindChildren<FrameworkElement>(npdview).FirstOrDefault(_ => _.Name == name);
            string retval = null;
            if (d is System.Windows.Controls.ContentControl)
            {
                retval = ((System.Windows.Controls.ContentControl)d).Content?.ToString();
            }
            if (d is System.Windows.Controls.Primitives.TextBoxBase)
            {
                retval = ((TextBlock)d).Text;
            }
            if (retval != null)
            {
                retval = System.Text.RegularExpressions.Regex.Replace(retval, @"\(_[A-Z]\)", "");
            }
            return retval;
        }

        public virtual void WriteToOption(IOption option)
        {
        }

        private bool disposed = false;
        protected virtual void Dispose(bool isnotFromFinalizer)
        {
            if (_iNotify != null)
            {
                _iNotify.PropertyChanged -= _npdviewDataContext_PropertyChanged;
            }
     

            if (!this.disposed)
            {
                if (isnotFromFinalizer)
                {
                    // ここでマネージドリソース
                }
                // ここでアンマネージドリソース
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NPDViewChildModelBase()
        {
            this.Dispose(false);
        }
    }
}
