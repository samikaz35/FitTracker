using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FitTracker.Views
{
    /// <summary>
    /// Logique d'interaction pour ConnectionPage.xaml
    /// </summary>
    public partial class ConnectionPage : Page
    {
        public ConnectionPage()
        {
            InitializeComponent();

           
        }

        private void btnConnection_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow fenetre)
            {
                fenetre.AfficherAccueil();
            }
        }

        private void btnVisible_Click(object sender, RoutedEventArgs e)
        {
            if (txtMotdePasse.Visibility == Visibility.Visible)
            {
                txtMotdePasseVisible.Text = txtMotdePasse.Password;

                txtMotdePasse.Visibility = Visibility.Hidden;
                txtMotdePasseVisible.Visibility = Visibility.Visible;
                imgVisible.Source = new BitmapImage(new Uri("pack://application:,,,/Image/MDPVisible.png"));
            }
            else
            {
                txtMotdePasse.Password = txtMotdePasseVisible.Text;
                txtMotdePasse.Visibility = Visibility.Visible;
                txtMotdePasseVisible.Visibility = Visibility.Hidden;
                imgVisible.Source = new BitmapImage(new Uri("pack://application:,,,/Image/MDPMasquer.png"));

            }
        }

      
    }
}
