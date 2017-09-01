using System;
using System.Windows;
using CameraActivityChecker.Notifications;
using Emgu.CV;
using Languages.Implementation;
using Languages.Interfaces;

namespace CameraActivityChecker
{
    public partial class MainWindow : Window
    {
        private Capture _capture;
        private ILanguageManager _languageManager;
        private ILanguage _language;

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
            // ReSharper disable once FunctionNeverReturns
        }

        private void Initialize()
        {
            Visibility = Visibility.Collapsed;
            _languageManager = new LanguageManager();
            _languageManager.SetCurrentLanguage("de-DE");
            _language = _languageManager.GetCurrentLanguage();
        }

        private void CheckCameraIsActive()
        {
            try
            {
                InitCamera();
                var message = _language.GetWord("CameraActivated");
                var toastNotification = new Notification(message, message, 1000, FormAnimator.AnimationMethod.Center, FormAnimator.AnimationDirection.Down);
                toastNotification.Show();
            }
            catch
            {
                Console.WriteLine("Error");
            }
        }

        private void InitCamera()
        {
            _capture = new Capture();
            _capture.QueryFrame();
        }
    }
}