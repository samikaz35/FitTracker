using CustomObjectWPF.Model;
using FitTracker.Model;
using FitTracker.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FitTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Notification> lstNotif = new List<Notification>()
        {
            new Notification()
            {
                Titre = "Bienvenu!",
                Description = "Bienvenu sur votre première utilisation de FitTracker",
                Emetteur = "Système"
            },
            new Notification()
            {
                Titre = "Avant de commencer.",
                Description = "Avant de commencer, veillez voir avec votre suppérieur qui sont vos client",
                Emetteur = "Système"
            }


        };


        public MainWindow()
        {
            InitializeComponent();

            lstNotification.ItemsSource = lstNotif;

          //  frmNav.Content = new VoirUtilisateurPage();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //frmNav.Content = new TemplatePage();
        }

        private void btnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button bouton && bouton.DataContext is Notification notification)
            {
                lstNotif.Remove(notification);
                lstNotification.SelectedItem = null;
                lstNotification.ItemsSource = null;

                lstNotification.ItemsSource = lstNotif;
            }
        }

        private void btnVoirUtilisateurNav_Click(object sender, RoutedEventArgs e)
        {
            grdHome.Visibility = Visibility.Hidden;
            frmNav.Content = new TemplatePage();
        }

        private void btnHomeNav_Click(object sender, RoutedEventArgs e)
        {
            frmNav.Content = null;
            grdHome.Visibility = Visibility.Visible;
        }

      

       
    }
}
