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

using MySql.Data.MySqlClient;

namespace Desktop_Scheduler_UI
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        MySqlConnection con;
        public MainView(MySqlConnection conNew)
        {
            InitializeComponent();
            con = conNew;

            DateTime today = DateTime.Today;
            DateTime first = new DateTime(today.Year,today.Month,1);
            List<Week> weeks = new List<Week>();
            String[] tempWeek = new String[7];

            var sql = "SELECT appointment.contact, appointment.title,Time(appointment.start) FROM appointment inner join customer ON appointment.customerID = customer.customerId WHERE appointment.userId=1 and appointment.start between '" + today.Year + "-" + today.Month + "-" + "{0}' and '" + today.Year + "-" + today.Month + "-" + "{0} 23:59:59'"; //prepared appt lookup SQL string formatted for string.format


            if (first.DayOfWeek.ToString() == "Sunday")
            {
                weeks.Add(new Week{Sunday = 1.ToString(), Monday = 2.ToString(), Tuesday = 3.ToString(), Wednesday = 4.ToString(), Thursday = 5.ToString(), Friday = 6.ToString(), Saturday = 7.ToString() });
                tempWeek = new string[7]{ 1.ToString(), 2.ToString(), 3.ToString(), 4.ToString(), 5.ToString(), 6.ToString(), 7.ToString()};
            }
            else
            {
                
                for (int i = 0; i < 7; i++)
                {
                    if(first.AddDays(-i).DayOfWeek.ToString() != "Sunday")
                    {
                        tempWeek[i] = "";
                    }
                    else
                    {
                        for(int q = 0; q < 7-(i); q++)
                        {
                            tempWeek[i+q] = (q+1).ToString();
                        }
                        break;
                    }
                }
                weeks.Add(new Week(tempWeek[0], tempWeek[1], tempWeek[2], tempWeek[3], tempWeek[4], tempWeek[5], tempWeek[6]));
            }
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
                    }
                    else
                    {
                        nextWeek[i] = "";
                    }
                        
                }
                tempWeek[6] = nextWeek[6];
                weeks.Add(new Week(nextWeek[0], nextWeek[1], nextWeek[2], nextWeek[3], nextWeek[4], nextWeek[5], nextWeek[6]));
            }

            int dayOfMonth;
            for(int i = 0; i < 5; i++)
            {
                String[] apptWeek = new string[7];
                String[] curWeek = weeks[i].ToArray();
                for (int d = 0; d < 7; d++)
                {
                    String curDay = curWeek[d];
                    if (int.TryParse(curDay, out dayOfMonth))
                    {
                        apptWeek[d] = curDay;
                        var cmd = new MySqlCommand(string.Format(sql, dayOfMonth), con);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            apptWeek[d] += "\n" + string.Format("{1} - {0}: {2}", rdr.GetString(1), rdr.GetString(0), DateTime.Parse(rdr.GetString(2)).ToLocalTime());
                        }
                        rdr.Close();
                    }
                    else
                    {
                        apptWeek[d] = curDay;
                    }
                    
                }
                dataGrid.ItemsSource = null;
                dataGrid.Items.Clear();
                weeks[i] = new Week(apptWeek[0], apptWeek[1], apptWeek[2], apptWeek[3], apptWeek[4], apptWeek[5], apptWeek[6]);
            }
            dataGrid.ItemsSource = weeks;
        }

        private void dataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //dataGrid.RowHeight = this.Height / 5.1;
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var test = dataGrid.SelectedItems;
            WeekView curWeek = new WeekView((Week)test[0],con);
            curWeek.Show();
        }

        private void frmMainView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            dataGrid.RowHeight = (Height-dataGrid.ColumnHeaderHeight) / 5;
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

        public String[] ToArray()
        {
            String[] days = new String[7] { Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday };
            return days;
        }
    }
}
