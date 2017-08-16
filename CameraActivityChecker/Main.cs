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
        }

        private void Query()
        {
            var win32DeviceClassName = "win32_processor";
            var query = $"select * from {win32DeviceClassName}";

            using (var searcher = new ManagementObjectSearcher(query))
            {
                var objectCollection = searcher.Get();

                foreach (var managementBaseObject in objectCollection)
                {
                    foreach (var propertyData in managementBaseObject.Properties)
                    {
                        Console.WriteLine(@"Property:  {0}, Value: {1}", propertyData.Name, propertyData.Value);
                    }
                }
            }
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