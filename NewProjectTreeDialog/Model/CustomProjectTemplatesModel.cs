namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;

    /// <summary>
    /// 1ページ目
    /// </summary>
    partial class CustomProjectTemplatesModel : NPDViewChildModelBase, IDisposable
    {
        public CustomProjectTemplatesModel()
        {
            foreach (string grpkey in TagTypeKey.defaultTagOrder)
            {
                this.TagTypeOrder.Add(grpkey);
            }
        }

        public override string ViewModelTypeName { get; } = "ProjectCreationViewModel";

        public override bool Initialize(IOption option, System.Windows.Controls.ContentControl npdview)
        {
            base.Initialize(option, npdview);

            this.IsNormalList = option.TemplateListMode == TemplateListMode.Normal;
            //this.IsShowTagsLeft = option.IsShowTagsLeft;

            if (string.IsNullOrEmpty(option.TagTypeOrder))
            {
                option.TagTypeOrder = string.Join(",", TagTypeKey.defaultTagOrder);
            }
            {
                var parts = option.TagTypeOrder.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                SortedDictionary<int, string> dic = new SortedDictionary<int, string>();
                for (int i = 0; i < TagTypeKey.defaultTagOrder.Count; i++)
                {
                    string key = TagTypeKey.defaultTagOrder[i];
                    int index = Array.IndexOf(parts, key.ToLower());
                    if (index < 0)
                    {
                        index = i + 1000;
                    }
                    dic.Add(index, key);
                }
                this.TagTypeOrder.Clear();

                foreach (string grpkey in dic.Values)
                {
                    this.TagTypeOrder.Add(grpkey);
                }
            }
            if (!IsNpdviewTargetContent())
            {
                return false;
            }
            if (!this.GetTexts(npdview))
            {
                return false;
            }

            if (!BuildTemplateNodes(npdview.Content))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(option.DisabledTags))
            {
                foreach (string disable in option.DisabledTags.Split(','))
                {
                    string[] parts = disable.Split(':');
                    if (parts.Length >= 2)
                    {
                        foreach (Tag tag in this.Tags.Where(_ => string.Equals(_.Type, parts[0], StringComparison.OrdinalIgnoreCase) && string.Equals(_.Id, parts[1], StringComparison.OrdinalIgnoreCase)))
                        {
                            tag.Filter = false;
                        }
                    }
                }
            }

            this.BuildTemplateTree();
            this.ExpandFirstRecentTree();

            return true;
        }

        protected override bool GetTexts(System.Windows.Controls.ContentControl npdview)
        {
            //16.2 TextBlockがLabelに変更された
            const string RECENT_PROJECT_TEMPLATES_TITLE = "RecentProjectTemplatesTitle";
            TextBlock textBlock = ControlFinder.FindChildren<TextBlock>(npdview).FirstOrDefault(_ => _.Name == RECENT_PROJECT_TEMPLATES_TITLE);
            if (textBlock != null)
            {
                this.header_recent = textBlock.Text;
            }
            else
            {
                Label label = ControlFinder.FindChildren<Label>(npdview).FirstOrDefault(_ => _.Name == RECENT_PROJECT_TEMPLATES_TITLE);
                if (label != null)
                {
                    var labelContent = label.Content;
                    if (label.Content is AccessText accessText)
                    {
                        this.header_recent = accessText.Text;
                    }
                    else
                    {
                        this.header_recent = label.Content.ToString();
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private bool has_HeaderOther = false;
        private string header_other = "Other";
        private string header_recent = "Recent";
        private System.Collections.Specialized.INotifyCollectionChanged recentCollection;
        private System.ComponentModel.GroupDescription _GroupDescription;

        /// <summary></summary>
        public bool IsNormalList
        {
            get { return _IsNormalList; }
            set { if (_IsNormalList != value) { _IsNormalList = value; OnPropertyChanged(nameof(IsNormalList)); } }
        }
        private bool _IsNormalList;

        //public bool IsShowTagsLeft
        //{
        //    get { return _IsShowTagsLeft; }
        //    set { if (_IsShowTagsLeft != value) { _IsShowTagsLeft = value; OnPropertyChanged(nameof(IsShowTagsLeft)); } }
        //}
        //private bool _IsShowTagsLeft;

        #region Node

        private TreeNodeItem recentNode;
        private TreeNodeItem onlineNode;

        /// <summary>TreeViewに表示する内容</summary>
        public System.Collections.ObjectModel.ObservableCollection<TreeNodeItem> TreeNodes { get; }
        = new System.Collections.ObjectModel.ObservableCollection<TreeNodeItem>();

        /// <summary>テンプレートの一覧</summary>
        private List<TreeNodeTemplateItem> TemplateNodes { get; } = new List<TreeNodeTemplateItem>();

        /// <summary>元のViewModelからテンプレート一覧を作る</summary>
        /// <param name="projectCreationViewModel"></param>
        /// <returns></returns>
        private bool BuildTemplateNodes(object projectCreationViewModel)
        {
            this.TemplateNodes.Clear();

            this.Tags.Clear();

            Type t = projectCreationViewModel.GetType();

            //フィルタを解除
            string clearFiltersCommandName;
            if (GLOBAL.DTEVersion < new Version(16, 3))
            {
                clearFiltersCommandName = "ClearFiltersCommand";
            }
            else
            {
                clearFiltersCommandName = "ClearCommand";
            }
            var clearFiltersCommand = t.GetProperty(clearFiltersCommandName)?.GetValue(projectCreationViewModel) as System.Windows.Input.ICommand;
            if (clearFiltersCommand != null)
            {
                if (clearFiltersCommand.CanExecute(null))
                {
                    clearFiltersCommand.Execute(null);
                }
            }

            //テンプレートの一覧を取得
            var extensions = t
                            .GetProperty("Extensions")?
                            .GetValue(projectCreationViewModel) as System.Collections.IEnumerable;
            if (extensions == null)
            {
                //16.3以降はExtensionsView
                extensions = (t
                    .GetProperty("ExtensionsView")?
                    .GetValue(projectCreationViewModel) as CollectionView)?.SourceCollection;

                if (extensions != null)
                {
                    var t0 = extensions.OfType<object>().FirstOrDefault()?.GetType();
                    //if (t0?.FullName == "Microsoft.VisualStudio.NewProjectDialog.VsTemplateViewModel")
                    //{
                    //    var pi=t0.GetProperty("Template");
                    //    extensions = extensions.OfType<object>().Select(_ => pi.GetValue(_)).OfType<object>();
                    //}
                }
            }

            if (extensions == null)
            {
                return false;
            }


            //"その他"に対応する文字列を調べる
            IEnumerable<TreeNodeTemplateItem> converter(bool forTest)
                => extensions.Cast<object>().Select(o_ext => CreateTreeNodeItemFromExtension(o_ext, forTest)).OfType<TreeNodeTemplateItem>();

            var tagOther = converter(true).SelectMany(n => n.Tags.Where(_ => _.Id?.ToString() == "other")).FirstOrDefault();
            if (tagOther != null)
            {
                this.header_other = tagOther.Value ?? "Other";
            }
            this.Tags.Clear();

            int nodeOrderIndex = 0;



            foreach (TreeNodeTemplateItem node in converter(false))
            {
                node.OrderIndex = nodeOrderIndex++;
                this.TemplateNodes.Add(node);
            }

            //ユーザーテンプレートに言語タグが付いていないので他の言語と比較して設定
            TreeNodeTemplateItem[] noLanguageTemplates = this.TemplateNodes.Where(_ => _.LanguageTags.Count() == 1 && _.LanguageTags.First().IsOther).ToArray();
            Tag[] languageTags = this.Tags.Where(_ => _.Type == TagTypeKey.Language && !_.IsOther).ToArray();
            foreach (Tag languageTag in languageTags)
            {
                var languages = this.TemplateNodes.Where(_ => _.Tags.Contains(languageTag)).Select(_ => new TemplateWrapper(_).ProjectType).Distinct().ToArray();
                if (languages.Length == 1)
                {
                    string language = languages[0];
                    foreach (TreeNodeTemplateItem nolanguage in noLanguageTemplates.Where(_ => _.TemplateWrapper.ProjectType == language))
                    {

                        int index = nolanguage.RemoveTag(nolanguage.LanguageTags.First());

                        nolanguage.insertTag(index, languageTag);
                    }
                }
            }
            foreach (TreeNodeTemplateItem userTemplate in this.TemplateNodes.Where(_ => _.TemplateWrapper.IsUserTemplate))
            {
                userTemplate.OrderIndex -= 10000;
            }


            //最近使ったテンプレートの一覧用
            this.recentNode = new TreeNodeItem() { Header = header_recent, IsExpanded = true, IsVisible = true };
            var mruvm = projectCreationViewModel.GetType().GetProperty("MruNewProjectsListViewModel")?.GetValue(projectCreationViewModel);
            if (mruvm != null)
            {
                object recentTemplates = mruvm?.GetType().GetProperty("RecentTemplates")?.GetValue(mruvm);
                if (recentTemplates == null)
                {//16.3?
                    recentTemplates = mruvm?.GetType().GetProperty("RecentTemplatesCollection")?.GetValue(mruvm);
                }

                var recents= recentTemplates as System.Collections.IEnumerable;
                if (recentTemplates is System.ComponentModel.ICollectionView icv)
                {//16.3?
                    recents= icv.SourceCollection as System.Collections.IEnumerable;
                }
                if (recentTemplates == null)
                {//16.3?
                    recentTemplates = mruvm?.GetType().GetProperty("RecentTemplatesCollectino")?.GetValue(mruvm);
                }
                UpdateRecentNode(recents);

                this.recentCollection = recents as System.Collections.Specialized.INotifyCollectionChanged;
                if (this.recentCollection != null)
                {
                    this.recentCollection.CollectionChanged += recentCollection_CollectionChanged;
                }
            }

            //2017にはあるオンラインテンプレートの検索はマーケットプレイスのページを表示させる
            var link = new Hyperlink(new Run("Online")) { NavigateUri = new Uri("https://marketplace.visualstudio.com/search?sortBy=Downloads&category=Templates&target=VS&vsVersion=vs2019") };
            System.Windows.Controls.TextBlock onlineHeader = new System.Windows.Controls.TextBlock();
            onlineHeader.Inlines.Add(link);
            onlineHeader.ToolTip = "Open Marketplace";
            link.RequestNavigate += (s, e) =>
            {
                if (e.Uri != null)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
                    link.NavigateUri = null;
                    Window.GetWindow(onlineHeader)?.Close();
                }
            };

            this.onlineNode = new TreeNodeItem() { Header = onlineHeader, IsExpanded = true, IsVisible = true };

            foreach (Tag tag in Tags.Where(_ => _.IsOther).ToArray())
            {
                this.Tags.Remove(tag);
                this.Tags.Add(tag);
            }
            return true;
        }

        /// <summary>Tagを使ってテンプレートをツリー状に分類する</summary>
        /// <param name="projectCreationViewModel"></param>
        private void BuildTemplateTree()
        {
            this.TreeNodes.Clear();

            SetChildNodes(null, this.TreeNodes, this.TemplateNodes, this.TagTypeOrder, 0);

            this.TreeNodes.Insert(0, this.recentNode);
            this.TreeNodes.Add(onlineNode);
        }

        /// <summary>ツリーになるようにNodeの子を作る</summary>
        /// <param name="parent">親のNode</param>
        /// <param name="parentList">個を入れるリスト</param>
        /// <param name="ie">リストに入れるNode</param>
        /// <param name="tagTypeStrings">分類用のタグ一覧</param>
        /// <param name="tagTypeIndex">分類の順位</param>
        private void SetChildNodes(TreeNodeItem parent, IList<TreeNodeItem> parentList, IEnumerable<TreeNodeItem> ie, IList<string> tagTypeStrings, int tagTypeIndex)
        {
            if (parent != null)
            {
                parent.FilteredItems.AddRange(ie.OrderBy(_ => _.OrderIndex));
            }

            if (tagTypeStrings.Count() <= tagTypeIndex)
            {
                parent.IsLastNode = true;
                foreach (TreeNodeItem n in ie)
                {
                    n.Parent = parent;
                    parentList.Add(n);
                }
            }
            else
            {
                string tagTypeString = tagTypeStrings[tagTypeIndex];

                var xx = ie.Select(n => new { Node = n, Tags = n.Tags.Where(t => t.Type == tagTypeString).ToArray() });
                var tags = xx.SelectMany(_ => _.Tags).Distinct().ToArray();
                foreach (Tag tag in tags)
                {
                    TreeNodeItem child = new TreeNodeItem();
                    child.Header = tag.Value;
                    child.Parent = parent;
                    child.IsOther = tag.IsOther;
                    child.AddTag(tag);

                    var subItems = xx.Where(o => o.Tags.Any(t => t.Value == tag.Value)).Select(_ => _.Node);
                    SetChildNodes(child, child.Items, subItems, tagTypeStrings, tagTypeIndex + 1);

                    parentList.Add(child);
                }

                var others = parentList.Where(_ => _.IsOther).ToArray();
                int i = parentList.Count();
                foreach (TreeNodeItem node in others)
                {
                    parentList.Remove(node);
                    parentList.Add(node);
                }
            }
        }


        /// <summary>
        /// VSのテンプレートをTreeNodeItemに変換する
        /// </summary>
        /// <param name="o_ext">テンプレート</param>
        /// <param name="forTest">true:変換できるかテストだけをする</param>
        /// <returns></returns>
        private TreeNodeTemplateItem CreateTreeNodeItemFromExtension(object o_ext, bool forTest = false)
        {
            try
            {
                object source = o_ext;
                Type t = o_ext.GetType();
                if (t.FullName == "Microsoft.VisualStudio.NewProjectDialog.VsTemplateViewModel")
                {//16.3
                    o_ext = t.GetProperty("Template").GetValue(source);
                }

                object o_name = o_ext.GetType().GetProperty("Name")?.GetValue(o_ext);
                if (o_name == null || o_name?.GetType().Name == "InstallMorePlaceHolderTemplate")
                {
                    return null;
                }

                TreeNodeTemplateItem node = new TreeNodeTemplateItem(o_ext, source);

                foreach (Tag tagTemp in node.TemplateWrapper.GetTemplateTags())
                {
                    if (!forTest)
                    {
                        var tag = this.Tags.FirstOrDefault(_ => _.Id == tagTemp.Id && _.Type == tagTemp.Type && _.Value == tagTemp.Value);
                        if (tag == null)
                        {
                            tag = tagTemp;
                            this.Tags.Add(tag);
                        }
                        node.AddTag(tag);
                    }
                    else
                    {
                        node.AddTag(tagTemp);
                    }
                }

                if (!forTest)
                {
                    string[] grpkeys = this.TagTypeOrder.ToArray();
                    if (!node.Tags.Any(_ => _.Type == TagTypeKey.ProjectType))
                    {
                        var name = node.TemplateWrapper.NonLocalizedCategoryFullName;
                        Tag tag = this.Tags.FirstOrDefault(_ => _.Type == TagTypeKey.ProjectType && _.Id == name);
                        if (tag == null)
                        {
                            tag = new Tag();
                            tag.Id = name;
                            tag.Type = TagTypeKey.ProjectType;
                            tag.Value = name;
                            this.Tags.Add(tag);
                        }
                        node.AddTag(tag);
                    }
                    foreach (string grpKey in grpkeys)
                    {
                        if (!node.Tags.Any(_ => _.Type == grpKey))
                        {
                            var tag = GetOtherTag(grpKey, forTest);
                            node.AddTag(tag);
                        }
                    }


                }
                return node;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Tag

        /// <summary>
        /// "その他"になっているタグを探す
        /// </summary>
        /// <param name="tagType"></param>
        /// <param name="forTest"></param>
        /// <returns></returns>
        private Tag GetOtherTag(string tagType, bool forTest)
        {
            if (!has_HeaderOther)
            {
                var tagOther = this.Tags.FirstOrDefault(_ => _.Id == "other");
                if (tagOther != null)
                {
                    tagOther.IsOther = true;
                    this.header_other = tagOther.Value;
                    has_HeaderOther = true;
                }
            }
            var tag = this.Tags.FirstOrDefault(_ => _.Type == tagType && _.IsOther);
            if (tag == null)
            {
                tag = new Tag();
                tag.Id = tagType + "_other_" + Guid.NewGuid().ToString();
                tag.Type = tagType;
                tag.Value = this.header_other;

                tag.IsOther = true;
                if (!forTest)
                {
                    this.Tags.Add(tag);
                }
            }
            return tag;
        }

        /// <summary>ツリーの並び,及びフィルターを適用する順番</summary>
        public System.Collections.ObjectModel.ObservableCollection<string> TagTypeOrder { get; } = new System.Collections.ObjectModel.ObservableCollection<string>();

        /// <summary>使われている全てのタグ一覧</summary>
        public List<Tag> Tags { get; } = new List<Tag>();

        /// <summary>タグをグループ化したコレクション</summary>
        public System.ComponentModel.ICollectionView TagsView
        {
            get
            {
                if (_TagsView == null)
                {
                    _GroupDescription = new PropertyGroupDescription(nameof(Tag.Type));
                    _GroupDescription.CustomSort = new TagOrder(this.TagTypeOrder);

                    _TagsView = CollectionViewSource.GetDefaultView(Tags);
                    _TagsView.GroupDescriptions.Add(_GroupDescription);
                }
                return _TagsView;
            }
        }
        private System.ComponentModel.ICollectionView _TagsView;

        /// <summary>グループの並べ替えを実行するコマンド</summary>
        public DelegateCommand TagSortCommand
        {
            get
            {
                if (_TagSortCommand == null)
                {
                    _TagSortCommand = new DelegateCommand((o) =>
                    {
                        string key = (string)o;
                        int index = this.TagTypeOrder.IndexOf(key);
                        if (index > 0)
                        {
                            this.TagTypeOrder.Remove(key);
                            this.TagTypeOrder.Insert(index - 1, key);
                        }

                        BuildTemplateTree();
                        this.TagsView.Refresh();
                    });
                }
                return _TagSortCommand;
            }
        }
        private DelegateCommand _TagSortCommand;

        #endregion

        #region 最近使ったテンプレート

        /// <summary>最近使用したテンプレートの一覧を更新</summary>
        /// <param name="ie"></param>
        private void UpdateRecentNode(System.Collections.IEnumerable ie)
        {
            if (this.recentNode != null && ie != null)
            {
                this.recentNode.Items.Clear();

                foreach (object o in ie)
                {
                    var o_ext = o.GetType().GetProperty("Template")?.GetValue(o);
                    TreeNodeItem node = CreateTreeNodeItemFromExtension(o_ext);
                    if (node != null)
                    {
                        node.FilteredItems.Add(node);
                        this.recentNode.Items.Add(node);
                        this.recentNode.FilteredItems.Add(node);
                    }
                }

                ExpandFirstRecentTree();
            }
        }
        private void ExpandFirstRecentTree()
        {
            if (this.recentNode.Items.Count() > 0)
            {
                var top = this.recentNode.Items[0];
                var node = this.TemplateNodes.FirstOrDefault(_ => _.Id == top.Id)?.Parent;
                while (node != null)
                {
                    node.IsExpanded = true;
                    node = node.Parent;
                }

            }
        }

        private void recentCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            System.Collections.IEnumerable ie = sender as System.Collections.IEnumerable;
            UpdateRecentNode(ie);
        }

        #endregion

        #region SelectedTemplate

        public event EventHandler<EventArgs> SelectedTemplateChanged;

        /// <summary>選択されたテンプレートをもとのViewModlに設定する</summary>
        public void CopySelectedExtensionToToOriginal()
        {
            SelectedExtensionOriginal = this._SelectedExtension;
        }

        /// <summary>
        /// VisualStudio側で選択されているテンプレート
        /// </summary>
        public object SelectedExtensionOriginal
        {
            get
            {
                var piSelectedExtension = this.OriginalViewModel.GetType().GetProperty("SelectedExtension");
                return piSelectedExtension.GetValue(this.OriginalViewModel);
            }
            private set
            {
                var piSelectedExtension = this.OriginalViewModel.GetType().GetProperty("SelectedExtension");
                piSelectedExtension.SetValue(this.OriginalViewModel, value);
            }
        }

        /// <summary>選択されているテンプレート</summary>
        public object SelectedExtension
        {
            get { return _SelectedExtension; }
            set { _SelectedExtension = value; SelectedTemplateChanged?.Invoke(this, EventArgs.Empty); }
        }
        private object _SelectedExtension;

        ///// <summary>元のViewmodel</summary>
        ///// <remarks>これは変化しないっぽい</remarks>
        //private object _OriginalViewModel;

        #endregion

        #region Search

        /// <summary></summary>
        public string SearchText
        {
            get
            {
                return _SearchText;
            }
            set
            {
                if (_SearchText != value)
                {
                    _SearchText = value;
                    OnPropertyChanged(nameof(SearchText));

                    if (value == null)
                    {
                        value = "";
                    }
                    string[] parts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (TreeNodeTemplateItem item in this.TemplateNodes)
                    {
                        item.UpdateSearch(parts);
                    }
                }
            }
        }
        private string _SearchText = string.Empty;

        public void SetFocusSearchTextBox()
        {

        }

        #endregion

        public override void WriteToOption(IOption option)
        {
            base.WriteToOption(option);

            option.TagTypeOrder = string.Join(",", this.TagTypeOrder.ToArray());
            option.DisabledTags = string.Join(",", this.Tags.Where(_ => !_.Filter && !_.IsOther).Select(_ => _.Type + ":" + _.Id).ToArray());
            option.TemplateListMode = this.IsNormalList ? TemplateListMode.Normal : TemplateListMode.Small;
        }

        #region IDisposable

        protected override void Dispose(bool isnotFromFinalizer)
        {
            if (isnotFromFinalizer)
            {
                if (this.recentCollection != null)
                {
                    this.recentCollection.CollectionChanged -= this.recentCollection_CollectionChanged;
                }
                this.recentCollection = null;
            }

            base.Dispose(isnotFromFinalizer);
        }


        ~CustomProjectTemplatesModel()
        {
            this.Dispose(false);
        }

        #endregion

    }











}
