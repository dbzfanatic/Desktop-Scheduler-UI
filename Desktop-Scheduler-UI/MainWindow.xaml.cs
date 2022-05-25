﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

using MySql.Data.MySqlClient;

namespace Desktop_Scheduler_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<String> supportedLanguages = new List<String> { "en-US", "fr-FR" };
        protected static MySqlConnection con;
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

        private bool tryLogin(String userName, String passWord)
        {
            MySqlCommand cmd = new MySqlCommand(String.Format("SELECT * FROM user WHERE userName='{0}' AND password='{1}'",userName,passWord),con);
            String user = null;
            try
            {
                user = cmd.ExecuteScalar().ToString();
            }
            catch
            {
                return false;
            }            
            return true;
        }

        private void databaseLogic()
        {
            con = new MySqlConnection(@"server=localhost;userid=sqlUser;password=Passw0rd!;database=client_schedule");//, new SqlCredential("sqlUser", secString));
            con.Open();
            
            if (conVerChk(con))
            {
                if (tryLogin(txtUser.Text, txtPass.Password))
                {
                    txtError.Visibility = Visibility.Hidden;
                    //new window logic
                    MainView mainView = new MainView();
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
}
