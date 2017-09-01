using System;
using System.Threading;
using System.Windows.Forms;
using CameraActivityChecker.Notifications;
using Emgu.CV;
using Languages.Implementation;
using Languages.Interfaces;

namespace CameraActivityChecker
{
    public partial class Main : Form
    {
        private bool _cameraActivated;
        private Capture _capture;
        private ILanguage _language;
        private ILanguageManager _languageManager;

        public Main()
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
            _languageManager = new LanguageManager();
            _languageManager.SetCurrentLanguage("de-DE");
            _language = _languageManager.GetCurrentLanguage();
        }

        private void CheckCameraIsActive()
        {
            try
            {
                var activated = IsCameraActivated();
                if (_cameraActivated == activated) return;
                _cameraActivated = activated;
                if (!activated) return;
                ShowNotification();
            }
            catch
            {
                // ReSharper disable once EmptyGeneralCatchClause
            }
        }

        private void ShowNotification()
        {
            var message = _language.GetWord("CameraActivated");
            var toastNotification = GetNotification(message);
            ShowNotificationDelayed(toastNotification);
        }

        private void ShowNotificationDelayed(Notification toastNotification)
        {
            toastNotification.Show();
            Thread.Sleep(2000);
            toastNotification.Hide();
        }

        private Notification GetNotification(string message)
        {
            return new Notification(message, message, 1000, FormAnimator.AnimationMethod.Center,
                FormAnimator.AnimationDirection.Down);
        }

        private bool IsCameraActivated()
        {
            _capture = new Capture();
            var frame = _capture.QueryFrame();
            return frame != null;
        }
    }
}