using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using SteamAchievementUnlocker.Application.Ports;
using static SteamAchievementUnlocker.InvariantFormatting;
using API = SteamIntegration;
using APITypes = SteamIntegration.Models;

namespace SteamAchievementUnlocker
{
    public partial class GameManagerWindow : Window
    {
        public event EventHandler ReturnToPickerRequested;

        private readonly long _gameId;
        private readonly API.SteamClientSession _steamClient;

        private readonly AchievementIconService _achievementIconService;
        private readonly IGameStatisticsSchemaSource _statsSchemaLoader;
        private readonly IStatisticsRequestGateway _statsRequestService;
        private readonly IStatisticsReader _statisticsReader;
        private readonly IAchievementStateProvider _achievementReader;
        private readonly IStatisticsStore _statsStoreService;
        private readonly List<Stats.StatDefinition> _statDefinitions = new();

        private readonly List<Stats.AchievementDefinition> _achievementDefinitions = new();

        private readonly ObservableCollection<Stats.AchievementInfo> _achievements = new();
        private readonly ObservableCollection<Stats.StatInfo> _statistics = new();
        private readonly DispatcherTimer _callbackTimer;
        private readonly Dictionary<GridViewColumnHeader, string> _achievementColumnLabels = new();
        private string _achievementSortProperty;
        private ListSortDirection? _achievementSortDirection;

        private readonly API.Callbacks.UserStatsReceivedCallback _userStatsReceivedCallback;

        public GameManagerWindow(long gameId, API.SteamClientSession client)
        {
            this.InitializeComponent();
            this._mainTabControl.SelectedIndex = 0;
            this._achievementListView.ItemsSource = this._achievements;
            this._statisticsDataGridView.ItemsSource = this._statistics;

            this._gameId = gameId;
            this._steamClient = client;

            this._achievementIconService = new AchievementIconService(this._gameId);
            this._achievementIconService.QueueChanged += this.OnIconQueueChanged;
            this._statsSchemaLoader = Composition.BackendComposition.CreateStatisticsSchemaSource(
                this._gameId,
                this._steamClient);
            this._statsRequestService = Composition.BackendComposition.CreateStatisticsRequestGateway(
                this._steamClient);
            this._statisticsReader = Composition.BackendComposition.CreateStatisticsReader(this._steamClient);
            this._achievementReader = Composition.BackendComposition.CreateAchievementStateProvider(
                this._steamClient);
            this._statsStoreService = Composition.BackendComposition.CreateStatisticsStore(this._steamClient);

            string name = this._steamClient.ApplicationData.GetAppData((uint)this._gameId, "name");
            if (name != null)
            {
                this.Title += " | " + name;
            }
            else
            {
                this.Title += " | " + this._gameId.ToString(CultureInfo.InvariantCulture);
            }
            this.TitleText.Text = this.Title;

            this._userStatsReceivedCallback = client.CreateAndRegisterCallback<API.Callbacks.UserStatsReceivedCallback>();
            this._userStatsReceivedCallback.OnRun += this.OnUserStatsReceived;

            this._callbackTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(100),
            };
            this._callbackTimer.Tick += this.OnTimer;

            this.Loaded += (_, _) =>
            {
                this._callbackTimer.Start();
                this.RefreshStats();
            };
            this.Closed += this.OnClosed;
        }

        private static string TranslateError(int id) => id switch
        {
            2 => "generic error -- this usually means you don't own the game",
            _ => Format($"{id}"),
        };

        private bool LoadGameStatsSchema()
        {
            if (this._statsSchemaLoader.TryLoad(
                out List<Stats.AchievementDefinition> achievementDefinitions,
                out List<Stats.StatDefinition> statDefinitions) == false)
            {
                return false;
            }

            this._achievementDefinitions.Clear();
            this._achievementDefinitions.AddRange(achievementDefinitions);
            this._statDefinitions.Clear();
            this._statDefinitions.AddRange(statDefinitions);
            return true;
        }

        private void OnUserStatsReceived(APITypes.UserStatsReceivedCallbackData param)
        {
            if (param.Result != 1)
            {
                this._gameStatusLabel.Text = $"Error while retrieving stats: {TranslateError(param.Result)}";
                this.EnableInput();
                return;
            }

            if (this.LoadGameStatsSchema() == false)
            {
                this._gameStatusLabel.Text = "Failed to load schema.";
                this.EnableInput();
                return;
            }

            try
            {
                this.LoadAchievements();
            }
            catch (Exception e)
            {
                this._gameStatusLabel.Text = "Error when handling achievements retrieval.";
                this.EnableInput();
                StyledDialog.Show(
                    this,
                    "Error when handling achievements retrieval:\n" + e,
                    "Error",
                    StyledDialogButtons.Ok,
                    StyledDialogIcon.Error);
                return;
            }

            try
            {
                this.LoadStatistics();
            }
            catch (Exception e)
            {
                this._gameStatusLabel.Text = "Error when handling stats retrieval.";
                this.EnableInput();
                StyledDialog.Show(
                    this,
                    "Error when handling stats retrieval:\n" + e,
                    "Error",
                    StyledDialogButtons.Ok,
                    StyledDialogIcon.Error);
                return;
            }

            this._gameStatusLabel.Text = $"{this._achievements.Count} achievements · {this._statistics.Count} statistics";
            this.EnableInput();
        }

        private void RefreshStats()
        {
            this._achievements.Clear();
            this._statistics.Clear();

            // This still triggers the UserStatsReceived callback, in addition to the callresult.
            // No need to implement callresults for the time being.
            if (this._statsRequestService.RequestUserStats() == false)
            {
                StyledDialog.Show(this, "Failed.", "Error", StyledDialogButtons.Ok, StyledDialogIcon.Error);
                return;
            }

            this._statsRequestService.RequestGlobalAchievementPercentages();

            this._gameStatusLabel.Text = "Retrieving stat information...";
            this.DisableInput();
        }

        private float? GetGlobalAchievementPercentage(string achievementId)
        {
            return this._statsRequestService.GetGlobalAchievementPercentage(achievementId);
        }

        private void UpdateGlobalAchievementPercentages()
        {
            foreach (Stats.AchievementInfo achievement in this._achievements)
            {
                achievement.GlobalUnlockPercentage =
                    this.GetGlobalAchievementPercentage(achievement.Id);
            }

            if (this._achievementSortProperty == nameof(Stats.AchievementInfo.GlobalUnlockPercentage))
            {
                CollectionViewSource.GetDefaultView(this._achievements).Refresh();
            }
        }

        private void CheckGlobalAchievementPercentagesRequest()
        {
            if (this._statsRequestService.TryProcessGlobalAchievementPercentages() == false)
            {
                return;
            }

            this.UpdateGlobalAchievementPercentages();
        }

        private void LoadAchievements()
        {
            string textSearch = this._matchingStringTextBox.Text.Length > 0
                ? this._matchingStringTextBox.Text
                : null;
            this._achievements.Clear();
            foreach (Stats.AchievementInfo achievement in this._achievementReader.Read(
                this._achievementDefinitions,
                textSearch,
                this._displayLockedOnlyButton.IsChecked == true,
                this._displayUnlockedOnlyButton.IsChecked == true,
                this.GetGlobalAchievementPercentage,
                this.AddAchievementToIconQueue))
            {
                this._achievements.Add(achievement);
            }

            this._achievementIconService.StartQueuedDownloads();
            this._achievementEmptyState.Visibility = this._achievements.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void LoadStatistics()
        {
            this._statistics.Clear();
            foreach (Stats.StatInfo statistic in this._statisticsReader.Read(this._statDefinitions))
            {
                this._statistics.Add(statistic);
            }
        }

        private void AddAchievementToIconQueue(Stats.AchievementInfo info, bool startDownload)
        {
            this._achievementIconService.Queue(info, startDownload);
        }

        private int StoreAchievements()
        {
            int count = this._statsStoreService.StoreAchievements(
                this._achievements,
                out string failedAchievementId);
            if (count < 0)
            {
                StyledDialog.Show(
                    this,
                    $"An error occurred while setting the state for {failedAchievementId}, aborting store.",
                    "Error",
                    StyledDialogButtons.Ok,
                    StyledDialogIcon.Error);
            }
            return count;
        }

        private int StoreStatistics()
        {
            int count = this._statsStoreService.StoreStatistics(
                this._statistics,
                out string failedStatisticId);
            if (count < 0)
            {
                StyledDialog.Show(
                    this,
                    $"An error occurred while setting the value for {failedStatisticId}, aborting store.",
                    "Error",
                    StyledDialogButtons.Ok,
                    StyledDialogIcon.Error);
            }
            return count;
        }

        private void DisableInput()
        {
            this._reloadButton.IsEnabled = false;
            this._storeButton.IsEnabled = false;
            this._loadingOverlay.Visibility = Visibility.Visible;
        }

        private void EnableInput()
        {
            this._reloadButton.IsEnabled = true;
            this._storeButton.IsEnabled = true;
            this._loadingOverlay.Visibility = Visibility.Collapsed;
        }

        private void OnTimer(object sender, EventArgs e)
        {
            this._callbackTimer.Stop();
            this._steamClient.RunCallbacks(false);
            this.CheckGlobalAchievementPercentagesRequest();
            this._callbackTimer.Start();
        }

        private void OnIconQueueChanged(int remaining)
        {
            this._downloadStatusLabel.Visibility = remaining > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
            this._downloadStatusLabel.Text = remaining > 0
                ? $"Downloading {remaining} icons..."
                : string.Empty;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            this._callbackTimer.Stop();
            this._callbackTimer.Tick -= this.OnTimer;
            this._userStatsReceivedCallback.OnRun -= this.OnUserStatsReceived;
            this._achievementIconService.QueueChanged -= this.OnIconQueueChanged;
            this._achievementIconService.Dispose();
            this.Closed -= this.OnClosed;
        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            this.RefreshStats();
        }

        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5 && this._reloadButton.IsEnabled)
            {
                e.Handled = true;
                this.RefreshStats();
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 &&
                e.Key == Key.S &&
                this._storeButton.IsEnabled)
            {
                e.Handled = true;
                this.OnStore(this._storeButton, new RoutedEventArgs());
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.F)
            {
                e.Handled = true;
                this._achievementsSegment.IsChecked = true;
                this._matchingStringTextBox.Focus();
                this._matchingStringTextBox.SelectAll();
            }
        }

        private void OnAchievementFilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
            {
                return;
            }

            this._matchingStringTextBox.Clear();
            e.Handled = true;
        }

        private void OnAchievementColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            if (sender is not GridViewColumnHeader header ||
                header.Tag is not string propertyName)
            {
                return;
            }

            if (string.Equals(this._achievementSortProperty, propertyName, StringComparison.Ordinal) == false)
            {
                this._achievementSortProperty = propertyName;
                this._achievementSortDirection = ListSortDirection.Ascending;
            }
            else if (this._achievementSortDirection == ListSortDirection.Ascending)
            {
                this._achievementSortDirection = ListSortDirection.Descending;
            }
            else
            {
                this._achievementSortProperty = null;
                this._achievementSortDirection = null;
            }

            this.ApplyAchievementSorting();
        }

        private void ApplyAchievementSorting()
        {
            var view = CollectionViewSource.GetDefaultView(this._achievements);
            using (view.DeferRefresh())
            {
                view.SortDescriptions.Clear();
                if (this._achievementSortProperty != null && this._achievementSortDirection.HasValue)
                {
                    view.SortDescriptions.Add(new SortDescription(
                        this._achievementSortProperty,
                        this._achievementSortDirection.Value));
                }
            }

            if (this._achievementListView.View is not GridView gridView)
            {
                return;
            }

            foreach (GridViewColumn column in gridView.Columns)
            {
                if (column.Header is not GridViewColumnHeader header || header.Tag is not string propertyName)
                {
                    continue;
                }

                if (this._achievementColumnLabels.TryGetValue(header, out string label) == false)
                {
                    label = header.Content?.ToString() ?? string.Empty;
                    this._achievementColumnLabels.Add(header, label);
                }

                string indicator = string.Equals(
                    this._achievementSortProperty,
                    propertyName,
                    StringComparison.Ordinal)
                    ? this._achievementSortDirection switch
                    {
                        ListSortDirection.Ascending => "  ↑",
                        ListSortDirection.Descending => "  ↓",
                        _ => string.Empty,
                    }
                    : string.Empty;
                header.Content = label + indicator;
            }
        }

        private void OnSelectAchievementsTab(object sender, RoutedEventArgs e)
        {
            this._mainTabControl.SelectedIndex = 0;
        }

        private void OnSelectStatisticsTab(object sender, RoutedEventArgs e)
        {
            this._mainTabControl.SelectedIndex = 1;
        }

        private void OnLockAll(object sender, RoutedEventArgs e)
        {
            foreach (var info in this._achievements.Where(info => info.IsProtected == false))
            {
                info.IsAchieved = false;
            }
        }

        private void OnInvertAll(object sender, RoutedEventArgs e)
        {
            foreach (var info in this._achievements.Where(info => info.IsProtected == false))
            {
                info.IsAchieved = !info.IsAchieved;
            }
        }

        private void OnUnlockAll(object sender, RoutedEventArgs e)
        {
            foreach (var info in this._achievements.Where(info => info.IsProtected == false))
            {
                info.IsAchieved = true;
            }
        }

        private bool Store()
        {
            if (this._steamClient.UserStatistics.StoreStats() == false)
            {
                StyledDialog.Show(
                    this,
                    "An error occurred while storing, aborting.",
                    "Error",
                    StyledDialogButtons.Ok,
                    StyledDialogIcon.Error);
                return false;
            }

            return true;
        }

        private void OnStore(object sender, RoutedEventArgs e)
        {
            int achievements = this.StoreAchievements();
            if (achievements < 0)
            {
                this.RefreshStats();
                return;
            }

            int stats = this.StoreStatistics();
            if (stats < 0)
            {
                this.RefreshStats();
                return;
            }

            if (this.Store() == false)
            {
                this.RefreshStats();
                return;
            }

            StyledDialog.Show(
                this,
                $"Stored {achievements} achievements and {stats} statistics.",
                "Information",
                StyledDialogButtons.Ok,
                StyledDialogIcon.Information);
            this.RefreshStats();
        }

        private void OnStatAgreementChecked(object sender, RoutedEventArgs e)
        {
            this._statisticsValueColumn.IsReadOnly = this._enableStatsEditingCheckBox.IsChecked != true;
        }

        private void OnResetAllStats(object sender, RoutedEventArgs e)
        {
            if (StyledDialog.Show(
                this,
                "Are you absolutely sure you want to reset stats?",
                "Warning",
                StyledDialogButtons.YesNo,
                StyledDialogIcon.Warning) == false)
            {
                return;
            }

            bool achievementsToo = StyledDialog.Show(
                this,
                "Do you want to reset achievements too?",
                "Question",
                StyledDialogButtons.YesNo,
                StyledDialogIcon.Question);

            if (StyledDialog.Show(
                this,
                "Really really sure?",
                "Warning",
                StyledDialogButtons.YesNo,
                StyledDialogIcon.Error) == false)
            {
                return;
            }

            if (this._steamClient.UserStatistics.ResetAllStats(achievementsToo) == false)
            {
                StyledDialog.Show(this, "Failed.", "Error", StyledDialogButtons.Ok, StyledDialogIcon.Error);
                return;
            }

            this.RefreshStats();
        }

        private void OnCheckAchievement(object sender, MouseButtonEventArgs e)
        {
            if (sender is not CheckBox checkBox ||
                checkBox.DataContext is not Stats.AchievementInfo info)
            {
                return;
            }

            if (info.IsProtected)
            {
                StyledDialog.Show(
                    this,
                    "Sorry, but this is a protected achievement and cannot be managed with Steam Achievement Unlocker.",
                    "Error",
                    StyledDialogButtons.Ok,
                    StyledDialogIcon.Error);
                e.Handled = true;
            }
        }

        private void OnDisplayUncheckedOnly(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton)?.IsChecked == true)
            {
                this._displayUnlockedOnlyButton.IsChecked = false;
            }

            this.LoadAchievements();
        }

        private void OnDisplayCheckedOnly(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton)?.IsChecked == true)
            {
                this._displayLockedOnlyButton.IsChecked = false;
            }

            this.LoadAchievements();
        }

        private void OnFilterUpdate(object sender, TextChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                this.LoadAchievements();
            }
        }

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                this.WindowState = this.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
                return;
            }

            this.DragMove();
        }

        private void OnMinimize(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private void OnMaximize(object sender, RoutedEventArgs e) =>
            this.WindowState = this.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

        private void OnBack(object sender, RoutedEventArgs e) =>
            this.ReturnToPickerRequested?.Invoke(this, EventArgs.Empty);

        private void OnClose(object sender, RoutedEventArgs e) => this.Close();
    }
}
