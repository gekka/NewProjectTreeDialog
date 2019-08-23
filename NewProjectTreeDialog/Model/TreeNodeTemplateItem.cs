namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.Model
{
    using System;
    using System.Linq;
    class TreeNodeTemplateItem : TreeNodeItem
    {
        public TreeNodeTemplateItem(object template,object templateSource)
        {
            this.IsLastNode = true;

            this.Template = template;
            this.TemplateWrapper = new TemplateWrapper(this);

            this.Id = this.TemplateWrapper.Id;
            this.Name = this.TemplateWrapper.Name;
            this.Description = this.TemplateWrapper.Description;
            this.Header = this.Name;

            this.TemplateSource = templateSource ?? template;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }

        public object Template
        {
            get;
            internal set;
        }

        /// <summary>
        /// テンプレートを取り出した元のソース
        /// </summary>
        /// <remarks>
        /// 16.3からテンプレートがViewModelに包まれるようになった
        /// </remarks>
        public object TemplateSource { get; private set; }

        public TemplateWrapper TemplateWrapper { get; private set; }

        protected override bool GetVisibleState()
        {
            bool visible = base.GetVisibleState() & isFound;
            return visible;
        }

        private bool isFound = true;

        internal void UpdateSearch(string[] andTexts)
        {
            bool result = false;
            if (andTexts == null || andTexts.Length == 0)
            {
                result = true;
            }
            else
            {
                foreach (string text in andTexts)
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    if (this.Name.ContainsIgnoreCase(text) || this.Description.ContainsIgnoreCase(text) || this.Tags.Any(_ => _.Value.ContainsIgnoreCase(text)))
                    {
                        result = true;
                        break;
                    }
                }
            }

            this.isFound = result;
            UpdateIsVisible();
        }
    }
}
