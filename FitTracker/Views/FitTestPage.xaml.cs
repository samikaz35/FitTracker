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

namespace FitTracker.Views
{
    /// <summary>
    /// Logique d'interaction pour FitTestPage.xaml
    /// </summary>
    public partial class FitTestPage : Page
    {
        public event EventHandler<EventArgs>? _Suivant;
        public event EventHandler<EventArgs>? _Retour;
        private Utilisateur _user = new();
        public FitTestPage(Utilisateur utilisateur)
        {
            _user = utilisateur;
            InitializeComponent();

            lblUser.Content = _user.ToString();

        }

        private void btnRetour_Click(object sender, RoutedEventArgs e)
        {
            _Retour!.Invoke(_user, null!);
        }

        private void btnSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            _user.FitTest = new FitTest()
            {
                Cardio = txtCardio.Text,
                Force = txtForce.Text,
                InformationSupplementaire = txtInfoSup.Text
            };

            Utilisateur.lstUtiliateurs.Add(_user);

            _Suivant!.Invoke(null, null!);

        }
    }
}
