using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalculadoraUDB.Models
{
    public class Expediente
    {
        public string Carrera { get; set; }
        public string CumPortal { get; set; }
        public string PromedioPortal { get; set; }
        public string UvGanadasPortal { get; set; }
        public string AvancePortal { get; set; }
        public List<MateriaExpediente> ConsolidadoMaterias { get; set; }
        public List<MateriaExpediente> ExpedienteBackUp { get; set; }
    }
}
