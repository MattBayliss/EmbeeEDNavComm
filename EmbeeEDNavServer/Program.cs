using System;
using System.Windows.Forms;

namespace EmbeeEDNavServer
{
    static class Program
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        [STAThread]
        static void Main(string[] args)
        {
            NamedPipeServer server = null;
            //try
            //{
                var config = new Config();
                var controller = new Controller();
                server = new NamedPipeServer(controller, 8);
                server.StartAsync().Wait();
                var trayIcon = new SystemTray(config);
                Application.Run(trayIcon);
            //}
            //catch (AggregateException aex)
            //{
            //    Logger.Error("Critical error", aex);
            //}
            //catch (Exception ex)
            //{
            //    Logger.Error("Critical error", ex);
            //}
            //finally
            //{
                if (server != null)
                {
                    server.Stop();
                }
            //}
        }
    }
}
