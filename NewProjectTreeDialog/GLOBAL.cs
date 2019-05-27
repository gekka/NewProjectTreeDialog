namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class GLOBAL
    {

        public static void Initialize(System.IServiceProvider sp, System.Windows.Threading.Dispatcher disp = null)
        {
            if (DTE == null)
            {
                DTE = sp.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
                if (disp == null)
                {
                    disp = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                }
                Dispatcher = disp;
            }
        }
        public static System.Windows.Threading.Dispatcher Dispatcher { get; private set; }

        public static EnvDTE80.DTE2 DTE { get; private set; }

        public static EnvDTE.CommandEvents CommandEvents
        {
            get
            {
                Dispatcher.VerifyAccess();
                if (_CommandEvents == null && DTE != null)
                {
                    _CommandEvents = DTE.Events.CommandEvents;
                }
                return _CommandEvents;
            }
        }
        private static EnvDTE.CommandEvents _CommandEvents;


        public static System.IServiceProvider ServiceProvider { get; set; }

    }
}
