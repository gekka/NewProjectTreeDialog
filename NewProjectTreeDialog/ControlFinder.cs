namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog
{
    using System;
    using System.Linq;
    using System.Windows.Media;

    class ControlFinder
    {
        public static System.Collections.Generic.IEnumerable<T> FindChildren<T>(System.Windows.DependencyObject d) where T : System.Windows.DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(d);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);
                T t = (child as T);
                if (t != null)
                {
                    yield return t;
                }
                foreach (T dd in FindChildren<T>(child))
                {
                    yield return dd;
                }
            }
        }
    }
}


