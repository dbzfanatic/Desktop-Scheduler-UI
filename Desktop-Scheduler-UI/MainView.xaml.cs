using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            DateTime today = DateTime.Today;
            Console.WriteLine(today.DayOfWeek);
            List<Week> weeks = new List<Week>();
            weeks.Add(new Week { Sunday = 1.ToString(), Monday = 2.ToString(), Tuesday = 3.ToString(), Wednesday = 4.ToString(), Thursday = 5.ToString(), Friday = 6.ToString(), Saturday = 7.ToString() });
            dataGrid.ItemsSource = weeks;
        }
    }

    public class Week
    {
        public string Sunday { get; set; }
        public string Monday { get; set; }
        public string Tuesday { get; set; }
        public string Wednesday { get; set; }
        public string Thursday { get; set; }
        public string Friday { get; set; }
        public string Saturday { get; set; }
    }
}
