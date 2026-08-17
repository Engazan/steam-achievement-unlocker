using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace SteamAchievementUnlocker
{
    public enum StyledDialogButtons
    {
        Ok,
        YesNo,
    }

    public enum StyledDialogIcon
    {
        Information,
        Error,
        Warning,
        Question,
    }

    public partial class StyledDialog : Window
    {
        private StyledDialog(
            Window owner,
            string message,
            string title,
            StyledDialogButtons buttons,
            StyledDialogIcon icon)
        {
            this.InitializeComponent();
            this.Owner = owner;
            this.Title = title;
            this.TitleText.Text = title;
            this.MessageText.Text = message;
            this.CancelButton.Visibility = buttons == StyledDialogButtons.YesNo
                ? Visibility.Visible
                : Visibility.Collapsed;
            this.OkButton.Content = buttons == StyledDialogButtons.YesNo ? "Yes" : "OK";
            this.ApplyIcon(icon);
        }

        public static bool Show(
            Window owner,
            string message,
            string title,
            StyledDialogButtons buttons = StyledDialogButtons.Ok,
            StyledDialogIcon icon = StyledDialogIcon.Information)
        {
            StyledDialog dialog = new(owner, message, title, buttons, icon);
            return dialog.ShowDialog() == true;
        }

        private void ApplyIcon(StyledDialogIcon icon)
        {
            string glyph;
            Brush background;
            Brush foreground;
            switch (icon)
            {
                case StyledDialogIcon.Error:
                    glyph = "!";
                    background = new SolidColorBrush(Color.FromRgb(107, 43, 61));
                    foreground = new SolidColorBrush(Color.FromRgb(255, 174, 188));
                    break;
                case StyledDialogIcon.Warning:
                    glyph = "!";
                    background = new SolidColorBrush(Color.FromRgb(103, 78, 35));
                    foreground = new SolidColorBrush(Color.FromRgb(255, 220, 139));
                    break;
                case StyledDialogIcon.Question:
                    glyph = "?";
                    background = new SolidColorBrush(Color.FromRgb(49, 67, 112));
                    foreground = new SolidColorBrush(Color.FromRgb(187, 209, 255));
                    break;
                default:
                    glyph = "i";
                    background = new SolidColorBrush(Color.FromRgb(38, 63, 112));
                    foreground = new SolidColorBrush(Color.FromRgb(187, 209, 255));
                    break;
            }

            this.TitleIconText.Text = glyph;
            this.MessageIconText.Text = glyph;
            this.TitleIconBorder.Background = background;
            this.MessageIconBorder.Background = background;
            this.MessageIconText.Foreground = foreground;
        }

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
