using System;
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

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Открываем ссылку в браузере
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = DownloadUri.ToString(),
                    UseShellExecute = true  // обязательно для URL
                });
            }
            catch (Exception ex)
            {
                // На случай ошибки (редко)
                MessageBox.Show(
                    $"Не удалось открыть страницу:\n{ex.Message}\n\nПерейдите вручную: {DownloadUri}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            this.Close();
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