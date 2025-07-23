using System;
using System.Windows;
using System.Net.Http;

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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TitleBlock.Text = $"Доступна новая версия: {Version}";
            ChangelogBlock.Text = Changelog;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = DownloadUri.ToString(),
                UseShellExecute = true
            });
            this.Close();
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