using System;
using System.Diagnostics;
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
            // Устанавливаем заголовок
            TitleBlock.Text = $"Доступна новая версия: {Version}";

            // Устанавливаем текст изменений
            ChangelogBlock.Text = Changelog?.Trim() ?? "Новые возможности и улучшения.";
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string newExePath = "CountdownWidget_new.exe";
            string updaterPath = "Updater.exe";
            string mainExe = "CountdownWidget.exe";

            try
            {
                DownloadButton.Visibility = Visibility.Collapsed;
                DownloadProgress.Visibility = Visibility.Visible;

                using (var client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("CountdownWidget");

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

                // Копируем Updater.exe рядом
                if (File.Exists(updaterPath))
                {
                    File.Copy(updaterPath, "Updater_temp.exe", true);
                }
                else
                {
                    MessageBox.Show("Файл Updater.exe не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Запускаем Updater и закрываемся
                Process.Start("Updater_temp.exe");
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}