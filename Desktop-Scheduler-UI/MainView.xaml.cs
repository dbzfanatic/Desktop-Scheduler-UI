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
        String[] Months = new String[12] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        public static int curMonth = 1;
        public static int curYear = 2022;
        public MainView(MySqlConnection conNew)
        {
            InitializeComponent();
            con = conNew;
            curMonth = DateTime.Today.Month;
            curYear = DateTime.Today.Year;

            GetAppts(curMonth);
            GetCust();

            
        }

        private void dataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var test = dataGrid.SelectedItems;
            WeekView curWeek = new WeekView((Week)test[0], con);
            curWeek.Show();
        }

        private void frmMainView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                dataGrid.RowHeight = (Height - dataGrid.ColumnHeaderHeight) / 6;
                for (int i = 0; i < 7; i++)
                {
                    (dataGrid.Columns.ElementAt(i) as DataGridTextColumn).FontSize = 12;
                }
                for (int i = 0; i < 9; i++)
                {
                    if (i == 2)
                    {
                        continue;
                    }
                    (dgCustomers.Columns.ElementAt(i) as DataGridTextColumn).FontSize = 12;
                }
                lblMonth.FontSize = 12;
                btnMonthNext.Margin = new Thickness(Width * .425, 0, 0, 0);
                btnMonthPrev.Margin = new Thickness(Width * .05, 0, 0, 0);
            }
            else
            {
                dataGrid.RowHeight = (Height - dataGrid.ColumnHeaderHeight) / 2.7;
                for(int i = 0; i < 7; i++)
                {
                    (dataGrid.Columns.ElementAt(i) as DataGridTextColumn).FontSize = 24;
                }
                for (int i = 0; i < 9; i++)
                {
                    if(i == 2)
                    {
                        continue;
                    }
                    (dgCustomers.Columns.ElementAt(i) as DataGridTextColumn).FontSize = 18;
                }
                btnMonthPrev.Margin = new Thickness(Width * .05, 0, 0, 0);
                btnMonthNext.Margin = new Thickness(Width * .90, 0, 0, 0);
                lblMonth.FontSize = 28;
            }

            btnMonthNext.Width = Width * .038;
            btnMonthPrev.Width = Width * .038;
            
        }

        private void dgCustomers_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            GetCust();
        }

        private void dgCustomers_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(e.OriginalSource is ScrollViewer)
            {
                dgCustomers.UnselectAll();
            }
        }

        private void dataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(e.OriginalSource is ScrollViewer)
            {
                dataGrid.UnselectAll();
            }
        }

        private void dgCustomers_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string delSQL = "DELETE FROM customer WHERE customerId = {0}";
            var grid = (DataGrid)sender;
            if(e.Key == Key.Delete)
            {
                foreach(var row in grid.SelectedItems)
                {
                    //delete rows and update database
                    var delCMD = new MySqlCommand(string.Format(delSQL, (row as Customer).customerID),con);
                    delCMD.ExecuteNonQuery();
                }
            }
        }

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            String repGenSQL = "select Count(type),type,Month(start) from appointment group by type,Month(start) order by Count(type) desc";
        }

        private void GetAppts(int Month)
        {
            DateTime today = DateTime.Today;
            DateTime first = new DateTime(curYear, Month, 1);
            List<Week> weeks = new List<Week>();
            String[] tempWeek = new String[7];

            var sql = "SELECT appointment.contact, appointment.title,Time(appointment.start) FROM appointment inner join customer ON appointment.customerID = customer.customerId WHERE appointment.userId=1 and appointment.start between '" + curYear + "-" + curMonth + "-" + "{0}' and '" + curYear + "-" + curMonth + "-" + "{0} 23:59:59'"; //prepared appt lookup SQL string formatted for string.format

            lblMonth.Content = Months[curMonth - 1] + " " + curYear.ToString();

            dataGrid.ItemsSource = null;
            dataGrid.Items.Clear();

            if (first.DayOfWeek.ToString() == "Sunday")
            {
                weeks.Add(new Week { Sunday = 1.ToString(), Monday = 2.ToString(), Tuesday = 3.ToString(), Wednesday = 4.ToString(), Thursday = 5.ToString(), Friday = 6.ToString(), Saturday = 7.ToString() });
                tempWeek = new string[7] { 1.ToString(), 2.ToString(), 3.ToString(), 4.ToString(), 5.ToString(), 6.ToString(), 7.ToString() };
            }
            else
            {

                for (int i = 0; i < 7; i++)
                {
                    if (first.AddDays(-i).DayOfWeek.ToString() != "Sunday")
                    {
                        tempWeek[i] = "";
                    }
                    else
                    {
                        for (int q = 0; q < 7 - (i); q++)
                        {
                            tempWeek[i + q] = (q + 1).ToString();
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
                    if (nextDay < monthEnd + 1)
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
            for (int i = 0; i < 5; i++)
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

        private void GetCust()
        {
            List<Customer> customers = new List<Customer>();
            String custSQL = "select * from customer inner join address on customer.addressId = address.addressId inner join city on address.cityId = city.cityId inner join country on city.countryId = country.countryId";
            var custCMD = new MySqlCommand(custSQL, con);
            MySqlDataReader custRDR = custCMD.ExecuteReader();
            while (custRDR.Read())
            {
                Customer tempcust = new Customer();
                tempcust.customerID = custRDR.GetInt16(0);
                tempcust.customerName = custRDR.GetString(1);
                tempcust.active = custRDR.GetInt16(3);
                tempcust.address = custRDR.GetString(9);
                tempcust.address2 = custRDR.GetString(10);
                tempcust.zip = custRDR.GetInt32(12);
                tempcust.phone = custRDR.GetString(13);
                tempcust.city = custRDR.GetString(19);
                tempcust.country = custRDR.GetString(26);
                tempcust.addressID = custRDR.GetInt16(2);
                tempcust.cityID = custRDR.GetInt16(11);
                tempcust.countryID = custRDR.GetInt16(20);
                customers.Add(tempcust);
            }
            custRDR.Close();
            dgCustomers.ItemsSource = customers;
        }

        private void btnMonthPrev_Click(object sender, RoutedEventArgs e)
        {
            curMonth--;
            if(curMonth < 1)
            {
                curMonth = 12;
                curYear--;
            }
            GetAppts(curMonth);
        }

        private void btnMonthNext_Click(object sender, RoutedEventArgs e)
        {
            curMonth++;
            if(curMonth > 12)
            {
                curMonth = 1;
                curYear++;
            }
            GetAppts(curMonth);
        }

        private void btnSched_Click(object sender, RoutedEventArgs e)
        {
            String schedSQL = "select user.userName,start,end,location,contact,address.address,address.address2,city.city,address.postalCode,country.country from appointment inner join customer on appointment.customerId = customer.customerId inner join address on customer.addressId = address.addressId inner join city on address.cityId = city.cityId inner join country on city.countryId = country.countryId inner join user on appointment.userId = user.userId order by user.userID,end desc";
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
            Monday = monday;
            Tuesday = tuesday;
            Wednesday = wednesday;
            Thursday = thursday;
            Friday = friday;
            Saturday = saturday;
        }

        public Week() { }

        public String[] ToArray()
        {
            String[] days = new String[7] { Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday };
            return days;
        }
    }

    public class Customer
    {
        public int customerID { get; set; }
        public string customerName { get; set; }
        public int active { get; set; }
        public string address { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public int zip { get; set; }
        public string phone { get; set; }
        public int addressID { get; set; }
        public int countryID { get; set; }
        public int cityID { get; set; }
    }
}
