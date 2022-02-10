namespace CameraActivityChecker.Notifications
{
    using System.Drawing;

    public class NotificationInfo
    {
        public NotificationInfo(string title, string roInfo, string updated, Image icon, Color color, int duration)
        {
            Title = title;
            RoInfo = roInfo;
            Updated = updated;
            Icon = icon;
            Color = color;
            Duration = duration;
        }

        public NotificationInfo()
        {
        }

        public string Title { get; set; }

        public string RoInfo { get; set; }

        public string Updated { get; set; }

        public Image Icon { get; set; }

        public Color Color { get; set; }

        public int Duration { get; set; }
    }
}