namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// <see cref="https://docs.microsoft.com/en-us/visualstudio/ide/template-tags?view=vs-2019"/>
    /// </remarks>
    class TagTypeKey
    {
        public const string Language = "Language";
        public const string Platform = "Platform";
        public const string ProjectType = "ProjectType";

        public static IReadOnlyList<string> defaultTagOrder
            = System.Array.AsReadOnly(new[] { TagTypeKey.Language, TagTypeKey.Platform, TagTypeKey.ProjectType });
    }
}
