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
            curWeek.Thursday = curWeek.Thursday + curWeek.Thursday + curWeek.Thursday + curWeek.Thursday;
            dataGrid.ItemsSource = new List<Week> { curWeek };

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            for(int i = 0; i < 7; i++)
            {
                ((DataGridTextColumn)dataGrid.Columns[i]).FontSize = Width / 150;
                Height = dataGrid.Height+25;
            }
        }

        private void dataGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            dataGrid.UnselectAllCells();
        }
    }
}
