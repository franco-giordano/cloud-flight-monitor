using System;
using System.Windows;
using Forms = System.Windows.Forms;
using CloudFlightMonitor.model;
using System.Timers;

namespace CloudFlightMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Forms.NotifyIcon notifyIcon;
        private System.Drawing.Icon[] chargeIcons;
        private CloudFlightHeadset headset = null;
        private Timer updateTimer;
        
        private const int UPDATE_TIMER_MS = 5000;   // 5 mins = 300000 ms
        private const string BASE_ICON_TEXT = "Cloud Flight Monitor";

        private Forms.ToolStripLabel batLabel;

        public App()
        {
            notifyIcon = new Forms.NotifyIcon();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            this.InitIcons();
            this.InitHIDDev();
            this.OnUpdate(null, null); // force update first time

            base.OnStartup(e);
            Console.WriteLine("Started!");

            // Start update Timer
            this.updateTimer = new Timer(UPDATE_TIMER_MS);
            this.updateTimer.Elapsed += this.OnUpdate;
            this.updateTimer.AutoReset = true;
            this.updateTimer.Enabled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose();
            base.OnExit(e);
        }

        private void OnUpdate(Object source, ElapsedEventArgs e)
        {
            //Get current batteryCharge
            int batteryCharge = this.headset.ReadBattery();

            string text = "Battery: " + batteryCharge + "%";
            batLabel.Text = text;
            notifyIcon.Text = BASE_ICON_TEXT + "\n" + text;

            //Update Tray Icon
            this.notifyIcon.Icon = this.chargeIcons[batteryCharge];
        }

        private void InitIcons()
        {
            this.chargeIcons = new System.Drawing.Icon[101];
            for (int i = 0; i <= 100; i++)
            {
                try
                {
                    this.chargeIcons[i] = new System.Drawing.Icon("icos\\" + i + ".ico");
                }
                catch (Exception)
                {
                    /*exit = true;*/
                    Console.WriteLine("At least one Icon was not found (" + i + ".ico). Process exiting.");
                    return;
                }
            }

            this.notifyIcon.Text = BASE_ICON_TEXT;

            Forms.ContextMenuStrip trayMenu = new Forms.ContextMenuStrip();

            batLabel = new Forms.ToolStripLabel("Battery Level: Unknown");
            trayMenu.Items.Add(batLabel);

            Forms.ToolStripMenuItem exitItem = new Forms.ToolStripMenuItem();
            exitItem.Text = "Exit";
            exitItem.Click += new System.EventHandler(ExitProgram);
            trayMenu.Items.Add(exitItem);

            this.notifyIcon.ContextMenuStrip = trayMenu;

            this.notifyIcon.Icon = this.chargeIcons[0];

            this.notifyIcon.Visible = true;
        }


        private void InitHIDDev()
        {
            try
            {
                this.headset = new CloudFlightHeadset();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cloud Flight Device not found! Msg: " + e.Message);
                /*exit = true;*/
            }
        }
        private void ExitProgram(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
