using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SteamAchievementUnlocker.Stats
{
    internal sealed class AchievementInfo : INotifyPropertyChanged
    {
        private bool _isAchieved;
        private bool _isIconLoading;
        private ImageSource _icon;
        private float? _globalUnlockPercentage;

        public string Id { get; set; }
        public bool OriginalIsAchieved { get; set; }
        public DateTime? UnlockTime { get; set; }
        public int Permission { get; set; }
        public string IconNormal { get; set; }
        public string IconLocked { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsAchieved
        {
            get => this._isAchieved;
            set
            {
                if (this._isAchieved == value)
                {
                    return;
                }
                this._isAchieved = value;
                this.OnPropertyChanged();
            }
        }

        public ImageSource Icon
        {
            get => this._icon;
            set
            {
                if (ReferenceEquals(this._icon, value))
                {
                    return;
                }
                this._icon = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsIconLoading
        {
            get => this._isIconLoading;
            set
            {
                if (this._isIconLoading == value)
                {
                    return;
                }

                this._isIconLoading = value;
                this.OnPropertyChanged();
            }
        }

        public float? GlobalUnlockPercentage
        {
            get => this._globalUnlockPercentage;
            set
            {
                if (this._globalUnlockPercentage == value)
                {
                    return;
                }

                this._globalUnlockPercentage = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.GlobalUnlockPercentageText));
            }
        }

        public bool IsProtected => (this.Permission & 3) != 0;
        public string UnlockTimeText => this.UnlockTime?.ToString() ?? string.Empty;
        public string GlobalUnlockPercentageText => this.GlobalUnlockPercentage.HasValue
            ? this.GlobalUnlockPercentage.Value.ToString("0.##", CultureInfo.CurrentCulture) + " %"
            : "—";

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
