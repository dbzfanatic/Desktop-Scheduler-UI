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
    /// Interaction logic for WeekView.xaml
    /// </summary>
    public partial class WeekView : Window
    {
        public WeekView(Week curWeek)
        {            
            InitializeComponent();

            String[] weekDays = new String[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            String[] stringSplit;
            curWeek.Thursday = curWeek.Thursday + curWeek.Thursday + curWeek.Thursday + curWeek.Thursday;
            String[] days = curWeek.ToArray();
            String[,] apptDays = new String[7,2];
            List<String[]> apptData = new List<String[]>();
            for(int i= 0; i < 7;i++)
            {
                if (int.TryParse(days[i], out int trash))
                {
                    stringSplit = new String[] { days[i], "" };
                }
                else if (days[i] != "")
                {
                    stringSplit = (days[i]).Split(null, 2);
                }
                else
                {
                    stringSplit = new String[] { "", "" };
                }

                apptDays[i,1] = stringSplit[1];
                apptDays[i,0] = weekDays[i] + "\n" + stringSplit[0];
                apptData.Add(new String[] { apptDays[i, 0] , apptDays[i,1]});
            }
            dataGrid.ItemsSource = apptData;

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            for(int i = 0; i < 7; i++)
            {
                if (dataGrid.Columns.Count > 0)
                {
                    /*((DataGridTextColumn)dataGrid.Columns[i]).FontSize = Width / 150;
                    Height = dataGrid.Height + 25;*/
                }
            }
        }

        private void dataGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            dataGrid.UnselectAllCells();
        }
    }
}
