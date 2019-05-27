using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.Model
{
    class TemplateWrapper
    {
        public TemplateWrapper(TreeNodeTemplateItem item)
        {
            this.TreeNodeTemplateItem = item;
            this.template = item.Template;
        }

        public TreeNodeTemplateItem TreeNodeTemplateItem { get; private set; }

        private object template;

        public string Id => GetPropertyText("Id");
        public string Name => GetPropertyText("Name");
        public string FullPath => GetPropertyText("FullPath");
        public string ProjectType => GetPropertyText("ProjectType");
        public string Description => GetPropertyText("Description");
        public string NonLocalizedCategoryFullName => GetPropertyText("NonLocalizedCategoryFullName");

        internal IEnumerable<Tag> GetTemplateTags()
        {

            var ietags = template.GetType().GetProperty("TemplateTags")?.GetValue(template) as System.Collections.IEnumerable;
            if (ietags == null)
            {
                return new Tag[0];
            }

            List<Tag> tags = new List<Tag>();
            foreach (object o_tag in ietags)
            {
                dynamic d_tag = o_tag;
                Tag tagTemp = new Tag();
                tagTemp.Id = d_tag.Id?.ToString() ?? "";
                tagTemp.Type = d_tag.Type?.ToString();
                tagTemp.Value = d_tag.Value?.ToString();
                tags.Add(tagTemp);
            }
            return tags;

        }

        private string GetPropertyText(string propertyName)
        {
            return template.GetType().GetProperty(propertyName).GetValue(template)?.ToString() ?? string.Empty;
        }

        public bool IsUserTemplate
        {
            get
            {
                if (!_IsUserTemplate.HasValue)
                {
                    string appdata = System.Environment.GetEnvironmentVariable("APPDATA");
                    _IsUserTemplate = this.FullPath?.StartsWith(appdata) == true;
                }

                return _IsUserTemplate.Value;
            }
        }
        public bool? _IsUserTemplate;

    }
}
