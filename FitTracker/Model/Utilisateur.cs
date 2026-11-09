using System;
using System.Collections.Generic;
using System.Text;

namespace FitTracker.Model
{
    public class Utilisateur
    {
        public static List<Utilisateur> lstUtiliateurs = new List<Utilisateur>() 
        {
            new Utilisateur { 
            Nom = "Drouin",
            Prenom = "Samuel",
            NumeroMembre = "#2268797",
            Telephone = "514 423-1234",
            InformationImportant = "possède le diabete",
            UtilisateurType = Utilisateur.TypeUlisateur.Admins,
            Objectif = "Perdre 169Lbs en 2 semaine",
            FitTest = new FitTest(){
                    Cardio = "5 Km/H",
                    Force = "5kg",
                    InformationSupplementaire = "A fait une crise cardiaque après 2 min de course"
                },
            NumeroEntraineurAssigner = "#E23F56"
            },
            new Utilisateur {
            Nom = "Boulet",
            Prenom = "Jérôme",
            NumeroMembre = "#2555423",
            Telephone = "514 423-4516",
            InformationImportant = "Déteste s'entrainer",
            UtilisateurType = Utilisateur.TypeUlisateur.Entraineur,
            Objectif = "Former le plus de clients au québec",
            FitTest = new FitTest(){
                    Cardio = "180 Km/H",
                    Force = "5000000kg",
                    InformationSupplementaire = "Aussi vite que flash"
                },
            NumeroEntraineurAssigner = "#E23F56"
            },
            new Utilisateur {
            Nom = "Hébert",
            Prenom = "Christopher",
            NumeroMembre = "#1728345",
            Telephone = "514 423-1412",
            InformationImportant = "Nip : 1846",
            UtilisateurType = Utilisateur.TypeUlisateur.Client,
            Objectif = "être moins proche de la mort",
            FitTest = new FitTest(){
                    Cardio = "j'en sais rien Km/H",
                    Force = "Mathématique",
                    InformationSupplementaire = "Gay"
                },
            NumeroEntraineurAssigner = "#E23F56"
            }


        };

        public enum TypeUlisateur {
            Tout,
            Admins,
            Entraineur,
            Client
        
        }


        public string NumeroMembre { get; set; } = "";

        public string NomComplet { get => Prenom + " " + Nom; }

        public string Nom { get; set; } = "";

        public string Prenom { get; set; } = "";

        public string Telephone { get; set; } = "";

        public FitTest FitTest { get; set; } = new();

        public string Objectif { get; set; } = "";

        public string InformationImportant { get; set; } = "";

        public TypeUlisateur UtilisateurType { get; set; } = Utilisateur.TypeUlisateur.Client;

        public string NumeroEntraineurAssigner { get; set; } = "#0000000";


        public override string ToString()
        {
            return $"Nom : {Prenom} {Nom}, Membre : {NumeroMembre} ";
        }


    }
}
