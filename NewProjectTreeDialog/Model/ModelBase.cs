

namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.Model
{
    class ModelBase : System.ComponentModel.INotifyPropertyChanged
    {
        #region INotifyPropertyChanged メンバ

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            var pc = PropertyChanged;
            if (pc != null)
            {
                pc(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
