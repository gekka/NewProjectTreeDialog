namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    class TreeNodeItem : ModelBase
    {
        public System.Collections.ObjectModel.ObservableCollection<TreeNodeItem> Items { get; }
            = new System.Collections.ObjectModel.ObservableCollection<TreeNodeItem>();

        public List<TreeNodeItem> FilteredItems { get; private set; } = new List<TreeNodeItem>();

        public int OrderIndex { get; set; }

        /// <summary>Data識別用のID</summary>
        public string Id { get; set; }

        /// <summary>ツリーのヘッダ</summary>
        public object Header { get; set; }

        /// <summary>このノードを分類するためのタグ</summary>
        public IEnumerable<Tag> Tags { get { return _Tags; } }
        private List<Tag> _Tags = new List<Tag>();

        internal void AddTag(Tag tag)
        {
            ((List<Tag>)this.Tags).Add(tag);
            tag.FilterChanged += OnTagFilterChanged;

            UpdateIsVisible();
        }
        internal void insertTag(int index,Tag tag)
        {           
            _Tags.Insert(index,tag);
            tag.FilterChanged += OnTagFilterChanged;

            UpdateIsVisible();
        }

        internal int RemoveTag(Tag tag)
        {
            int index = this._Tags.IndexOf(tag);
            tag.FilterChanged -= OnTagFilterChanged;

            ((List<Tag>)this.Tags).Remove(tag);
            UpdateIsVisible();
            return index;
        }


        public IEnumerable<Tag> LanguageTags
        {
            get { return this.Tags.Where(_ => _.Type == TagTypeKey.Language); }
        }
        public IEnumerable<Tag> PlatformTags
        {
            get { return this.Tags.Where(_ => _.Type == TagTypeKey.Platform); }
        }
        public IEnumerable<Tag> ProjectTypeTags
        {
            get { return this.Tags.Where(_ => _.Type == TagTypeKey.ProjectType); }
        }


        private void OnTagFilterChanged(object sender, EventArgs e)
        {
            UpdateIsVisible();
        }

        /// <summary></summary>
        public bool IsVisible
        {
            get { return _IsVisible; }
            set { if (_IsVisible != value) { _IsVisible = value; OnPropertyChanged(nameof(IsVisible)); } }
        }
        private bool _IsVisible;

        protected void UpdateIsVisible()
        {
            this.IsVisible = GetVisibleState();
        }
        protected virtual bool GetVisibleState()
        {
            return this.Tags.Any(_ => _.Filter);
        }

        /// <summary>このノードを展開して表示させるか</summary>
        public bool IsExpanded
        {
            get
            {
                return _IsExpanded;
            }
            set
            {
                if (_IsExpanded != value)
                {
                    _IsExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }
        private bool _IsExpanded;

        /// <summary></summary>
        public bool IsLastNode { get; set; }

        public bool IsOther { get; set; }

        internal TreeNodeItem Parent { get; set; }

        public override string ToString()
        {
            return this.Header?.ToString();
        }
    }
}
