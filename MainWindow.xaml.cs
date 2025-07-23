using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using System.Net.Http;
using WpfApp = System.Windows.Application;
using WinForms = System.Windows.Forms;

namespace CountdownWidget
{
    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer timer;
        private DateTime targetDate;
        private NotifyIcon notifyIcon;
        private bool isTopmost = true;
        internal SimpleSettings appSettings;
        private const string CurrentVersion = "0.5.1";
        private readonly Uri UpdateCheckUri = new Uri("https://raw.githubusercontent.com/skipaq/countdown-widget/main/version.txt");
        private readonly Uri ChangelogCheckUri = new Uri("https://raw.githubusercontent.com/skipaq/countdown-widget/main/changelog.txt");
        private readonly Uri DownloadPageUri = new Uri("https://github.com/skipaq/countdown-widget/releases/latest");

        public MainWindow()
        {
            InitializeComponent();
            Task.Run(() => CheckForUpdates());
            appSettings = LoadSettings();

            // Устанавливаем позицию до отображения
            if (!double.IsNaN(appSettings.Left) && !double.IsNaN(appSettings.Top))
            {
                this.Left = appSettings.Left;
                this.Top = appSettings.Top;
            }
            else
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            // Устанавливаем дату
            if (DateTime.TryParse(appSettings.TargetDate, out DateTime date))
                targetDate = date;
            else
                targetDate = new DateTime(2025, 12, 31, 23, 59, 59);

            // Устанавливаем прозрачность
            this.Opacity = appSettings.Opacity;

            // Иконка
            this.Icon = CreateWpfIcon();

            this.Topmost = isTopmost;
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            SetupNotifyIcon();
            UpdateTimer();
        }

        private async void CheckForUpdates()
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);

                    string latestVersion = (await client.GetStringAsync(UpdateCheckUri)).Trim();
                    string changelog = await client.GetStringAsync(ChangelogCheckUri);

                    if (CompareVersions(latestVersion, CurrentVersion) > 0)
                    {
                        // ✅ Исправлено: полное имя
                        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            var updateWindow = new UpdateWindow
                            {
                                Version = latestVersion,
                                Changelog = changelog,
                                DownloadUri = DownloadPageUri
                            };
                            updateWindow.Owner = this;
                            updateWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                            updateWindow.ShowDialog();
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка проверки обновлений: {ex.Message}");
            }
        }

        private async Task<Uri> GetLatestDownloadUrl()
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                    string json = await client.GetStringAsync("https://api.github.com/repos/ твой-логин/countdown-widget/releases/latest");

                    // Простой парсинг JSON (без Newtonsoft.Json)
                    int assetIndex = json.IndexOf("\"browser_download_url\"") + 27;
                    int endQuote = json.IndexOf("\"", assetIndex);
                    string url = json.Substring(assetIndex, endQuote - assetIndex);

                    return new Uri(url);
                }
            }
            catch
            {
                // Резерв: ведёт на страницу релизов
                return new Uri("https://github.com/ твой-логин/countdown-widget/releases/latest");
            }
        }

        private int CompareVersions(string v1, string v2)
        {
            string[] p1 = v1.Split('.');
            string[] p2 = v2.Split('.');

            int length = Math.Max(p1.Length, p2.Length);
            for (int i = 0; i < length; i++)
            {
                int n1 = i < p1.Length ? int.Parse(p1[i]) : 0;
                int n2 = i < p2.Length ? int.Parse(p2[i]) : 0;

                if (n1 > n2) return 1;
                if (n1 < n2) return -1;
            }
            return 0;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            TimeSpan remaining = targetDate - DateTime.Now;

            if (remaining <= TimeSpan.Zero)
            {
                LabelBlock.Text = "";
                TimeBlock.Text = "Время пришло!";
                timer.Stop();
            }
            else
            {
                LabelBlock.Text = "Осталось:";
                TimeBlock.Text = $"{remaining.Days}д {remaining.Hours}ч {remaining.Minutes}м {remaining.Seconds}с";
            }
        }

        private SimpleSettings LoadSettings()
        {
            var settings = new SimpleSettings();
            string path = "settings.txt";

            if (File.Exists(path))
            {
                try
                {
                    foreach (string line in File.ReadAllLines(path))
                    {
                        string trimmed = line.Trim();
                        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                            continue;

                        int separator = trimmed.IndexOf('=');
                        if (separator > 0)
                        {
                            string key = trimmed.Substring(0, separator).Trim();
                            string value = trimmed.Substring(separator + 1).Trim();

                            switch (key.ToLower())
                            {
                                case "targetdate":
                                    settings.TargetDate = value;
                                    break;
                                case "opacity":
                                    if (double.TryParse(value, out double opacity) && opacity >= 0.0 && opacity <= 1.0)
                                        settings.Opacity = opacity;
                                    break;
                                case "left":
                                    if (double.TryParse(value, out double left) && !double.IsNaN(left))
                                        settings.Left = left;
                                    break;
                                case "top":
                                    if (double.TryParse(value, out double top) && !double.IsNaN(top))
                                        settings.Top = top;
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка чтения settings.txt: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                SaveSettings(settings);
            }

            return settings;
        }

        public void SaveSettings(SimpleSettings settings)
        {
            try
            {
                string content = $"TargetDate={settings.TargetDate}\nOpacity={settings.Opacity:0.0}\nLeft={settings.Left:0}\nTop={settings.Top:0}";
                File.WriteAllText("settings.txt", content);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить настройки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReloadSettings()
        {
            if (DateTime.TryParse(appSettings.TargetDate, out DateTime date))
                targetDate = date;
            UpdateTimer();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(this.Left, this.Top, this.ActualWidth, this.ActualHeight);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void TopmostButton_Click(object sender, RoutedEventArgs e)
        {
            isTopmost = !isTopmost;
            this.Topmost = isTopmost;

            TopmostButton.Foreground = isTopmost
                ? System.Windows.Media.Brushes.White
                : System.Windows.Media.Brushes.LightGray;

            TopmostButton.ToolTip = isTopmost
                ? "Выключить 'поверх всех окон'"
                : "Включить 'поверх всех окон'";
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            notifyIcon.Visible = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

        private void SetupNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Text = "Таймер обратного отсчёта";
            notifyIcon.Icon = CreateDrawingIcon();
            notifyIcon.Visible = false;

            notifyIcon.MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                    notifyIcon.Visible = false;
                }
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Показать", null, (sender, e) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                notifyIcon.Visible = false;
            });
            contextMenu.Items.Add("Закрыть", null, (sender, e) =>
            {
                notifyIcon.Visible = false;
                System.Windows.Application.Current.Shutdown();
            });

            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private System.Windows.Media.ImageSource CreateWpfIcon()
        {
            var drawing = new System.Windows.Media.GeometryDrawing();
            drawing.Brush = System.Windows.Media.Brushes.White;
            var geometry = System.Windows.Media.Geometry.Parse("M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,6V12H17L12,6Z");
            drawing.Geometry = geometry;
            return new System.Windows.Media.DrawingImage(drawing);
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private System.Drawing.Icon CreateDrawingIcon()
        {
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.FromArgb(60, 60, 60));
                using (var pen = new Pen(Color.White, 2.5f))
                {
                    g.DrawEllipse(pen, 6, 6, 20, 20);
                    g.DrawLine(pen, 16, 16, 16, 10);
                    g.DrawLine(pen, 16, 16, 20, 16);
                }
            }
            IntPtr hIcon = bitmap.GetHicon();
            var icon = System.Drawing.Icon.FromHandle(hIcon);
            DeleteObject(hIcon);
            bitmap.Dispose();
            return icon;
        }

        protected override void OnClosed(EventArgs e)
        {
            if (this.IsLoaded)
            {
                appSettings.Left = this.Left;
                appSettings.Top = this.Top;
                SaveSettings(appSettings);
            }

            notifyIcon?.Dispose();
            base.OnClosed(e);
        }
    }

    public class SimpleSettings
    {
        public string TargetDate { get; set; } = "2025-12-31 23:59:59";
        public double Opacity { get; set; } = 0.8;
        public double Left { get; set; } = double.NaN;
        public double Top { get; set; } = double.NaN;
    }
}