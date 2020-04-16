using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalculadoraUDB.Models
{
    public class MateriaExpediente
    {
        public int Anio { get; set; }

        public int Ciclo { get; set; }

        public string Codigo { get; set; }

        public string Asignatura {get; set;}

        public int Matricula { get; set; }

        public double Nota { get; set; }

        public string Resultado { get; set; }
    }
}
