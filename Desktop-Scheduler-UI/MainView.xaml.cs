using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
using System.Windows.Threading;
using MySql.Data.MySqlClient;

namespace Desktop_Scheduler_UI
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        MySqlConnection con;
        string[] Months = new string[12] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        public static int curMonth = 1;
        public static int curYear = 2022;
        public static ObservableCollection<Customer> customers;
        public List<DateTime> appointmentsToday = new List<DateTime>();
        public List<String> appointentNamesToday = new List<string>();
        private bool isEditing = false;

        public MainView()
        {
            InitializeComponent();
        }

        public MainView(MySqlConnection conNew)
        {
            InitializeComponent();
            con = conNew;
            curMonth = DateTime.Today.Month;
            curYear = DateTime.Today.Year;
            customers = new ObservableCollection<Customer>();

            GetAppts(curMonth);
            GetCust();

            DispatcherTimer apptTimer = new DispatcherTimer();
            apptTimer.Interval = TimeSpan.FromSeconds(5);
            apptTimer.Tick += CheckApptTimes();
            apptTimer.Start();
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
            switch(e.Key) 
            {
                case Key.Delete:
                    if (!isEditing)
                    {
                        if (MessageBox.Show("Are you sure you wish to delete this customer?", "Delete Customer?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            foreach (var row in grid.SelectedItems)
                            {
                                //delete rows and update database
                                var delCMD = new MySqlCommand(string.Format(delSQL, (row as Customer).customerID), con);
                                delCMD.ExecuteNonQuery();
                            }
                        }
                    }
                    break;                
            }
            
        }

        private void GetAppts(int Month)
        {
            DateTime today = DateTime.Today;
            DateTime first = new DateTime(curYear, Month, 1);
            List<Week> weeks = new List<Week>();
            string[] tempWeek = new string[7];

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
            string[] nextWeek = new string[7];
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
                string[] apptWeek = new string[7];
                string[] curWeek = weeks[i].ToArray();
                for (int d = 0; d < 7; d++)
                {
                    string curDay = curWeek[d];
                    if (int.TryParse(curDay, out dayOfMonth))
                    {
                        apptWeek[d] = curDay;
                        var cmd = new MySqlCommand(string.Format(sql, dayOfMonth), con);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            apptWeek[d] += "\n" + string.Format("{1} - {0}: {2}", rdr.GetString(1), rdr.GetString(0), DateTime.Parse(rdr.GetString(2)).ToLocalTime());
                            if (dayOfMonth == DateTime.Today.Day)
                            {
                                appointmentsToday.Add(DateTime.Parse(rdr.GetString(2)).ToLocalTime());
                                appointentNamesToday.Add(rdr.GetString(1));
                            }
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
            customers.Clear();
            string custSQL = "select * from customer inner join address on customer.addressId = address.addressId inner join city on address.cityId = city.cityId inner join country on city.countryId = country.countryId";
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

        private void reportMonthByType()
        {
            string typeSQL = "select Count(type),type,Month(start) from appointment group by type,Month(start) order by Count(type) desc";
                        
            var cmdType = new MySqlCommand(typeSQL, con);
            MySqlDataReader typeRDR = cmdType.ExecuteReader();
            PrintDialog printing = new PrintDialog();
            Paragraph para = new Paragraph();
            para.FontFamily = new FontFamily("Courier New");
            para.FontSize = 16;

            para.Inlines.Add(new Bold(new Run() { Text = "Monthly Meetings by Type\n\n", FontSize = 48 }));

            while (typeRDR.Read())
            {
                List<String> results = new List<String>();

                results.Add(Months[typeRDR.GetInt16(2) - 1]);
                results.Add(typeRDR.GetString(1));
                results.Add(typeRDR.GetInt16(0).ToString());

                para.Inlines.Add(new Bold(new Run("Month: " + "\t\t" + results[0] + "\r")));
                para.Inlines.Add(new Bold(new Run("Type:  " + "\t\t" + results[1] + "\r")));
                para.Inlines.Add(new Bold(new Run("Count: " + "\t\t" + results[2] + "\r\r")));
            }

            typeRDR.Close();

            string typePerMonthSQL = "select count(distinct type),Month(start) from appointment where Month(start) group by Month(start)";

            cmdType = new MySqlCommand(typePerMonthSQL, con);
            typeRDR = cmdType.ExecuteReader();
            para.Inlines.Add("\r\r");
            while (typeRDR.Read())
            {
                para.Inlines.Add(new Bold(new Run("Distinct appointment types in " + Months[typeRDR.GetInt16(1)-1] + ": \t\t" + typeRDR.GetInt16(0) + "\r")));
            }

            typeRDR.Close();

            FlowDocument typeDoc = new FlowDocument(para);
            typeDoc.ColumnWidth = printing.PrintableAreaWidth;
            typeDoc.Name = "Meeting_Type_by_Month_Report";
            IDocumentPaginatorSource idpSource = typeDoc;
            if ((bool)printing.ShowDialog())
            {
                printing.PrintDocument(idpSource.DocumentPaginator, "Printing Report");
            }
        }

        private void btnPrntRep_Click(object sender, RoutedEventArgs e)
        {
            switch (cmbReport.SelectedIndex)
            {
                case 0:
                    reportMonthByType();
                    break;
                case 1:
                    reportAllScheduled();
                    break;
                case 2:
                    reportByCountry();
                    break;
                default:
                    break;
            }
        }

        private void reportAllScheduled()
        {
            string schedSQL = "select user.userName,Date(start),Time(start),Time(end),location,contact,address.address,address.address2,city.city,address.postalCode,country.country from appointment inner join customer on appointment.customerId = customer.customerId inner join address on customer.addressId = address.addressId inner join city on address.cityId = city.cityId inner join country on city.countryId = country.countryId inner join user on appointment.userId = user.userId order by user.userID,end desc";

            var cmdSched = new MySqlCommand(schedSQL, con);
            MySqlDataReader schedRDR = cmdSched.ExecuteReader();

            PrintDialog printing = new PrintDialog();
            Paragraph para = new Paragraph();
            para.FontFamily = new FontFamily("Courier New");
            para.FontSize = 11;
            ///////////////////////////Setup Report Layout/////////////////////////
            para.Inlines.Add(new Bold(new Run() { Text = "Scheduling Report\n\n", FontSize = 48 }));
            para.Inlines.Add(new Run("----------------------------------------------------------------------------------------------------------------------\r"));
            para.Inlines.Add(new Run("|"));
            para.Inlines.Add(new Bold(new Run("Consultant:")));
            para.Inlines.Add(new Run("| |    "));
            para.Inlines.Add(new Bold(new Run("Date:   ")));
            para.Inlines.Add(new Run("| | "));
            para.Inlines.Add(new Bold(new Run("Start Time: ")));
            para.Inlines.Add(new Run("| | "));
            para.Inlines.Add(new Bold(new Run("End Time: ")));
            para.Inlines.Add(new Run("| |  "));
            para.Inlines.Add(new Bold(new Run("Location:  ")));
            para.Inlines.Add(new Run("| |  "));
            para.Inlines.Add(new Bold(new Run("Contact:  ")));
            para.Inlines.Add(new Run("| |         "));
            para.Inlines.Add(new Bold(new Run("Address:    ")));
            para.Inlines.Add(new Run("     |\r"));
            ///////////////////////////Finish Report Layout/////////////////////////
            while (schedRDR.Read())
            {
                List<String> results = new List<String>();
                results.Add(schedRDR.GetString(0));
                results.Add((schedRDR.GetString(1).Split())[0]);
                results.Add(DateTime.Parse(schedRDR.GetString(2)).ToLocalTime().ToString("hh:mm tt"));
                results.Add(DateTime.Parse(schedRDR.GetString(3)).ToLocalTime().ToString("hh:mm tt"));
                results.Add(schedRDR.GetString(4));
                results.Add(schedRDR.GetString(5));
                results.Add(schedRDR.GetString(7) != "" ? " " + schedRDR.GetString(7) + " " : " ");
                results.Add(schedRDR.GetString(6) + results[6] + schedRDR.GetString(8) + " " + schedRDR.GetString(9) + " " + schedRDR.GetString(10));

                para.Inlines.Add(new Run("|" + results[0].PadRight(11).Substring(0,11) + "| |"));
                para.Inlines.Add(new Run(results[1].PadRight(12).Substring(0, 12) + "| |"));
                para.Inlines.Add(new Run(results[2].PadRight(13).Substring(0, 13) + "| |"));
                para.Inlines.Add(new Run(results[3].PadRight(11).Substring(0, 11) + "| |"));
                para.Inlines.Add(new Run(results[4].PadRight(13).Substring(0, 13) + "| |"));
                para.Inlines.Add(new Run(results[5].PadRight(12).Substring(0, 12) + "| |"));
                para.Inlines.Add(new Run(results[7].PadRight(26).Substring(0, 26) + "|\r"));
            }
            schedRDR.Close();
            para.Inlines.Add(new Run("----------------------------------------------------------------------------------------------------------------------\r"));
            FlowDocument schedDoc = new FlowDocument(para);
            schedDoc.ColumnWidth = printing.PrintableAreaWidth;
            schedDoc.TextAlignment = TextAlignment.Center;
            schedDoc.Name = "Scheduling_Report";
            IDocumentPaginatorSource idpSource = schedDoc;
            if ((bool)printing.ShowDialog())
            {
                printing.PrintDocument(idpSource.DocumentPaginator, "Printing Scheduling Report");
            }
        }

        private void reportByCountry()
        {
            string countrySQL = "select Count(country),country from country inner join city on country.countryId = city.countryId inner join address on city.cityID = address.cityId inner join customer on address.addressId = customer.addressId inner join appointment on customer.customerId = appointment.customerId group by country order by count(country) desc";

            var countryType = new MySqlCommand(countrySQL, con);
            MySqlDataReader countryRDR = countryType.ExecuteReader();
            PrintDialog printing = new PrintDialog();
            Paragraph para = new Paragraph();
            para.FontFamily = new FontFamily("Courier New");
            para.FontSize = 16;

            para.Inlines.Add(new Bold(new Run() { Text = "Appointments by Country\n\n", FontSize = 48 }));

            while (countryRDR.Read())
            {
                List<String> results = new List<String>();

                results.Add(countryRDR.GetString(1));
                results.Add(countryRDR.GetInt16(0).ToString());

                para.Inlines.Add(new Bold(new Run("Country:  " + "\t\t" + results[0] + "\r")));
                para.Inlines.Add(new Bold(new Run("Count: " + "\t\t" + results[1] + "\r\r")));
            }

            countryRDR.Close();

            FlowDocument typeDoc = new FlowDocument(para);
            typeDoc.ColumnWidth = printing.PrintableAreaWidth;
            typeDoc.Name = "Meeting_Number_by_Country_Report";
            IDocumentPaginatorSource idpSource = typeDoc;
            if ((bool)printing.ShowDialog())
            {
                printing.PrintDocument(idpSource.DocumentPaginator, "Printing Report");
            }

        }

        private void ShowWeek_Click(object sender, RoutedEventArgs e)
        {
            dataGrid_MouseDoubleClick(null, null);
        }

        private void dgCustomers_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                string newCustSQL = "INSERT INTO customer (customerName, addressId, active, createDate, createdBy, lastUpdate, lastUpdateBy) VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}'); Select LAST_INSERT_ID();";
                string newAddySQL = "INSERT INTO address (address,address2,cityId,postalCode,phone,createDate,CreatedBy,lastUpdate,lastUpdateBy)  VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}'); Select LAST_INSERT_ID();";
                string newCitySQL = "INSERT INTO city (city,countryId,createDate,createdBy,lastUpdate,lastUpdateBy)  VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}'); Select LAST_INSERT_ID();";
                string newCountrySQL = "INSERT INTO country (country,createDate,createdBy,lastUpdate,lastUpdateBy)  VALUES('{0}', '{1}', '{2}', '{3}', '{4}'); Select LAST_INSERT_ID();";

                string inUpCustSQL = "Select customerId,customerName,active from customer where customerName='{0}' and customerId='{1}'";
                string inUpAddySQL = "Select addressId,address,address2,postalCode,phone from address WHERE address = '{0}' and address2 = '{1}'";
                string inUpCitySQL = "Select cityId,city from city where city = '{0}'";
                string inUpCountrySQL = "Select countryId,country from country WHERE country='{0}'";

                string updateCustSQL = "update customer,address,city,country set customerName='{0}', active='{1}', customer.lastUpdate='{2}',address.lastUpdate='{2}',customer.lastUpdateBy='{3}', address.lastUpdateBy='{3}',address.phone='{4}',customer.addressId='{6}',address.cityId='{7}',address.postalCode='{8}',city.countryId='{9}' where customerId = {5} and address.addressId='{6}' and city.cityId='{7}' and country.countryId='{9}';";

                string countryID = "";
                string cityID = "";
                string addressID = "";
                string customerID = "";

                Customer newTemp = new Customer();

                newTemp = e.Row.Item as Customer;


                ///////////////////////////Begin Country Logic///////////////////////////
                var custCMD = new MySqlCommand(string.Format(inUpCountrySQL, newTemp.country), con);
                MySqlDataReader custRDR = custCMD.ExecuteReader();
                while (custRDR.Read())
                {
                    countryID = custRDR.GetString(0);
                }
                custRDR.Close();
                if (!int.TryParse(countryID, out int trash))
                {
                    custCMD = new MySqlCommand(string.Format(newCountrySQL, newTemp.country, DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), MainWindow.user.Name, DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), MainWindow.user.Name), con);
                    countryID = custCMD.ExecuteScalar().ToString(); // get inserted country ID

                    custRDR.Close();

                }
                //////////////////////////End country logic///////////////////////

                ///////////////////////////Begin City Logic///////////////////////////
                custCMD = new MySqlCommand(string.Format(inUpCitySQL, newTemp.city), con);
                custRDR = custCMD.ExecuteReader();
                while (custRDR.Read())
                {
                    cityID = custRDR.GetString(0);
                }
                custRDR.Close();
                if (!int.TryParse(cityID, out trash))
                {
                    custCMD = new MySqlCommand(string.Format(newCitySQL, newTemp.city, countryID, DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), MainWindow.user.Name, DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), MainWindow.user.Name), con);
                    cityID = custCMD.ExecuteScalar().ToString(); // get inserted city ID

                    custRDR.Close();

                }
                //////////////////////////End City logic///////////////////////

                ///////////////////////////Begin Address Logic///////////////////////////
                custCMD = new MySqlCommand(string.Format(inUpAddySQL, newTemp.address, newTemp.address2), con);
                custRDR = custCMD.ExecuteReader();
                while (custRDR.Read())
                {
                    addressID = custRDR.GetString(0);
                }
                custRDR.Close();
                if (!int.TryParse(addressID, out trash))
                {
                    custCMD = new MySqlCommand(string.Format(newAddySQL, newTemp.address, newTemp.address2, cityID, newTemp.zip, newTemp.phone, DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), MainWindow.user.Name, DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), MainWindow.user.Name), con);
                    addressID = custCMD.ExecuteScalar().ToString(); // get inserted city ID

                    custRDR.Close();

                }
                //////////////////////////End Address logic///////////////////////

                ///////////////////////////Begin Customer Logic///////////////////////////
                custCMD = new MySqlCommand(string.Format(inUpCustSQL, newTemp.customerName,(e.Row.Item as Customer).customerID), con);
                custRDR = custCMD.ExecuteReader();
                while (custRDR.Read())
                {
                    customerID = custRDR.GetString(0);
                }
                custRDR.Close();
                if (!int.TryParse(customerID, out trash))
                {
                    custCMD = new MySqlCommand(string.Format(newCustSQL, newTemp.customerName, addressID, newTemp.active, DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), MainWindow.user.Name, DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), MainWindow.user.Name), con);
                    customerID = custCMD.ExecuteScalar().ToString(); // get inserted city ID

                    custRDR.Close();

                }
                //////////////////////////End Customer logic///////////////////////

                custCMD = new MySqlCommand(string.Format(updateCustSQL, newTemp.customerName, newTemp.active, DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), MainWindow.user.Name, newTemp.phone, int.Parse((dgCustomers.Columns[0].GetCellContent(e.Row) as TextBlock).Text), addressID,cityID,newTemp.zip,countryID), con);
                custRDR = (MySqlDataReader)custCMD.ExecuteScalar();

                GetCust();
            }

        }

        private EventHandler CheckApptTimes()
        {
            int iApt = 0;
            if (appointmentsToday.Count > 0)
            {
                foreach (DateTime aptTime in appointmentsToday)
                {
                    if (aptTime <= DateTime.Now.AddMinutes(15) && aptTime > DateTime.Now)
                    {
                        new NotificationWindow(appointentNamesToday[iApt], appointmentsToday[iApt].ToString("HH:mm tt")).Show();
                    }
                    iApt++;
                }
            }

            return null;
        }

        private void btnAddApt_Click(object sender, RoutedEventArgs e)
        {
            AppointmentManager apptMan = new AppointmentManager(DateTime.Today,con);
            apptMan.Show();
        }

        private void dgCustomers_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            isEditing = true;
        }

        private void dgCustomers_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            isEditing = false;
        }

        private void ShowAptMan_Click(object sender, RoutedEventArgs e)
        {
            btnAddApt_Click(null, null);
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

        public Week(string sunday, string monday, string tuesday, string wednesday, string thursday, string friday, string saturday)
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

        public string[] ToArray()
        {
            string[] days = new string[7] { Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday };
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

        public string[] ToArray()
        {
            string[] custValues = new string[12] { customerID.ToString(), customerName, active.ToString(), address, address2, city, country, zip.ToString(), phone, addressID.ToString(), countryID.ToString(), cityID.ToString() };
            return custValues;
        }
    }
}
