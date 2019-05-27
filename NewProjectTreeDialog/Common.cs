using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog
{
    class Common
    {
        public static void ShowError(string message)
        {
            string appname = System.Reflection.Assembly.GetExecutingAssembly().FullName.Split(',').FirstOrDefault();
            Microsoft.VisualStudio.Shell.VsShellUtilities.ShowMessageBox(GLOBAL.ServiceProvider as System.IServiceProvider, message, appname, Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING, Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK, Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

        }
    }

    static class StringExtenstion
    {
        public static bool ContainsIgnoreCase(this string text1, string text2)
        {
            return text1.IndexOf(text2, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
