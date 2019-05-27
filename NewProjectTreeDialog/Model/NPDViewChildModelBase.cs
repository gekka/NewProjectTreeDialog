namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.Model
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    abstract class NPDViewChildModelBase : ModelBase, IDisposable
    {
        public bool IsNpdviewTargetContent()
        {
            return this.IsNpdviewTargetContent(this.NPDView);
        }

        protected abstract string ViewModelTypeName { get; }

        public bool IsNpdviewSelected
        {
            get
            {
                var npdviewDataContext = this.NPDView?.DataContext;
                if (npdviewDataContext == null)
                {
                    return false;
                }

                var selectedViewModel = npdviewDataContext.GetType().GetProperty("SelectedViewModel")?.GetValue(npdviewDataContext);
                return selectedViewModel?.GetType().Name == ViewModelTypeName;
            }
        }
        public  bool IsNpdviewTargetContent(System.Windows.Controls.ContentControl npdview)
        {
            if (npdview == null)
            {
                return false;
            }
            return npdview.Content?.GetType().Name == ViewModelTypeName;
        }
        public abstract bool Initialize(IOption option, System.Windows.Controls.ContentControl npdview);

        /// <summary></summary>
        public System.Windows.Controls.ContentControl NPDView
        {
            get
            {
                return _NPDView;
            }
            protected set
            {
                if (_NPDView != value)
                {
                    _NPDView = value;
                    OnPropertyChanged(nameof(NPDView));
                    if (value != null)
                    {
                        GetTexts(value);
                    }
                }
            }
        }
        private System.Windows.Controls.ContentControl _NPDView;

        protected abstract bool GetTexts(System.Windows.Controls.ContentControl npdview);

        protected static string GetText(System.Windows.Controls.ContentControl npdview, string name)
        {
            var d = ControlFinder.FindChildren<FrameworkElement>(npdview).FirstOrDefault(_ => _.Name == name);

            if (d is System.Windows.Controls.ContentControl)
            {
                return ((System.Windows.Controls.ContentControl)d).Content?.ToString();
            }
            if (d is System.Windows.Controls.Primitives.TextBoxBase)
            {
                return ((TextBlock)d).Text;
            }
            return null;
        }

        public virtual void WriteToOption(IOption option)
        {
        }

        private bool disposed = false;
        protected virtual void Dispose(bool isnotFromFinalizer)
        {
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
