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

            grdHome.Visibility = Visibility.Collapsed;
            grdNav.Visibility = Visibility.Collapsed;
            grdmain.RowDefinitions[1].Height = new GridLength(0);
            frmNav.Content = new ConnectionPage();
        }

        /// <summary>
        /// Affiche l'accueil après une connexion réussie.
        /// À appeler depuis la page de connexion quand l'utilisateur est validé.
        /// </summary>
        public void AfficherAccueil()
        {
            frmNav.Content = null;
            grdHome.Visibility = Visibility.Visible;
            grdNav.Visibility = Visibility.Visible;
            grdmain.RowDefinitions[1].Height = new GridLength(140);
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
            VoirUserPage page = new VoirUserPage();

            // page._Modifier += ModifierUser;
            page._Ajouter += AjouterUser;


            frmNav.Content = page;
        }

        private void AjouterUser(object? sender, EventArgs e)
        {
            frmNav.Content = null;
            AjouterUser page = new AjouterUser();
            page._Retour += VoirUserRetour;
            page._Suivant += VoirUserSuivant;
            frmNav.Content = page;
            //grdHome.Visibility=Visibility.Visible;
        }

        private void VoirUserSuivant(object? sender, EventArgs e)
        {
            if (sender is Utilisateur user)
            {

                FitTestPage page = new FitTestPage(user);

                page._Retour += FitTestRetour;
                page._Suivant += FitTestSave;

                frmNav.Content = null;
                frmNav.Content = page;
            }
        }

        private void FitTestSave(object? sender, EventArgs e)
        {
            frmNav.Content = null;
            grdHome.Visibility = Visibility.Visible;
        }

        private void FitTestRetour(object? sender, EventArgs e)
        {
            if (sender is Utilisateur user)
            {
                frmNav.Content = null;
                AjouterUser page = new AjouterUser(user);
                page._Retour += VoirUserRetour;
                page._Suivant += VoirUserSuivant;
                frmNav.Content = page;
            }

        }

        private void VoirUserRetour(object? sender, EventArgs e)
        {
            grdHome.Visibility = Visibility.Visible;
            frmNav.Content = null;
        }

        private void ModifierUser(object? sender, EventArgs e)
        {
            MessageBox.Show("Modifier n'as pas été implémenter");
        }

        private void btnHomeNav_Click(object sender, RoutedEventArgs e)
        {
            frmNav.Content = null;
            grdHome.Visibility = Visibility.Visible;
        }

        private void btnDecoNav_Click(object sender, RoutedEventArgs e)
        {
            grdHome.Visibility = Visibility.Collapsed;
            grdNav.Visibility = Visibility.Collapsed;
            grdmain.RowDefinitions[1].Height = new GridLength(0);
            frmNav.Content = new ConnectionPage();
        }

        private void btnDeconnection_Click(object sender, RoutedEventArgs e)
        {
            grdHome.Visibility = Visibility.Collapsed;
            grdNav.Visibility = Visibility.Collapsed;
            grdmain.RowDefinitions[1].Height = new GridLength(0);
            frmNav.Content = new ConnectionPage();
        }

        private void btnClients_Click(object sender, RoutedEventArgs e)
        {
            frmNav.Content = null;
            grdHome.Visibility = Visibility.Hidden;
            MesClients page = new();
            page._Ajouter += MesClientAjout;
            frmNav.Content = page;
        }

        private void MesClientAjout(object? sender, EventArgs e)
        {
            frmNav.Content = null;
            AjouterUser page = new AjouterUser();
            page._Retour += VoirUserRetour;
            page._Suivant += VoirUserSuivant;
            frmNav.Content = page;
        }
    }
}
