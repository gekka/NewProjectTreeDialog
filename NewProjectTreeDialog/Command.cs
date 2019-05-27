namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog
{
    using System;

    class DelegateCommand : System.Windows.Input.ICommand
    {
        public DelegateCommand(Action act, Func<bool> can = null)
        {
            this.act = (o) => act();

            if (can == null)
            {
                this.can = (o) => true;
            }
            else
            {
                this.can = (o) => can();
            }
        }
        public DelegateCommand(Action<object> act, Func<object, bool> can = null)
        {
            this.act = act;
            this.can = can ?? ((object o) => true);
        }

        private Action<object> act;
        private Func<object, bool> can;
        public bool CanExecute(object parameter)
        {
            return can(parameter);
        }

        public void Execute(object parameter)
        {
            act(parameter);
        }


        public event EventHandler CanExecuteChanged
        {
            add
            {
                System.Windows.Input.CommandManager.RequerySuggested += value;
            }
            remove
            {
                System.Windows.Input.CommandManager.RequerySuggested -= value;
            }
        }
    }
}
