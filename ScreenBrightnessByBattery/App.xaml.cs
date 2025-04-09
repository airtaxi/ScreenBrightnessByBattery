using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using ScreenBrightnessByBattery.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenBrightnessByBattery;

public partial class App : Application
{
    private const string UpdateAvailableTitle = "Update Available";
    private const int UpdateCheckIntervalInMinutes = 10;
    private static readonly Timer UpdateCheckTimer;
    private static readonly string VersionFilePath = Path.Combine(AppContext.BaseDirectory, "version");

    static App() => UpdateCheckTimer = new(UpdateCheckTimerCallback, null, (int)TimeSpan.FromMinutes(UpdateCheckIntervalInMinutes).TotalMilliseconds, Timeout.Infinite);

    private static async void UpdateCheckTimerCallback(object state) => await CheckForUpdateAsync();

    private static async Task CheckForUpdateAsync()
    {
        try
        {
            var url = "https://raw.githubusercontent.com/airtaxi/ScreenBrightnessByBattery/master/latest";
            var remoteVersionString = await HttpHelper.GetContentFromUrlAsync(url);
            if (remoteVersionString is null) return;
            else remoteVersionString = remoteVersionString.Trim();

            var localVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var remoteVersion = new Version(remoteVersionString);
            if (localVersion >= remoteVersion) return;

            string lastVersionString = null;
            if (File.Exists(VersionFilePath))
            {
                lastVersionString = File.ReadAllText(VersionFilePath);
                if (remoteVersionString == lastVersionString) return;
            }

            if (remoteVersionString != lastVersionString) File.WriteAllText(VersionFilePath, remoteVersionString);

            var builder = new AppNotificationBuilder()
                .AddText(UpdateAvailableTitle)
                .AddText($"A new version ({remoteVersion}) is available.\nDo you want to download it?")
                .AddArgument("versionString", remoteVersionString);

            var notificationManager = AppNotificationManager.Default;
            notificationManager.Show(builder.BuildNotification());
        }
        catch (HttpRequestException) { } // Ignore 
        finally { UpdateCheckTimer.Change((int)TimeSpan.FromMinutes(UpdateCheckIntervalInMinutes).TotalMilliseconds, Timeout.Infinite); }
    }

    private Window _mainWindow;

    public App()
    {
        // Check for duplicated running instances.
        var currentProcess = Process.GetCurrentProcess();
        var processes = Process.GetProcessesByName(currentProcess.ProcessName);

        // If there is more than one instance, kill the current one.
        if (processes.Length > 1)
        {
            currentProcess.Kill();
            return;
        }

        Environment.CurrentDirectory = AppContext.BaseDirectory;

        InitializeComponent();
    }

    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        AppNotificationManager notificationManager = AppNotificationManager.Default;
        notificationManager.NotificationInvoked += OnNotificationManagerNotificationInvoked;
        notificationManager.Register();

        var activatedArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
        var activationKind = activatedArgs.Kind;

        if (activationKind != ExtendedActivationKind.AppNotification) LaunchAndBringToForegroundIfNeeded();
        else HandleNotification((AppNotificationActivatedEventArgs)activatedArgs.Data);

        await CheckForUpdateAsync();
    }

    private static void OnNotificationManagerNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args) => HandleNotification(args);

    private static void HandleNotification(AppNotificationActivatedEventArgs args)
    {
        var versionString = args.Arguments["versionString"];
        if (versionString != null)
        {
            var url = "https://github.com/airtaxi/ScreenBrightnessByBattery/releases/tag/" + versionString;
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }

    private void LaunchAndBringToForegroundIfNeeded()
    {
        if (_mainWindow == null)
        {
            _mainWindow = new MainWindow();
            _mainWindow.Activate();
        }
        else
        {
            WindowHelper.ShowWindow(_mainWindow);
        }
    }

    private static partial class WindowHelper
    {
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetForegroundWindow(IntPtr hWnd);

        public static void ShowWindow(Window window)
        {
            // Bring the window to the foreground... first get the window handle...
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Restore window if minimized... requires DLL import above
            ShowWindow(hwnd, 0x00000009);

            // And call SetForegroundWindow... requires DLL import above
            SetForegroundWindow(hwnd);
        }
    }

}
