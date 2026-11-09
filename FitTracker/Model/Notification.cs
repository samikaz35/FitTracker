using System;
using System.Collections.Generic;
using System.Text;

namespace FitTracker.Model
{
    public class Notification
    {
        public string Titre { get; set; } = "";
        public DateTime DateEmission { get; set; } = DateTime.Now;

        public string Description { get; set; } = "";

        public string Emetteur { get; set; } = "";
    }
}
