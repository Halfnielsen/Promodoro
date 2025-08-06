using System;
using System.ComponentModel;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Input; // For NotifyIcon

namespace PomodoroWpf
{//Before changes
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Durations (minutes)
   
        private int WorkMinutes = 25;
        private int BreakMinutes = 5;
        private int LongBreakMinutes = 15;

        private readonly Timer _timer = new(1000);
        private TimeSpan _timeLeft;
        private bool _isRunning;
        private int _pomodorosCompleted;
        private bool _inWorkPhase = true;

        private readonly NotifyIcon _tray = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            _timeLeft = TimeSpan.FromMinutes(WorkMinutes);
            UpdateTimeText();

            _timer.Elapsed += (_, _) => Dispatcher.Invoke(Tick);

            // Tray icon
            _tray.Icon = System.Drawing.SystemIcons.Application;
            _tray.Visible = true;
            _tray.Text = "Pomodoro Timer";
        }

        private void StartPause_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
            {
                _timer.Stop();
                _isRunning = false;
                StartPauseBtn.Content = "Start";
            }
            else
            {
                _timer.Start();
                _isRunning = true;
                StartPauseBtn.Content = "Pause";
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _inWorkPhase = true;
            _timeLeft = TimeSpan.FromMinutes(WorkMinutes);
            _pomodorosCompleted = 0;
            _isRunning = false;
            StartPauseBtn.Content = "Start";
            UpdateTimeText();
            DrawProgress(0);
            StateText.Text = "Ready";
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SettingsWindow(WorkMinutes, BreakMinutes, LongBreakMinutes);
            if (dlg.ShowDialog() == true)
            {
                (WorkMinutes, BreakMinutes, LongBreakMinutes) = dlg.Values;
                if (!_inWorkPhase) _timeLeft = TimeSpan.FromMinutes(BreakMinutes);
                UpdateTimeText();
            }
        }

        private void Tick()
        {
            _timeLeft -= TimeSpan.FromSeconds(1);
            UpdateTimeText();
            DrawProgress(1 - _timeLeft.TotalSeconds / PhaseTotalSeconds());

            if (_timeLeft <= TimeSpan.Zero)
            {
                System.Media.SystemSounds.Beep.Play();
                _timer.Stop();
                _isRunning = false;
                StartPauseBtn.Content = "Start";

                if (_inWorkPhase)
                {
                    _pomodorosCompleted++;
                    _tray.ShowBalloonTip(2000, "Pomodoro complete!",
                        "Time for a break.", ToolTipIcon.Info);

                    if (_pomodorosCompleted % 4 == 0)
                    {
                        _timeLeft = TimeSpan.FromMinutes(LongBreakMinutes);
                        StateText.Text = "Long Break";
                    }
                    else
                    {
                        _timeLeft = TimeSpan.FromMinutes(BreakMinutes);
                        StateText.Text = "Break";
                    }
                }
                else
                {
                    _tray.ShowBalloonTip(2000, "Break over",
                        "Ready for the next Pomodoro.", ToolTipIcon.Info);
                    _timeLeft = TimeSpan.FromMinutes(WorkMinutes);
                    StateText.Text = "Work";
                }

                _inWorkPhase = !_inWorkPhase;
                DrawProgress(0);
            }
        }

        private double PhaseTotalSeconds() =>
            _inWorkPhase
                ? WorkMinutes * 60
                : ((_pomodorosCompleted % 4 == 0 && !_inWorkPhase) ? LongBreakMinutes * 60 : BreakMinutes * 60);

        private void UpdateTimeText()
        {
            TimeText.Text = _timeLeft.ToString(@"mm\:ss");
        }

        // Simple circular progress arc
        private void DrawProgress(double percent)
        {
            const double Radius = 120;
            const double StartAngle = -90; // top center

            double endAngle = StartAngle + percent * 360;
            Point start = Polar(Radius, StartAngle);
            Point end = Polar(Radius, endAngle);

            bool largeArc = percent > 0.5;

            var geom = new PathGeometry();
            var figure = new PathFigure { StartPoint = Center(start) };
            var arc = new ArcSegment
            {
                Point = Center(end),
                Size = new Size(Radius, Radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = largeArc
            };
            figure.Segments.Add(arc);
            geom.Figures.Add(figure);
            ProgressArc.Data = geom;
        }

        private static Point Polar(double r, double deg)
        {
            double rad = deg * Math.PI / 180;
            return new Point(r * Math.Cos(rad), r * Math.Sin(rad));
        }

        private Point Center(Point p) => new(Width / 2 + p.X - 8, Height / 2 + p.Y - 40); // tweak offsets

        private void Ellipse_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}
