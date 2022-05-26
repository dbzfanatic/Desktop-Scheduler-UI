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
            DateTime first = new DateTime(today.Year,today.Month,1);
            List<Week> weeks = new List<Week>();
            //weeks.Add(new Week { Sunday = 1.ToString(), Monday = 2.ToString(), Tuesday = 3.ToString(), Wednesday = 4.ToString(), Thursday = 5.ToString(), Friday = 6.ToString(), Saturday = 7.ToString() });
            String[] tempWeek = new String[7];
            if (first.DayOfWeek.ToString() == "Sunday")
            {
                weeks.Add(new Week{Sunday = 1.ToString(), Monday = 2.ToString(), Tuesday = 3.ToString(), Wednesday = 4.ToString(), Thursday = 5.ToString(), Friday = 6.ToString(), Saturday = 7.ToString() });
                tempWeek = new string[7]{ 1.ToString(), 2.ToString(), 3.ToString(), 4.ToString(), 5.ToString(), 6.ToString(), 7.ToString()};
            }
            else
            {
                
                for (int i = 0; i < 7; i++)
                {
                    if(first.AddDays(i).DayOfWeek.ToString() != "Sunday")
                    {
                        tempWeek[i] = "";
                    }
                }
                weeks.Add(new Week(tempWeek[0], tempWeek[1], tempWeek[2], tempWeek[3], tempWeek[4], tempWeek[5], tempWeek[6]));
            }
            if(today.Month.ToString() != "February")
            {
                String[] nextWeek = new String[7];
                int monthEnd = int.Parse(first.AddMonths(1).AddDays(-1).Day.ToString());
                for (int w = 1; w < 5; w++)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        int nextDay = int.Parse(tempWeek[6]) + i + 1;                        
                        if (nextDay < monthEnd+1)
                        {
                            nextWeek[i] = nextDay.ToString();
                            //nextWeek[i] = nextWeek[i] + "\nTest";
                        }
                        else
                        {
                            nextWeek[i] = "";
                        }
                        
                    }
                    tempWeek[6] = nextWeek[6];
                    weeks.Add(new Week(nextWeek[0], nextWeek[1], nextWeek[2], nextWeek[3], nextWeek[4], nextWeek[5], nextWeek[6]));
                }
            }
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

        public Week(String sunday, String monday, String tuesday, String wednesday, String thursday, String friday, String saturday)
        {
            Sunday = sunday;
            Monday= monday;
            Tuesday= tuesday;
            Wednesday= wednesday;
            Thursday= thursday;
            Friday= friday;
            Saturday = saturday;
        }

        public Week() { }
    }
}
