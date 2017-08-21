using System;
using System.Windows.Forms;
using System.Management;

namespace CameraActivityChecker
{
    public partial class Main : Form
    {
        private readonly NotifyIcon _trayIcon = new NotifyIcon();

        public Main()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            InitializeTrayIcon();
            //USB\VID_04F2&PID_B354&MI_00\7&47877a&0&0000
            var connected = IsUsbDeviceConnected("PID_B354", "VID_04F2");
        }


        private bool IsUsbDeviceConnected(string pid, string vid)
        {
            using (var searcher =
              new ManagementObjectSearcher(@"Select * From Win32_USBControllerDevice"))
            {
                using (var collection = searcher.Get())
                {
                    foreach (var device in collection)
                    {
                       
                        var usbDevice = Convert.ToString(device);

                        if (usbDevice.Contains(pid) && usbDevice.Contains(vid))
                            return true;
                    }
                }
            }
            return false;
        }


    private void InitializeTrayIcon()
        {
            _trayIcon.Text = @"";
            _trayIcon.Visible = true;
           // _trayIcon.Icon = Resources.ConnectedIcon;
        }


        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
        }

        private void SetIconSuccess()
        {
            //_trayIcon.Icon = Resources.ConnectedIcon;
            //_trayIcon.BalloonTipTitle = Resources.InternetConnectionReestablished;
            //_trayIcon.BalloonTipText = Resources.InternetConnectionReestablished;
            //_trayIcon.Text = Resources.MainName + Environment.NewLine + Resources.Connected;
            //_trayIcon.ShowBalloonTip(1000);
        }

        private void SetIconFail()
        {
            //_trayIcon.Icon = Resources.NotConnectedIcon;
            //_trayIcon.BalloonTipTitle = Resources.InternetConnectionLost;
            //_trayIcon.BalloonTipText = Resources.InternetConnectionLost;
            //_trayIcon.Text = Resources.MainName + Environment.NewLine + Resources.NotConnected;
            //_trayIcon.ShowBalloonTip(1000);
        }
    }
}