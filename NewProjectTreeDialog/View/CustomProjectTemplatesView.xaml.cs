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
    }
}
