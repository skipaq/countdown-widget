using System;
using System.Windows;

namespace CountdownWidget
{
    public partial class DateTimeInputWindow : Window
    {
        public string Result { get; private set; }

        public DateTimeInputWindow()
        {
            InitializeComponent();
            InputTextBox.Text = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            InputTextBox.SelectAll();
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

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Result = InputTextBox.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}