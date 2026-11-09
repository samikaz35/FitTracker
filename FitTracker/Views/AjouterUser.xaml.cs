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
using static System.Net.Mime.MediaTypeNames;

namespace FitTracker.Views
{
    /// <summary>
    /// Logique d'interaction pour AjouterUser.xaml
    /// </summary>
    public partial class AjouterUser : Page
    {
        public event EventHandler<EventArgs>? _Suivant;
        public event EventHandler<EventArgs>? _Retour;



        public AjouterUser()
        {
            InitializeComponent();

            List<TypeUlisateur> lstType = Enum.GetValues<TypeUlisateur>().ToList();
            lstType.RemoveAt(0);
            cmbType.ItemsSource = lstType;
            cmbType.SelectedItem = TypeUlisateur.Client;

        }

        public AjouterUser(Utilisateur user)
        {
            InitializeComponent();

            List<TypeUlisateur> lstType = Enum.GetValues<TypeUlisateur>().ToList();
            lstType.RemoveAt(0);
            cmbType.ItemsSource = lstType;
            cmbType.SelectedItem = TypeUlisateur.Client;

            txtInfo.Text = user.InformationImportant;
            txtNom.Text = user.NomComplet;
            txtNumEntraineur.Text = user.NumeroEntraineurAssigner;
            txtNumMembre.Text = user.NumeroMembre;
            txtObjectif.Text = user.Objectif;
            txtTel.Text = user.Telephone;
            cmbType.SelectedItem = user.UtilisateurType;


        }


        private void btnRetour_Click(object sender, RoutedEventArgs e)
        {
            _Retour?.Invoke(this, EventArgs.Empty);
        }

        private void btnSuivant_Click(object sender, RoutedEventArgs e)
        {
            string prenom = "";
            string nom = "";

            if (txtNom.Text.Contains(' '))
            {
                int indexEspace = txtNom.Text.IndexOf(' ');
                 prenom = txtNom.Text.Substring(0, indexEspace);
                 nom = txtNom.Text.Substring(indexEspace + 1);

            }
            else
            {
                 prenom = txtNom.Text;
            }


            Utilisateur utilisateur = new Utilisateur()
            {
                InformationImportant = txtInfo.Text,
                Nom = nom,
                Prenom = prenom,
                NumeroEntraineurAssigner = txtNumEntraineur.Text,
                Objectif = txtObjectif.Text,
                Telephone = txtTel.Text,
                NumeroMembre = txtNumMembre.Text,
                UtilisateurType = (Utilisateur.TypeUlisateur)cmbType.SelectedItem



            };

            _Suivant!.Invoke(utilisateur, null!);
        }
    }
}
