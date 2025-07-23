using System;
using System.Windows;

namespace CountdownWidget
{
    public partial class OpacitySettingsWindow : Window
    {
        private MainWindow mainWindow;

        public OpacitySettingsWindow(MainWindow mainWindow)
        {
            if (mainWindow == null)
                throw new ArgumentNullException(nameof(mainWindow));

            InitializeComponent();

            this.mainWindow = mainWindow;
            this.OpacitySlider.Value = mainWindow.Opacity > 0 ? mainWindow.Opacity : 0.8;
            UpdateOpacityText();
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mainWindow == null) return;

            double opacity = OpacitySlider.Value;
            mainWindow.Opacity = opacity;
            mainWindow.appSettings.Opacity = opacity;
            mainWindow.SaveSettings(mainWindow.appSettings);
            UpdateOpacityText();
        }

        private void UpdateOpacityText()
        {
            OpacityValueText.Text = $"{OpacitySlider.Value:P0}".Replace(" %", "%");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }



        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (Owner != null)
            {
                double left = Owner.Left + (Owner.ActualWidth - this.ActualWidth) / 2;
                double top = Owner.Top + (Owner.ActualHeight - this.ActualHeight) / 2;

                var workArea = SystemParameters.WorkArea;
                left = Math.Max(workArea.Left, Math.Min(left, workArea.Right - this.ActualWidth));
                top = Math.Max(workArea.Top, Math.Min(top, workArea.Bottom - this.ActualHeight));

                this.Left = left;
                this.Top = top;
            }
        }
    }
}