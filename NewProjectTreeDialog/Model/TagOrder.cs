namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.Model
{
    /// <summary>タグのグループを並べ替えるためのIComparer</summary>
    class TagOrder : System.Collections.IComparer
    {
        public TagOrder(System.Collections.ObjectModel.ObservableCollection<string> order)
        {
            this.order = order;
        }
        private System.Collections.ObjectModel.ObservableCollection<string> order;
        public int Compare(object x, object y)
        {
            string a = ((System.Windows.Data.CollectionViewGroup)x).Name.ToString();
            string b = ((System.Windows.Data.CollectionViewGroup)y).Name.ToString();
            return order.IndexOf(a).CompareTo(order.IndexOf(b));
        }
    }
}
