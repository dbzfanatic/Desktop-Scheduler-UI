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
    /// Interaction logic for AppointmentManager.xaml
    /// </summary>
    public partial class AppointmentManager : Window
    {
        public AppointmentManager()
        {
            InitializeComponent();
        }
    }

    public class customerList
    {
        public string Name
        {
            get => Name;
            set => Name = value.PadRight(10).Substring(10); //use lambda to fit customer name to 10 characters to fit combobox, pads if name too is too short trims if name is too long
        }

        public int ID { get; set; }
    }
}
