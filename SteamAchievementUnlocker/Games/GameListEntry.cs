using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SteamAchievementUnlocker
{
    internal sealed class GameListEntry : INotifyPropertyChanged
    {
        private string _name;
        private ImageSource _logo;
        private string _achievementProgress;
        private bool _hasAchievementProgress;

        public uint AppId { get; }
        public string GameType { get; }

        public string Name
        {
            get => this._name;
            set
            {
                string name = value ?? "App " + this.AppId.ToString(CultureInfo.InvariantCulture);
                if (this._name == name)
                {
                    return;
                }
                this._name = name;
                this.OnPropertyChanged();
            }
        }

        public string ImageUrl { get; internal set; }

        public ImageSource Logo
        {
            get => this._logo;
            set
            {
                if (ReferenceEquals(this._logo, value))
                {
                    return;
                }
                this._logo = value;
                this.OnPropertyChanged();
            }
        }

        public string AchievementProgress
        {
            get => this._achievementProgress;
            private set
            {
                if (this._achievementProgress == value)
                {
                    return;
                }
                this._achievementProgress = value;
                this.OnPropertyChanged();
            }
        }

        public bool HasAchievementProgress
        {
            get => this._hasAchievementProgress;
            private set
            {
                if (this._hasAchievementProgress == value)
                {
                    return;
                }
                this._hasAchievementProgress = value;
                this.OnPropertyChanged();
            }
        }

        public bool AchievementProgressLoaded { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public GameListEntry(uint id, string type)
        {
            this.AppId = id;
            this.GameType = type;
            this.Name = null;
            this.ImageUrl = null;
        }

        public void SetAchievementProgress(int achieved, int total)
        {
            this.AchievementProgress = achieved.ToString(CultureInfo.InvariantCulture) + " / " +
                                       total.ToString(CultureInfo.InvariantCulture);
            this.HasAchievementProgress = total > 0;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
