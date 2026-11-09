using FitTracker.Model;
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
using static FitTracker.Model.Utilisateur;

namespace FitTracker.Views
{
    /// <summary>
    /// Logique d'interaction pour VoirUserPage.xaml
    /// </summary>
    public partial class VoirUserPage : Page
    {
        //public event EventHandler<EventArgs>? _Modifier;
        public event EventHandler<EventArgs>? _Ajouter;

        private void AppliquerFiltre()
        {
            if (lstUtilisateur == null || cmbType == null
                || txtNumTel == null || txtNumMembre == null
                || Utilisateur.lstUtiliateurs == null)
            {
                return;
            }

            if (cmbType.SelectedItem is not TypeUlisateur type)
            {
                lstUtilisateur.ItemsSource = Utilisateur.lstUtiliateurs;
                return;
            }

            IEnumerable<Utilisateur> req = Utilisateur.lstUtiliateurs;

            if (!string.IsNullOrWhiteSpace(txtNumTel.Text))
            {
                req = req.Where(x => (x.Telephone ?? "").Contains(txtNumTel.Text));
            }

            if (!string.IsNullOrWhiteSpace(txtNumMembre.Text))
            {
                req = req.Where(x => (x.NumeroMembre ?? "").Contains(txtNumMembre.Text));
            }

            if (type != TypeUlisateur.Tout)
            {
                req = req.Where(x => x.UtilisateurType == type);
            }

            lstUtilisateur.ItemsSource = req.ToList();
        }

        public VoirUserPage()
        {
            InitializeComponent();
            lstUtilisateur.ItemsSource = Utilisateur.lstUtiliateurs;

            cmbType.ItemsSource = Enum.GetValues<TypeUlisateur>();
            cmbType.SelectedItem = TypeUlisateur.Tout;

        }

        private void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            AppliquerFiltre();

            if (cmbType.SelectedItem is TypeUlisateur type)
            {
               // MessageBox.Show("Choisi : " + type);

            }
        }

        private void txtNumMembre_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppliquerFiltre();
        }

        private void txtNumTel_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppliquerFiltre();
        }

        private void btnAjouter_Click(object sender, RoutedEventArgs e)
        {
            _Ajouter!.Invoke(null, null!);
        }

        private void lstUtilisateur_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
