using System;
using System.Windows;

namespace PomodoroWpf
{
    public partial class SettingsWindow : Window
    {
        public (int work, int brk, int longBreak) Values { get; private set; }

        public SettingsWindow(int w, int b, int l)
        {
            InitializeComponent();
            WorkBox.Text = w.ToString();
            BreakBox.Text = b.ToString();
            LongBox.Text = l.ToString();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WorkBox.Text, out int w) &&
                int.TryParse(BreakBox.Text, out int b) &&
                int.TryParse(LongBox.Text, out int l) &&
                w > 0 && b > 0 && l > 0)
            {
                Values = (w, b, l);
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter valid positive integers.", "Input Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
