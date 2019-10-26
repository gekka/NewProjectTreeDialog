namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.View
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
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
            var item = e.NewValue as Model.TreeNodeItem;
            if (item != null)
            {
                if (lst.SelectedIndex == -1 && lst.Items.Count > 0)
                {
                    lst.SelectedIndex = 0;
                }
            }
        }

        private void lst_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            DependencyObject d = sender as DependencyObject;

            var sv = ControlFinder.FindParent<ScrollViewer>(d);
            if (sv != null)
            {
                var scp = sv.Template.FindName("PART_ScrollContentPresenter", sv) as ScrollContentPresenter;
                var isi = scp as System.Windows.Controls.Primitives.IScrollInfo;
                if (isi.CanHorizontallyScroll)
                {
                    double sign = 1;
                    if (e.Delta > 0)
                    {
                        sign = -1;
                    }
                    isi.SetHorizontalOffset(isi.HorizontalOffset + isi.ViewportWidth / 2 * sign);
                }

            }

        }

        private void SearchTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var txb = (TextBox)sender;
            var wnd = Window.GetWindow(this);
            if (wnd != null)
            {
                wnd.CommandBindings.Add(new System.Windows.Input.CommandBinding(System.Windows.Input.ApplicationCommands.Find, (sx, ex) =>
               {
                   txb.Focus();
                   txb.SelectAll();
               }));
            }
        }

        private void GroupAllCheck_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            string tag = btn.Tag.ToString();
            bool check = (tag == "true");

            var group = ControlFinder.FindParent<GroupItem>(btn);
            if (group == null)
            {
                return;
            }
            var info = (System.Windows.Controls.Primitives.IHierarchicalVirtualizationAndScrollInfo)group;
            Panel panel = info.ItemsHost;
            if (panel == null)
            {
                return;
            }
            foreach (var toggle in ControlFinder.FindChildren<System.Windows.Controls.Primitives.ToggleButton>(panel))
            {
                toggle.IsChecked = check;
            }
        }

        private void TreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)sender;
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(tvi);
            if (count == 1)
            {
                var grid = VisualTreeHelper.GetChild(tvi, 0) as Grid;
                if (grid != null && grid.Background == null)
                {
                    grid.Background = Brushes.Transparent;
                }
            }

            tvi.IsVisibleChanged += Tvi_IsVisibleChanged;
        }

        private void Tvi_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var node = (TreeViewItem)sender;
            if (!node.IsVisible
                && node.IsSelected
                && this.trv.SelectedItem != null
                && object.Equals(this.trv.SelectedItem, node.DataContext))
            {
                TreeViewItem parentNode = node;
                while (null != (parentNode = ControlFinder.FindParent<TreeViewItem>(parentNode)))
                {
                    if (parentNode.IsVisible)
                    {
                        parentNode.IsSelected = true;
                        return;
                    }
                }
                node.IsSelected = false;
            }
        }

    }
}
