using System;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace CountdownWidget
{
    public partial class UpdateWindow : Window
    {
        public string Version { get; set; }
        public string Changelog { get; set; }
        public Uri DownloadUri { get; set; }

        public UpdateWindow()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TitleBlock.Text = $"Доступна новая версия: {Version}";
            ChangelogBlock.Text = Changelog?.Trim() ?? "Новые возможности и улучшения.";
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string newExePath = "CountdownWidget_new.exe";
            string updaterTempPath = "Updater_temp.exe";
            string updaterSourcePath = "Updater.exe";

            try
            {
                if (!File.Exists(updaterSourcePath))
                {
                    MessageBox.Show(
                        $"Файл {updaterSourcePath} не найден.\n" +
                        "Убедитесь, что он находится в той же папке, что и программа.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                DownloadButton.Visibility = Visibility.Collapsed;
                DownloadProgress.Visibility = Visibility.Visible;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("CountdownWidget/1.0");

                    var response = await client.GetAsync(DownloadUri, HttpCompletionOption.ResponseHeadersRead);
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(newExePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        long totalRead = 0;
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;

                            if (totalBytes != -1)
                            {
                                double progress = (double)totalRead / totalBytes;
                                await Dispatcher.InvokeAsync(() => DownloadProgress.Value = progress * 100);
                            }
                        }
                    }
                }

                File.Copy(updaterSourcePath, updaterTempPath, true);

                System.Diagnostics.Process.Start(updaterTempPath);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
    }
}