using System;
using System.Windows;
using Forms = System.Windows.Forms;
using CloudFlightMonitor.model;
using System.Timers;
using System.Drawing;


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

        private const int UPDATE_TIMER_MS = 60000;   // 5 mins = 300000 ms
        private const string BASE_ICON_TEXT = "Cloud Flight Monitor";

        private Forms.ToolStripLabel batLabel;

        public App()
        {
            notifyIcon = new Forms.NotifyIcon();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            InitIcons();
            InitHIDDev();
            OnUpdate(null, null); // force update first time

            base.OnStartup(e);
            Console.WriteLine("Started!");

            // Start update Timer
            updateTimer = new Timer(UPDATE_TIMER_MS);
            updateTimer.Elapsed += OnUpdate;
            updateTimer.AutoReset = true;
            updateTimer.Enabled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose();
            base.OnExit(e);
        }

        private void OnUpdate(Object source, ElapsedEventArgs _)
        {
            try
            {
                //Get current batteryCharge
                int batteryCharge = headset.ReadBattery();

                if (batteryCharge <= 100)
                {
                    string text = "Battery: " + batteryCharge + "%";
                    batLabel.Text = text;
                    notifyIcon.Text = BASE_ICON_TEXT + "\n" + text;

                    //Update Tray Icon
                    notifyIcon.Icon = chargeIcons[batteryCharge];
                }
                else if (batteryCharge == 199)
                {
                    // headset is charging
                    string text = "Headset Charging";
                    batLabel.Text = text;
                    notifyIcon.Text = BASE_ICON_TEXT + "\n" + text;

                    //Update Tray Icon
                    notifyIcon.Icon = chargeIcons[100];
                }
                else
                {
                    // headset returned a strange/unknown value...
                    string text = "Unknown Status";
                    batLabel.Text = text;
                    notifyIcon.Text = BASE_ICON_TEXT + "\n" + text;

                    //Update Tray Icon
                    notifyIcon.Icon = chargeIcons[101];
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error while updating battery charge, message: ", e.Message);
                const string txt = "Couldn't connect to headset";
                batLabel.Text = txt;
                notifyIcon.Text = BASE_ICON_TEXT + "\n" + txt;

                //Update Tray Icon
                notifyIcon.Icon = chargeIcons[0];

                // Reset connection...
                InitHIDDev();
            }

        }

        private void InitIcons()
        {
            chargeIcons = new Icon[102];
            for (int i = 0; i <= 100; i++)
            {
                try
                {
                    chargeIcons[i] = LoadIcon("icos\\" + i + ".ico");
                }
                catch (Exception)
                {
                    Console.WriteLine("At least one Icon was not found (" + i + ".ico). Process exiting.");
                    ExitProgram();
                }
            }

            chargeIcons[101] = LoadIcon("icos\\Headset.ico");

            notifyIcon.Text = BASE_ICON_TEXT + "\n" + "Waiting for connection...";

            Forms.ContextMenuStrip trayMenu = new Forms.ContextMenuStrip();

            batLabel = new Forms.ToolStripLabel("Battery Level: Unknown");
            trayMenu.Items.Add(batLabel);

            Forms.ToolStripMenuItem exitItem = new Forms.ToolStripMenuItem();
            exitItem.Text = "Exit";
            exitItem.Click += new EventHandler(ExitProgram);
            trayMenu.Items.Add(exitItem);

            notifyIcon.ContextMenuStrip = trayMenu;

            notifyIcon.Icon = chargeIcons[0];

            notifyIcon.Visible = true;
        }

        private Icon LoadIcon(string path)
        {
            try
            {
                return new Icon(path);
            }
            catch (Exception)
            {
                Console.WriteLine("At least one Icon was not found ({0}). Process exiting.", path);
                ExitProgram();
                return null;    // unreachable
            }
        }

        private void InitHIDDev()
        {
            try
            {
                headset = new CloudFlightHeadset();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cloud Flight Device not found! Message: " + e.Message);
            }
        }
        private void ExitProgram(object sender = null, EventArgs e = null)
        {
            Application.Current.Shutdown();
        }
    }
}
