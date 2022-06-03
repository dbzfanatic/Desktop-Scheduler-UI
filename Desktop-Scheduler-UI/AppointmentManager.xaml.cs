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

        public AppointmentManager() 
        {
            InitializeComponent();
        }

        public AppointmentManager(DateTime today,MySqlConnection con)
        {
            today = DateTime.Parse(today.ToString("yyyy-MM-dd"));
            cList = new ObservableCollection<CustomerID>();

            InitializeComponent();
            
            foreach(Customer cust in MainView.customers)
            {
                CustomerID tempCustID = new CustomerID();
                tempCustID.Name = cust.customerName;
                tempCustID.ID = cust.customerID;

                cList.Add(tempCustID);
            }

            apps = new ObservableCollection<Appointment>();

            string apptMgrSQL = "SELECT appointment.appointmentId, appointment.customerId,customer.customerName, appointment.userId, appointment.title, appointment.description, appointment.location, appointment.contact, appointment.type, appointment.url, appointment.start, appointment.end FROM appointment inner join customer WHERE appointment.userId=1 and customer.customerId = appointment.customerId and Date(appointment.start) < '" + today.Year + "-" + today.ToString("MM") + "-" + today.ToString("dd") + "'";
            var apptMgrCMD = new MySqlCommand(apptMgrSQL, con);
            MySqlDataReader rdr = apptMgrCMD.ExecuteReader();
            while (rdr.Read())
            {
                apps.Add(new Appointment(rdr.GetInt16(0), x=> new CustomerID { ID = rdr.GetInt16(1), Name = rdr.GetString(2) },rdr.GetInt16(3), rdr.GetString(4), rdr.GetString(5), rdr.GetString(6), rdr.GetString(7), rdr.GetString(8), rdr.GetString(9),DateTime.Parse(rdr.GetString(10)),DateTime.Parse(rdr.GetString(11))));
            }
            rdr.Close();
                apps.Add(new Appointment(1, cList[3], 1, "test", "", "shed", "mike", "meeting", "http://www.google.com", DateTime.Now, DateTime.Now));
            dataGrid.ItemsSource = apps;
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
            custName = Name;
        }
        public CustomerID() { }
    }

    public class Appointment
    {
        public int apptID { get; set; }
        public CustomerID custData { get; set; }
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

        public Appointment(int appID,CustomerID cust,int UID, string titeleConstruct,string description,string loc, string cont, string typeConstruct,string uURL, DateTime startConstruct, DateTime endConstruct)
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
