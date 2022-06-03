using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Desktop_Scheduler_UI
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private DispatcherTimer timer;
        private DispatcherTimer lifeTimer;

        private int startXPos;
        private int startYPos;
        private int secPassed;

        private string alertName;
        private string alertTime;

        public NotificationWindow()
        {
            InitializeComponent();
        }

        public NotificationWindow(String aName, String aTime)
        {

            InitializeComponent();

            alertName = aName;
            alertTime = aTime;

            Topmost = true;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            //timer.Tick += timerTick();
            timer.Start();

            Timer test = new Timer(timerTick,null,100,0);

            lifeTimer = new DispatcherTimer();
            lifeTimer.Interval = TimeSpan.FromSeconds(1);
            lifeTimer.Tick += lifeTimerTick();
            lifeTimer.Start();

            startXPos = (int)(System.Windows.SystemParameters.PrimaryScreenWidth - Width - 10);
            startYPos = (int)(System.Windows.SystemParameters.PrimaryScreenHeight);

            Top = startYPos;
            Left = startXPos;

            lblNotification.Text = string.Format("Your appointment: {0} is starting soon. Please be ready by {1}", alertName, alertTime);
        }

        private void frmNotWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        void timerTick(object state)
        {
            startYPos -= 5;
            if (startYPos <= (int)(System.Windows.SystemParameters.PrimaryScreenHeight) - Height - 5)
            {
                timer.Stop();
                Console.WriteLine("Stopped timer");
            }
            else
            {
                Top = startYPos;
            }
            return;
        }

        EventHandler lifeTimerTick()
        {
            Console.WriteLine("Seconds passed: " + secPassed);
            if (secPassed >= 30)
            {
                this.Close();
            }
            else
            {
                secPassed++;
            }

            return null;
        }
    }
}
