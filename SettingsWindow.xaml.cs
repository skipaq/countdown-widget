using System;
using System.IO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace CountdownWidget
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(double mainWindowLeft, double mainWindowTop, double mainWindowWidth, double mainWindowHeight)
        {
            InitializeComponent();
            PositionWindow(mainWindowLeft, mainWindowTop, mainWindowWidth, mainWindowHeight);
        }

        private void PositionWindow(double mainLeft, double mainTop, double mainWidth, double mainHeight)
        {
            double thisWidth = this.Width;
            double thisHeight = this.Height;

            double top = mainTop + (mainHeight - thisHeight) / 2;

            double left;

            if (mainLeft < SystemParameters.WorkArea.Width / 2)
                left = mainLeft + mainWidth + 10;
            else
                left = mainLeft - thisWidth - 10;

            var workArea = SystemParameters.WorkArea;
            if (left < workArea.Left) left = workArea.Left;
            if (left + thisWidth > workArea.Right) left = workArea.Right - thisWidth;
            if (top < workArea.Top) top = workArea.Top;
            if (top + thisHeight > workArea.Bottom) top = workArea.Bottom - thisHeight;

            this.Left = left;
            this.Top = top;
        }

        private void ChangeTimeButton_Click(object sender, RoutedEventArgs e)
        {
            var inputWindow = new DateTimeInputWindow();
            inputWindow.Owner = this;

            if (inputWindow.ShowDialog() == true)
            {
                string input = inputWindow.Result;

                if (string.IsNullOrWhiteSpace(input))
                    return;

                if (DateTime.TryParse(input, out DateTime newDate))
                {
                    try
                    {
                        if (this.Owner is MainWindow mainWindow)
                        {
                            mainWindow.appSettings.TargetDate = input;
                            mainWindow.SaveSettings(mainWindow.appSettings);

                            MessageBox.Show($"Время успешно изменено!\nНовая дата: {newDate:dd.MM.yyyy HH:mm:ss}",
                                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

                            mainWindow.ReloadSettings();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка записи файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Неверный формат даты.\n\nИспользуйте:\nГГГГ-ММ-ДД ЧЧ:ММ:СС\nНапример: 2025-12-31 23:59:59",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
        }

        private void OpacityButton_Click(object sender, RoutedEventArgs e)
        {
            var opacityWindow = new OpacitySettingsWindow((MainWindow)this.Owner);
            opacityWindow.Owner = this;
            opacityWindow.ShowDialog();
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