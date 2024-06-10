using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AnimatedTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer(DispatcherPriority.Send)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (s, a) =>
            {
                time -= TimeSpan.FromSeconds(1);
                if (time >= TimeSpan.Zero)
                {
                    timerView.Time = time;
                }
                else
                {
                    timer.Stop();
                }
            };
        }

        private DispatcherTimer timer;
        private TimeSpan time;

        private void Start()
        {
            timer.Stop();
            time = TimeSpan.FromMinutes(60);
            timerView.Time = time;
            timer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }
    }
}