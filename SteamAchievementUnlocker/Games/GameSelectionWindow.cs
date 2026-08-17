using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SteamAchievementUnlocker.Application.Ports;
using static SteamAchievementUnlocker.InvariantFormatting;
using API = SteamIntegration;
using APITypes = SteamIntegration.Models;

namespace SteamAchievementUnlocker
{
    public partial class GameSelectionWindow : Window
    {

        private readonly API.SteamClientSession _steamClient;
        private readonly IGameCatalog _gameCatalogService;
        private readonly IGameArtworkProvider _gameLogoService;
        private readonly GameManagerLauncher _gameManagerLauncher = new();
        private readonly GameListFilterService _gameListFilterService = new();
        private readonly Dictionary<uint, GameListEntry> _games = new();
        private readonly ObservableCollection<GameListEntry> _filteredGames = new();
        private readonly Dictionary<string, Task<BitmapImage>> _logoTasks = new();
        private readonly DispatcherTimer _callbackTimer;
        private readonly API.Callbacks.AppDataChangedCallback _appDataChangedCallback;
        private readonly AchievementProgressService _achievementProgressService;
        private bool _gameManagerRunning;
        private bool _launchingGame;
        private int _activeLogoDownloads;

        public GameSelectionWindow(API.SteamClientSession client)
        {
            this.InitializeComponent();
            this.DataContext = this._filteredGames;
            this._steamClient = client;
            this._gameCatalogService = Composition.BackendComposition.CreateGameCatalog(client);
            this._gameLogoService = Composition.BackendComposition.CreateGameArtworkProvider(client);
            ulong steamId = client.UserAccount.GetSteamId();
            uint accountId = (uint)steamId;
            string steamLibraryCachePath = Path.Combine(
                API.SteamInstallLocator.GetInstallPath(),
                "userdata",
                accountId.ToString(CultureInfo.InvariantCulture),
                "config",
                "librarycache");
            this._achievementProgressService = new AchievementProgressService(
                steamId,
                steamLibraryCachePath,
                client);

            this._appDataChangedCallback = client.CreateAndRegisterCallback<API.Callbacks.AppDataChangedCallback>();
            this._appDataChangedCallback.OnRun += this.OnAppDataChanged;

            this._callbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            this._callbackTimer.Tick += this.OnTimer;
            this._callbackTimer.Start();

            this.Loaded += async (_, _) => await this.AddGamesAsync(false);
            this.Closed += this.OnClosed;
        }

        private void OnAppDataChanged(APITypes.AppDataChangedCallbackData param)
        {
            if (param.Result && this._games.TryGetValue(param.Id, out var game))
            {
                game.Name = this._steamClient.ApplicationData.GetAppData(game.AppId, "name");
            }
        }

        private async Task AddGamesAsync(bool forceRefresh)
        {
            this.RefreshButton.IsEnabled = false;
            this.LoadingOverlay.Visibility = Visibility.Visible;
            this.LoadingStatusText.Text = forceRefresh ? "Refreshing game data..." : "Checking saved game data...";
            this._callbackTimer.Stop();

            try
            {
                Progress<uint> scanProgress = new(currentAppId =>
                {
                    string action = forceRefresh
                        ? "Refreshing game data..."
                        : "Checking saved game data...";
                    this.LoadingStatusText.Text =
                        $"{action} {currentAppId:N0}/{GameDiscoveryService.MaximumAppIdToScan:N0}";
                });
                var games = await Task.Run(() =>
                    this._gameCatalogService.LoadOwnedGames(forceRefresh, scanProgress));
                this._games.Clear();
                foreach (var game in games)
                {
                    this._games[game.AppId] = game;
                }
            }
            catch (Exception e)
            {
                this._games.Clear();
                StyledDialog.Show(this, e.Message, "Error", StyledDialogButtons.Ok, StyledDialogIcon.Error);
            }
            finally
            {
                this._callbackTimer.Start();
                this.RefreshGames();
                this.RefreshButton.IsEnabled = true;
                this.LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void RefreshGames()
        {
            string search = string.IsNullOrWhiteSpace(this.SearchTextBox.Text) ? null : this.SearchTextBox.Text;
            IReadOnlyList<GameListEntry> filteredGames = this._gameListFilterService.Filter(
                this._games.Values,
                search,
                this.ShowGamesMenuItem.IsChecked,
                this.ShowDemosMenuItem.IsChecked);

            this._filteredGames.Clear();
            foreach (GameListEntry game in filteredGames)
            {
                this._filteredGames.Add(game);
            }

            this.StatusText.Text = this._filteredGames.Count == this._games.Count
                ? $"{this._games.Count:N0} games"
                : $"{this._filteredGames.Count:N0} of {this._games.Count:N0} games";
            this.EmptyState.Visibility = this._filteredGames.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
            if (this._filteredGames.Count > 0)
            {
                this.GameList.SelectedIndex = 0;
            }
            else
            {
                this.GameList.SelectedIndex = -1;
            }
        }

        private async void OnGameCardLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is GameListEntry info)
            {
                await this.LoadLogoAsync(info);
                if (this.IsVisibleInGameList(element))
                {
                    await this.LoadAchievementProgressAsync(info);
                }
            }
        }

        private async void OnGameListScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            await Dispatcher.Yield(DispatcherPriority.Background);
            List<Task> tasks = new();
            foreach (GameListEntry info in this._filteredGames)
            {
                if (this.GameList.ItemContainerGenerator.ContainerFromItem(info) is FrameworkElement container &&
                    this.IsVisibleInGameList(container))
                {
                    tasks.Add(this.LoadAchievementProgressAsync(info));
                }
            }

            await Task.WhenAll(tasks);
        }

        private bool IsVisibleInGameList(FrameworkElement element)
        {
            if (element.IsVisible == false || this.GameList.ActualWidth <= 0 || this.GameList.ActualHeight <= 0)
            {
                return false;
            }

            try
            {
                Rect bounds = element.TransformToAncestor(this.GameList).TransformBounds(
                    new Rect(0, 0, element.ActualWidth, element.ActualHeight));
                return bounds.IntersectsWith(new Rect(0, 0, this.GameList.ActualWidth, this.GameList.ActualHeight));
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private async Task LoadAchievementProgressAsync(GameListEntry info)
        {
            if (info.AchievementProgressLoaded)
            {
                return;
            }

            info.AchievementProgressLoaded = true;
            var progress = await this._achievementProgressService.LoadAsync(info.AppId);

            if (progress.HasValue)
            {
                info.SetAchievementProgress(progress.Value.Achieved, progress.Value.Total);
            }
        }

        private async Task LoadLogoAsync(GameListEntry info)
        {
            if (info.Logo != null)
            {
                return;
            }

            info.ImageUrl ??= this._gameLogoService.GetImageUrl(info.AppId);
            if (string.IsNullOrEmpty(info.ImageUrl))
            {
                return;
            }

            string imageUrl = info.ImageUrl;
            bool ownsDownload = this._logoTasks.TryGetValue(imageUrl, out var imageTask) == false;
            if (ownsDownload)
            {
                imageTask = this._gameLogoService.LoadAsync(info);
                this._logoTasks[imageUrl] = imageTask;
                this._activeLogoDownloads++;
                this.UpdateLogoStatus();
            }

            try
            {
                BitmapImage image = await imageTask;
                if (image != null)
                {
                    info.Logo = image;
                }
                else
                {
                    this._logoTasks.Remove(imageUrl);
                }
            }
            finally
            {
                if (ownsDownload)
                {
                    this._activeLogoDownloads--;
                    this.UpdateLogoStatus();
                }
            }
        }

        private void UpdateLogoStatus()
        {
            this.DownloadStatusText.Visibility = this._activeLogoDownloads > 0 ? Visibility.Visible : Visibility.Collapsed;
            this.DownloadStatusText.Text = this._activeLogoDownloads > 0
                ? $"Loading {this._activeLogoDownloads} game icons..."
                : string.Empty;
        }


        private async void OnRefresh(object sender, RoutedEventArgs e)
        {
            await this.AddGamesAsync(true);
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape || this.SearchTextBox.Text.Length == 0)
            {
                return;
            }

            e.Handled = true;
            this.SearchTextBox.Clear();
        }

        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F || Keyboard.Modifiers != ModifierKeys.Control)
            {
                return;
            }

            e.Handled = true;
            this.SearchTextBox.Focus();
            this.SearchTextBox.SelectAll();
        }

        private void OnFilterUpdate(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                this.RefreshGames();
            }
        }

        private void OnOpenFilters(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.ContextMenu == null)
            {
                return;
            }

            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = PlacementMode.Bottom;
            button.ContextMenu.HorizontalOffset = 0;
            button.ContextMenu.VerticalOffset = 4;
            button.ContextMenu.IsOpen = true;
            e.Handled = true;
        }

        private async void OnActivateGame(object sender, MouseButtonEventArgs e) =>
            await this.ActivateSelectedGameAsync();

        private async void OnGameListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            e.Handled = true;
            await this.ActivateSelectedGameAsync();
        }

        private async Task ActivateSelectedGameAsync()
        {
            if (this.GameList.SelectedItem is not GameListEntry info ||
                this._gameManagerRunning ||
                this._launchingGame)
            {
                return;
            }

            this._launchingGame = true;
            try
            {
                Rect placement = this.WindowState == WindowState.Normal
                    ? new Rect(this.Left, this.Top, this.Width, this.Height)
                    : this.RestoreBounds;
                this._gameManagerRunning = true;
                this.Hide();
                this._callbackTimer.Stop();
                int exitCode = await this._gameManagerLauncher.LaunchAndWaitAsync(
                    info.AppId,
                    placement,
                    this.WindowState);

                if (exitCode == Program.ReturnToPickerExitCode)
                {
                    this.Show();
                    this.Activate();
                    this._callbackTimer.Start();
                }
                else
                {
                    this.Close();
                }
            }
            catch (Exception ex) when (ex is Win32Exception ||
                                       ex is FileNotFoundException ||
                                       ex is InvalidOperationException)
            {
                this.Show();
                this.Activate();
                this._callbackTimer.Start();
                StyledDialog.Show(
                    this,
                    "Failed to open the game detail.\n\n" + ex.Message,
                    "Error",
                    StyledDialogButtons.Ok,
                    StyledDialogIcon.Error);
            }
            finally
            {
                this._gameManagerRunning = false;
                this._launchingGame = false;
            }
        }

        private void OnTimer(object sender, EventArgs e)
        {
            this._callbackTimer.Stop();
            this._steamClient.RunCallbacks(false);
            this._callbackTimer.Start();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            this._callbackTimer.Stop();
            this._callbackTimer.Tick -= this.OnTimer;
            this._appDataChangedCallback.OnRun -= this.OnAppDataChanged;
            this.Closed -= this.OnClosed;
        }

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }
            if (e.ClickCount == 2)
            {
                this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else
            {
                this.DragMove();
            }
        }

        private void OnMinimize(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void OnMaximize(object sender, RoutedEventArgs e) =>
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void OnClose(object sender, RoutedEventArgs e) => this.Close();

    }
}
