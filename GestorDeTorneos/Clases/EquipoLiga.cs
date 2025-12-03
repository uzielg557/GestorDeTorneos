using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestorDeTorneos.Clases
{
    public class EquipoLiga
    {
        public string Nombre { get; set; }
        public int PartidosJugados { get; set; }
        public int Ganados { get; set; }
        public int Empatados { get; set; }
        public int Perdidos { get; set; }
        public int GolesFavor { get; set; }
        public int GolesContra { get; set; }
        public int Diferencia => GolesFavor - GolesContra;
        public int Puntos => (Ganados * 3) + Empatados;
    }
}
