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
        protected int targHeight = (int)(System.Windows.SystemParameters.PrimaryScreenHeight) - 290*2;

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

            lblNotification.Text = string.Format("Your appointment, {0}, is starting soon. Please be ready at {1}.", alertName, alertTime);

            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => // lambda to invoke action to begin notification animation placement
            {
                var workingArea = System.Windows.SystemParameters.WorkArea; // get usable screen size/area
                var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice; // get transform for window
                var corner = transform.Transform(new Point(workingArea.Right, workingArea.Bottom));

                Left = corner.X - this.ActualWidth - 10;
                Top = corner.Y - this.ActualHeight - 10;
            }));
        }

        private void DoubleAnimationUsingKeyFrames_Completed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
