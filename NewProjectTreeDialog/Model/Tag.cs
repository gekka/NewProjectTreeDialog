namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.Model
{
    using System;

    class Tag : ModelBase
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public bool Filter
        {
            get { return _Filter; }
            set
            {
                if (_Filter != value)
                {
                    _Filter = value;
                    OnPropertyChanged(nameof(Filter));
                    OnFilterChanged();
                }
            }
        }
        private bool _Filter = true;

        public EventHandler<EventArgs> FilterChanged;
        private void OnFilterChanged()
        {
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsOther { get; set; }

        public override string ToString()
        {
            return this.Value;
        }
    }
}
