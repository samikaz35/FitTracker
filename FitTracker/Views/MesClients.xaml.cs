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
    /// Logique d'interaction pour MesClients.xaml
    /// </summary>
    public partial class MesClients : Page
    {
        public event EventHandler<EventArgs>? _Ajouter;



        private void AppliquerFiltre()
        {
            if (lstUtilisateur == null || txtNom == null
                || txtNumTel == null || txtNumMembre == null
                || Utilisateur.lstUtiliateurs == null)
            {
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

            if (!string.IsNullOrWhiteSpace(txtNom.Text))
            {
                req = req.Where(x => (x.NomComplet.ToLower() ?? "").Contains(txtNom.Text.ToLower()));
            }


            lstUtilisateur.ItemsSource = req.ToList();
        }



        public MesClients()
        {
            InitializeComponent();

            lstUtilisateur.ItemsSource = Utilisateur.lstUtiliateurs;

        }

        private void txtNumMembre_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppliquerFiltre();
        }

        private void txtNumTel_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppliquerFiltre();

        }

        

        private void tktNom_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppliquerFiltre();

        }

        private void lstUtilisateur_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnAjouter_Click(object sender, RoutedEventArgs e)
        {
            _Ajouter!.Invoke(null, null!);
        }

        private void btnSuivie_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
