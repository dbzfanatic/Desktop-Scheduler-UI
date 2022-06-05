using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
    /// Interaction logic for WeekView.xaml
    /// </summary>
    public partial class WeekView : Window
    {
        public WeekView(Week curWeek, MySqlConnection con = null)
        {            
            InitializeComponent();

            DateTime today = DateTime.Today;
            var sql = "SELECT * FROM appointment inner join customer ON appointment.customerID = customer.customerId inner join address on customer.addressID = address.addressId inner join city on address.cityId = city.cityId inner join country on city.countryId = country.countryId WHERE appointment.userId='{1}' and appointment.start between '" + MainView.curYear + "-" + MainView.curMonth + "-" + "{0}' and '" + MainView.curYear + "-" + MainView.curMonth + "-" + "{0} 23:59:59'"; //prepared appt lookup SQL string formatted for string.format


            string[] weekDays = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            string[] days = curWeek.ToArray();
            string[,] apptDays = new string[7,2];
            List<string[]> apptData = new List<string[]>();

            int dayOfMonth;
            for(int i= 0; i < 7;i++)
            {
                string apptString = "";
                if (!int.TryParse(days[i], out dayOfMonth) && days[i] != "")
                {
                    dayOfMonth = int.Parse((days[i]).Split(null, 2)[0]);
                }

                var cmd = new MySqlCommand(string.Format(sql, dayOfMonth,MainWindow.user.ID), con);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    apptString += string.Format("{1} - {0}: {2}\t\tLocation: {3}: {6} {7}, {9} {8} {10}\t{4} for {5}\n", rdr.GetString(3), rdr.GetString(6), DateTime.Parse(rdr.GetString(9)).ToLocalTime(), rdr.GetString(5), rdr.GetString(7), rdr.GetString(16),rdr.GetString(24), rdr.GetString(25), rdr.GetString(27),rdr.GetString(34),rdr.GetString(41));
                }
                rdr.Close();

                apptDays[i,1] = apptString;
                apptDays[i,0] = weekDays[i] + "\n" + Regex.Replace(dayOfMonth.ToString(),@"(?<!\d+)0(?!\d+)","");
                apptData.Add(new string[] { apptDays[i, 0] , apptDays[i,1]});
            }
            dataGrid.ItemsSource = apptData;

            this.Title = ("Week of " + apptData[0][0].Replace("\n", " ") + " - " + apptData[6][0]).Replace("\n"," ");
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            for(int i = 0; i < 7; i++)
            {
                if (dataGrid.Columns.Count > 0)
                {
                    //possibly adjust font size here
                }
            }
        }

        private void dataGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            dataGrid.UnselectAllCells();
        }
    }
}
