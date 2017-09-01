using System;
using System.Windows;
using Emgu.CV;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace CameraActivityChecker
{
    public partial class MainWindow : Window
    {
        private Capture _capture;
        private Notifier _notifier;

        public MainWindow()
        {
            InitializeComponent();
            TryInitialize();
            CheckCameraActivated();
        }

        private void TryInitialize()
        {
            try
            {
                Initialize();
            }
            catch
            {
                Console.WriteLine("");
            }
        }

        private void CheckCameraActivated()
        {
            while (true)
                CheckCameraIsActive();
        }

        private void Initialize()
        {
            Visibility = Visibility.Collapsed;
            InitToastMessages();
        }

        private void InitToastMessages()
        {
            _notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(Application.Current.MainWindow, Corner.TopRight, 10, 10);
                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromSeconds(3), MaximumNotificationCount.FromCount(5));
                cfg.Dispatcher = Application.Current.Dispatcher;
            });
        }

        private void CheckCameraIsActive()
        {
            try
            {
                InitCamera();
                _notifier.ShowInformation("");
            }
            catch
            {
                Console.WriteLine("");
            }
        }

        private void InitCamera()
        {
            _capture = new Capture();
            _capture.QueryFrame();
        }
    }
}