namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class GLOBAL
    {

        public static void Initialize(System.Windows.Threading.Dispatcher disp = null)
        {
            //if (ServiceProvider == null)
            //{
            //    ServiceProvider = sp;
            //}
            try
            {
                if (DTE == null)
                {
                    DTE = ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;

                    var v = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileVersionInfo;
                    DTEVersion = new Version(v.ProductVersion.Split(' ').First());

                    if (disp == null)
                    {
                        //disp = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                        disp = System.Windows.Application.Current.Dispatcher;
                    }
                    Dispatcher = disp;
                }
                
            }
            catch
            {
            }
        }
        public static System.Windows.Threading.Dispatcher Dispatcher { get; private set; }

        public static EnvDTE80.DTE2 DTE { get; private set; }
        public static Version DTEVersion { get; private set; }

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


        public static System.IServiceProvider ServiceProvider
        {
            get
            {
                if (_ServiceProvider != null)
                {
                    return _ServiceProvider;
                }
                if (GLOBAL.DTE == null)
                {
                    GLOBAL.DTE = (EnvDTE80.DTE2)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE));
                }

                _ServiceProvider = new Microsoft.VisualStudio.Shell.ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)GLOBAL.DTE);
                return _ServiceProvider;
            }
        }// private set; }
        private static System.IServiceProvider _ServiceProvider;

    }
}
