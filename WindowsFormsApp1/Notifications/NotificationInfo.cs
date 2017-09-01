using System.Drawing;

namespace CameraActivityChecker.Notifications
{
    // ReSharper disable once UnusedMember.Global
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

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string Title { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string RoInfo { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string Updated { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public Image Icon { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public Color Color { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public int Duration { get; set; }
    }
}