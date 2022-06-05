using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for AppointmentManager.xaml
    /// </summary>
    public partial class AppointmentManager : Window
    {
        public static ObservableCollection<CustomerID> cList;
        public static ObservableCollection<Appointment> apps;
        MySqlConnection con;
        private bool isEditing = false;

        public AppointmentManager() 
        {
            InitializeComponent();
        }

        public AppointmentManager(DateTime today,MySqlConnection conOut)
        {
            today = DateTime.Parse(today.ToString("yyyy-MM-dd"));
            cList = new ObservableCollection<CustomerID>();
            con = conOut;

            InitializeComponent();
            
            foreach(Customer cust in MainView.customers)
            {
                CustomerID tempCustID = new CustomerID();
                tempCustID.Name = cust.customerName;
                tempCustID.ID = cust.customerID;

                cList.Add(tempCustID);
            }

            apps = new ObservableCollection<Appointment>();

            dataGrid.ItemsSource = apps;

            GetComingAppts(today);
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GetComingAppts(DateTime dtToday)
        {
            apps.Clear();
            DateTime today = dtToday;
            string apptMgrSQL = "SELECT appointment.appointmentId, appointment.customerId,customer.customerName, appointment.userId, appointment.title, appointment.description, appointment.location, appointment.contact, appointment.type, appointment.url, appointment.start, appointment.end FROM appointment inner join customer WHERE appointment.userId=1 and customer.customerId = appointment.customerId and Date(appointment.start) >= '" + today.Year + "-" + today.ToString("MM") + "-" + (int.Parse(today.ToString("dd"))-1) + "'";
            var apptMgrCMD = new MySqlCommand(apptMgrSQL, con);
            MySqlDataReader rdr = apptMgrCMD.ExecuteReader();
            while (rdr.Read())
            {
                apps.Add(new Appointment(rdr.GetInt16(0), rdr.GetInt16(1), rdr.GetInt16(3), rdr.GetString(4), rdr.GetString(5), rdr.GetString(6), rdr.GetString(7), rdr.GetString(8), rdr.GetString(9), DateTime.Parse(rdr.GetString(10)), DateTime.Parse(rdr.GetString(11))));
            }
            rdr.Close();
        }

        private void dataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                string newApptSQL = "INSERT INTO appointment (customerId, userId, title, description, location, contact, type, url, start, end, createDate, createdBy, lastUpdate, lastUpdateBy) VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}','{13}'); Select LAST_INSERT_ID();";
                string inUpApptSQL = "SELECT appointment.appointmentId, appointment.customerId,customer.customerName, appointment.userId, appointment.title, appointment.description, appointment.location, appointment.contact, appointment.type, appointment.url, appointment.start, appointment.end FROM appointment inner join customer WHERE appointment.userId=1 and customer.customerId = appointment.customerId and appointment.appointmentId = '{0}'";
                string updateApptSQL = "UPDATE appointment set customerId='{0}', title='{1}', description='{2}', location='{3}', contact='{4}', type='{5}', url='{6}', start='{7}', end='{8}', lastUpdate='{9}', lastUpdateBy='{10}' WHERE appointmentId='{11}'";

                Appointment newTemp = new Appointment();

                newTemp = e.Row.Item as Appointment;

                string apptID = "";

                string rowDate = (e.Row.Item as Appointment).start.ToString("yyyy-MM-dd");
                DateTime rowStartTime = DateTime.Parse((e.Row.Item as Appointment).start.ToString("HH:mm:ss"));
                DateTime rowEndTime = DateTime.Parse((e.Row.Item as Appointment).end.ToString("HH:mm:ss"));

                if (!CheckBusHours((e.Row.Item as Appointment).start))
                {
                    MessageBox.Show("Start time for appointment is outside normal business hours.", "Invalid Time");
                    (dataGrid.Columns[8].GetCellContent(e.Row) as TextBlock).Background = Brushes.Red;
                    return;
                }
                if (!CheckBusHours((e.Row.Item as Appointment).end))
                {
                    MessageBox.Show("End time for appointment is outside normal business hours.", "Invalid Time");
                    (dataGrid.Columns[9].GetCellContent(e.Row) as TextBox).Background = Brushes.Red;
                    return;
                }

                foreach (var curItem in dataGrid.Items)
                {
                    if(curItem != e.Row.Item && curItem != CollectionView.NewItemPlaceholder)
                    {
                        Appointment temp = (curItem as Appointment);
                        DateTime tempStart = DateTime.Parse(temp.start.ToString("HH:mm:ss"));
                        DateTime tempEnd = DateTime.Parse(temp.end.ToString("HH:mm:ss"));                        

                        if (rowDate == temp.start.ToString("yyyy-MM-dd"))
                        {
                            if(tempStart.Ticks < rowStartTime.Ticks && rowStartTime.Ticks < tempEnd.Ticks)
                            {
                                MessageBox.Show("Start time for appointment is during an existing appointment.", "Invalid Time");
                                (dataGrid.Columns[8].GetCellContent(e.Row) as TextBox).Background = Brushes.Red;
                                return;
                            }
                            if(rowEndTime.Ticks > tempStart.Ticks && rowEndTime.Ticks < tempEnd.Ticks)
                            {
                                MessageBox.Show("End time for appointment is during an existing appointment.", "Invalid Time");
                                (dataGrid.Columns[9].GetCellContent(e.Row) as TextBox).Background = Brushes.Red;
                                return;
                            }
                        }

                    }
                }

                var aptChngCMD = new MySqlCommand(string.Format(inUpApptSQL, newTemp.apptID), con);
                MySqlDataReader aptRDR = aptChngCMD.ExecuteReader();
                while (aptRDR.Read())
                {
                    apptID = aptRDR.GetString(0);
                }
                aptRDR.Close();
                if (!int.TryParse(apptID, out int trash))
                {
                    aptChngCMD = new MySqlCommand(string.Format(newApptSQL, newTemp.custData,MainWindow.user.ID,newTemp.title,newTemp.desc,newTemp.location,newTemp.contact,newTemp.type,newTemp.url, newTemp.start.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), newTemp.end.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), MainWindow.user.Name, DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), MainWindow.user.Name), con);
                    apptID = aptChngCMD.ExecuteScalar().ToString(); // get inserted country ID

                    aptRDR.Close();

                }

                string newSQL = string.Format(updateApptSQL, newTemp.custData,newTemp.title, newTemp.desc, newTemp.location, newTemp.contact, newTemp.type, newTemp.url, newTemp.start.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), newTemp.end.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"),MainWindow.user.Name, apptID);
                aptChngCMD = new MySqlCommand(newSQL, con);
                aptRDR = (MySqlDataReader)aptChngCMD.ExecuteScalar();

                for (int i = 0; i < dataGrid.Items.Count - 2; i++) { //sub 2, one for index offset one for newitemplaceholder
                    (dataGrid.Columns[8].GetCellContent(i) as TextBlock).Background = Brushes.White;
                    (dataGrid.Columns[9].GetCellContent(i) as TextBlock).Background = Brushes.White;
                }

                GetComingAppts(DateTime.Today);
            }
        }

        private bool CheckBusHours(DateTime time)
        {
            if (time.DayOfWeek != DayOfWeek.Sunday && time.DayOfWeek != DayOfWeek.Saturday)
            {
                if (time.Hour < 8 || time.Hour > 17)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string delSQL = "DELETE FROM appointment WHERE appointmentId = {0}";
            var grid = (DataGrid)sender;
            switch (e.Key)
            {
                case Key.Delete:
                    if (!isEditing)
                    {
                        if (MessageBox.Show("Are you sure you wish to delete this appointment?", "Delete Appointment?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            foreach (var row in grid.SelectedItems)
                            {
                                //delete rows and update database
                                var delCMD = new MySqlCommand(string.Format(delSQL, (row as Appointment).apptID), con);
                                delCMD.ExecuteNonQuery();
                            }
                        }
                    }
                    break;
            }
        }

        private void dataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            isEditing = true;
        }

        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            isEditing = false;
        }
    }

    public class CustomerID
    {
        private int custID;
        private string custName;
        public string Name
        {
            get => custName;
            set => custName = value.PadRight(20).Substring(0,20); //use lambda to fit customer name to 10 characters to fit combobox, pads if name too is too short trims if name is too long
        }
        public int ID { get => custID; set => custID = value; } //use lambda for shorthand declaration for assigning to private variables to ensure proper processing and that no outside factors can change the given values (part of error prevention)

        public CustomerID(int ID,string Name)
        {
            custID = ID;
            //custName = Name;
        }
        public CustomerID() { }
    }

    public class Appointment
    {
        public int apptID { get; set; }
        public int custData { get; set; }
        public int userId { get; set; }
        public string title { get; set; }
        public string desc { get; set; }
        public string location { get; set; }
        public string contact { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }

        public Appointment() { }

        public Appointment(int appID,int cust,int UID, string titeleConstruct,string description,string loc, string cont, string typeConstruct,string uURL, DateTime startConstruct, DateTime endConstruct)
        {
            apptID = appID;
            custData = cust;
            userId = UID;
            title = titeleConstruct;
            desc = description;
            location = loc;
            contact = cont;
            type = typeConstruct;
            url = uURL;
            start = startConstruct;
            end = endConstruct;
        }
    }
}
