using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmbeeEDNavServer
{
    class SystemTray : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        public SystemTray()
        {
            var versionText = string.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version);

            // Create a simple tray menu with only one item
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add(versionText).Enabled = false;
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "Embee Elite:Dangerous Navigation Helper";
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EmbeeEDNavServer.Resources.rocket.ico"))
            {
                var icon = new Icon(stream);

                Icon = icon;
                trayIcon.Icon = icon;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }

    }
}
