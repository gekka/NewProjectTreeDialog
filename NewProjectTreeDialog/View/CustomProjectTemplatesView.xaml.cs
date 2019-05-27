namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.View
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// CustomProjectTemplatesView.xaml の相互作用ロジック
    /// </summary>
    partial class CustomProjectTemplatesView : UserControl
    {
        public CustomProjectTemplatesView()
        {
            InitializeComponent();
        }

        private void Trv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item=e.NewValue as Model.TreeNodeItem;
            if (item != null)
            {
                if (lst.SelectedIndex == -1 && lst.Items.Count > 0)
                {
                    lst.SelectedIndex = 0;
                }
            }
        }
    }
}
