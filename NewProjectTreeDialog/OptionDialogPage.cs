namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Shell;

    class OptionDialogPage : Microsoft.VisualStudio.Shell.DialogPage, IOption
    {
        public OptionDialogPage()
        {
        }

        [System.ComponentModel.Category("Tag")]
        [System.ComponentModel.DisplayName("TreeLevelOrder")]
        [System.ComponentModel.Description("")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.ComponentModel.Browsable(false)]
        public string TagTypeOrder { get; set; }

        [System.ComponentModel.Category("Tag")]
        [System.ComponentModel.DisplayName("DisabledTags")]
        [System.ComponentModel.Description("")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.ComponentModel.Browsable(false)]
        public string DisabledTags { get; set; }

        [System.ComponentModel.Category("Template")]
        [System.ComponentModel.DisplayName("ListMode")]
        [System.ComponentModel.Description("")]
        [System.ComponentModel.DefaultValue(TemplateListMode.Normal)]
        public TemplateListMode TemplateListMode { get; set; } = TemplateListMode.Normal;

        [System.ComponentModel.Category("Tree")]
        [System.ComponentModel.DisplayName("Expand TreeNode")]
        [System.ComponentModel.Description("")]
        [System.ComponentModel.DefaultValue(false)]
        public bool ExpandMode { get; set; } = false;


        //[System.ComponentModel.Category("Tag")]
        //[System.ComponentModel.DisplayName("TagsShowLeft")]
        //[System.ComponentModel.Description("")]
        //[System.ComponentModel.DefaultValue(true)]
        //public bool IsShowTagsLeft { get; set; } = true;
    }

    interface IOption
    {
        string TagTypeOrder { get; set; }
        string DisabledTags { get; set; }
        TemplateListMode TemplateListMode { get; set; }
        bool ExpandMode { get; set; } 
        //bool IsShowTagsLeft { get; set; }
        void SaveSettingsToStorage();
    }

    public enum TemplateListMode
    {
        Normal,
        Small
    }

}
