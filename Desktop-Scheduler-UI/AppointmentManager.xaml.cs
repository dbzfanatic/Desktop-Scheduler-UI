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

        public AppointmentManager()
        {
            cList = new ObservableCollection<CustomerID>();

            InitializeComponent();
            
            foreach(Customer cust in MainView.customers)
            {
                CustomerID tempCustID = new CustomerID();
                tempCustID.Name = cust.customerName;
                tempCustID.ID = cust.customerID;

                cList.Add(tempCustID);
            }

            ObservableCollection<Appointment> apps = new ObservableCollection<Appointment>();
            apps.Add(new Appointment(1, cList[3].ID, 1, "test", "", "shed", "mike", "meeting", "http://www.google.com", DateTime.Now, DateTime.Now));
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
