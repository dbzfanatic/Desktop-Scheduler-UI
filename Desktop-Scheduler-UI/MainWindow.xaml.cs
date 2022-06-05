using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
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

using MySql.Data.MySqlClient;

namespace Desktop_Scheduler_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> supportedLanguages = new List<string> { "en-US", "fr-FR" };
        protected static MySqlConnection con;

        public static User user = new User();

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
                    string caption = "Exception";
                    if (!supportedLanguages.Contains(curCult.ToString())) //check if language is supported, if not change the caption of the alert to be more clear
                    {
                        caption = "Unsupported Language";
                    }
                    MessageBox.Show("The following error occurred: \n" + e.ToString(),caption); //show error
                    return;
                }
                ResourceManager rm = new ResourceManager(string.Format("Desktop_Scheduler_UI.Properties.Resources.{0}",curCult.ToString()), language);
                lblPass.Content = rm.GetString("passString");
                lblUser.Content = rm.GetString("userString");
                txtWelcome.Text = rm.GetString("welcomeString");
                btnExit.Content = rm.GetString("exitString");
                btnLogin.Content = rm.GetString("loginString");
                txtError.Text = rm.GetString("loginErrorString");
            }            
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            databaseLogic();            
        }

        private bool conVerChk(MySqlConnection con)
        {
            if (con.ServerVersion != "8.0.25")
            {
                MessageBox.Show("MySQL Server must match lab environment. Please install version 8.0.25", "Version Mismatch");
                return false; 
            }
            return true;
        }

        private bool tryLogin(string userName, string passWord)
        {
            MySqlCommand cmd = new MySqlCommand(string.Format("SELECT userId,userName FROM user WHERE userName='{0}' AND password='{1}' and active='1'",userName,passWord),con);
            
            try
            {
                MySqlDataReader usrRDR =  cmd.ExecuteReader();
                user.ID = usrRDR.GetInt32(0);
                user.Name = usrRDR.GetString(1);
            }
            catch
            {
                return false;
            }            
            return true;
        }

        private void databaseLogic()
        {
            con = new MySqlConnection(@"server=localhost;userid=sqlUser;password=Passw0rd!;database=client_schedule");//connection string in code is bad practice but easy for school and verification
            con.Open();
            
            if (conVerChk(con))
            {
                if (tryLogin(txtUser.Text, txtPass.Password))
                {
                    File.AppendAllText("signins-log.txt", string.Format("\nNew login detected: {0}", DateTime.Now.ToString()));  //automatically creates the file if it does not exist or opens it
                   
                    txtError.Visibility = Visibility.Hidden;
                    //new window logic
                    MainView mainView = new MainView(con); //pass database connection to new window instead of opening a new one
                    Application.Current.MainWindow = mainView; //set mainwindow to new window so when it closes the entire app closes
                    mainView.Show();
                    this.Close();
                }
                else
                {
                    txtError.Visibility = Visibility.Visible;
                }
            }
        }

        private void txtPass_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter || e.Key == Key.Return)
            {
                databaseLogic();
            }
        }
    }

    public class User
    {
        private int userID { get; set; }
        private string userName { get; set; }

        public int ID {
            get  => userID;
            set => userID = value;
        }
        public string Name 
        {
            get => userName;
            set => userName = value;
        }
    }
}
