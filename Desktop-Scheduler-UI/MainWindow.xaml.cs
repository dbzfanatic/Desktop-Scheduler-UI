using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Desktop_Scheduler_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<String> supportedLanguages = new List<String> { "en-US", "fr-FR" };
        public MainWindow()
        {
            InitializeComponent();

            CultureInfo curCult = CultureInfo.CurrentCulture;
            Assembly language = null;
            
            if (curCult.ToString() != "en-US")
            {
                try{
                    language = Assembly.LoadFrom(".\\" + curCult.ToString() + "\\Desktop-Scheduler-UI.resources.dll"); //try to load the proper language DLL
                }
                catch(Exception e)
                {
                    String caption = "Exception";
                    if (!supportedLanguages.Contains(curCult.ToString())) //check if language is supported, if not change the caption of the alert to be more clear
                    {
                        caption = "Unsupported Language";
                    }
                    MessageBox.Show("The following error occurred: \n" + e.ToString(),caption); //show error
                    return;
                }
                ResourceManager rm = new ResourceManager(String.Format("Desktop_Scheduler_UI.Properties.Resources.{0}",curCult.ToString()), language);
                lblPass.Content = rm.GetString("loginString");
                lblUser.Content = rm.GetString("userString");
                txtWelcome.Text = rm.GetString("welcomeString");
                btnExit.Content = rm.GetString("exitString");
                btnLogin.Content = rm.GetString("loginString");

            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
