using CalculadoraUDB.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CalculadoraUDB.Services
{
    public interface IHTMLParser
    {
        Estudiante getEstudianteModelFromHtml(Dictionary<string, string> htmls);
    }

    public class HTMLParser : IHTMLParser
    {
        public Estudiante getEstudianteModelFromHtml(Dictionary<string, string> htmls)
        {
            Estudiante estudiante = new Estudiante();

            /*SE OBTIENE LOS DATOS DEL PERFIL DEL ESTUDIANTE*/
            estudiante.Perfil = getEstudiantePerfilFromHtml(htmls.GetValueOrDefault("perfil"));
            estudiante.Perfil.EstudianteImage = htmls.GetValueOrDefault("imageEstudiante");
            estudiante.Perfil.TutorImage = htmls.GetValueOrDefault("imageTutor");

            /*SE OBTIENE LOS DETALLES DE CADA MATERIA DEL PENSUM*/
            estudiante.Pensum = getPensumFromHtml(htmls.GetValueOrDefault("pensum"));

            /*CON EL PENSUM CARGADO PODEMOS SABER EL MAX UV PARA GRADUARSE*/
            estudiante.MaxUV = getMaxUV(estudiante.Pensum.ToList());

            /*SE OBTIENE EL EXPEDIENTE ACADEMICO*/
            estudiante.Expediente = new Expediente();
            estudiante.Expediente = UpdateExpedienteFromPensum(htmls.GetValueOrDefault("expediente"));

            return estudiante;
        }

        private EstudiantePerfil getEstudiantePerfilFromHtml(string HTML)
        {
            EstudiantePerfil perfil = new EstudiantePerfil();
            var doc = new HtmlDocument();
            doc.LoadHtml(HTML);
            perfil.Nombre = doc.GetElementbyId("ContentPlaceHolder1_lbNombres").InnerHtml;
            perfil.Apellido = doc.GetElementbyId("ContentPlaceHolder1_lbApellidos").InnerHtml;
            perfil.Responsable = doc.GetElementbyId("ContentPlaceHolder1_LbResponsable").InnerHtml;
            perfil.Carnet = doc.GetElementbyId("ContentPlaceHolder1_lbCarnet").InnerHtml;
            perfil.Correo = doc.GetElementbyId("ContentPlaceHolder1_LbAEmail").InnerHtml;
            perfil.Direccion = doc.GetElementbyId("ContentPlaceHolder1_LbADireccion").InnerHtml;
            perfil.Municipio = doc.GetElementbyId("ContentPlaceHolder1_LbAMunicipio").InnerHtml;
            perfil.Departamento = doc.GetElementbyId("ContentPlaceHolder1_LbADepto").InnerHtml;
            perfil.Telefono = doc.GetElementbyId("ContentPlaceHolder1_LbATelCasa").InnerHtml;
            perfil.Celular = doc.GetElementbyId("ContentPlaceHolder1_LbATelCel").InnerHtml;
            perfil.Nacimiento = doc.GetElementbyId("ContentPlaceHolder1_LbAFechaNac").InnerHtml;
            perfil.TutorNombre = doc.GetElementbyId("ContentPlaceHolder1_LbTutor").InnerHtml;

            return perfil;
        }

        private List<MateriaPensum> getPensumFromHtml(string HTML)
        {
            List<MateriaPensum> pensum = new List<MateriaPensum>();
            var doc = new HtmlDocument();
            doc.LoadHtml(HTML);
            //Obtención de materias por su class: PA PI PD PP
            var materiasHTML = doc.DocumentNode.SelectNodes("//table[contains(@class, 'PA') or " +
                                                                    "contains(@class, 'PI') or " +
                                                                    "contains(@class, 'PD') or " +
                                                                    "contains(@class, 'PP')]");
            foreach (HtmlNode node in materiasHTML)
            {
                MateriaPensum materia = new MateriaPensum();
                materia.ElectivaName = node.Attributes["name"].Value;

                var tds = node.Descendants("td");
                materia.Codigo = tds.ElementAt(0).InnerText;
                materia.UV = int.Parse(tds.ElementAt(1).InnerText);
                materia.Nombre = tds.ElementAt(2).InnerText;
                materia.Requisito = tds.ElementAt(3).InnerText;

                pensum.Add(materia);
            }

            return pensum;
        }

        private Expediente UpdateExpedienteFromPensum(string HTML)
        {
            Expediente expediente = new Expediente();
            List<MateriaExpediente> consolidadoMaterias = new List<MateriaExpediente>();
            var doc = new HtmlDocument();
            doc.LoadHtml(HTML);

            expediente.Carrera = doc.GetElementbyId("ContentPlaceHolder1_LbTituloC").InnerHtml;
            expediente.PromedioPortal = doc.GetElementbyId("ContentPlaceHolder1_LbCNotaP").InnerHtml;
            expediente.CumPortal = doc.GetElementbyId("ContentPlaceHolder1_LbCCUM").InnerHtml;
            expediente.UvGanadasPortal = doc.GetElementbyId("ContentPlaceHolder1_LbCUVG").InnerHtml;
            expediente.AvancePortal = doc.GetElementbyId("ContentPlaceHolder1_LbCPorcentaje").InnerHtml;

            var tabla = doc.GetElementbyId("ContentPlaceHolder1_GVNotas");
            IEnumerable<HtmlNode> filas = null;

            if (tabla != null)
            {
                filas = tabla.Descendants("tr");
                filas = filas.Skip(1);

                foreach (var fila in filas)
                {
                    var tds = fila.Descendants("td");
                    string codigo = tds.ElementAt(2).InnerText;

                    MateriaExpediente materia = new MateriaExpediente();
                    materia.Anio = int.Parse(tds.ElementAt(0).InnerText);
                    materia.Ciclo = int.Parse(tds.ElementAt(1).InnerText);
                    materia.Codigo = tds.ElementAt(2).InnerText;
                    materia.Asignatura = tds.ElementAt(3).InnerText;
                    materia.Matricula = int.Parse(tds.ElementAt(4).InnerText);
                    materia.Nota = double.Parse(tds.ElementAt(5).InnerText);
                    materia.Resultado = tds.ElementAt(6).InnerText;
                    consolidadoMaterias.Add(materia);
                }
            }

            expediente.ConsolidadoMaterias = consolidadoMaterias.ToList();
            expediente.ExpedienteBackUp = consolidadoMaterias.ToList();

            return expediente;
        }

        private int getMaxUV(List<MateriaPensum> pensum)
        {
            int maxUV = 0;
            int? maxElectiva = null;
            List<MateriaPensum> pensumOneElectivaPerColumn = pensum.Where(m => !m.ElectivaName.Contains("TEC")).ToList(); //Primero obtenemos las materias no electivas.

            try { maxElectiva = pensum.Where(m => m.ElectivaName.Contains("TEC")).Max(m => int.Parse(m.ElectivaName.Substring(3)));}catch (Exception) { }
            if (maxElectiva != null) //Si una columna de materias electivas se dejara solamente una materia electiva en ese bloque
            {
                for(int i=0; i<maxElectiva; i++)
                {
                    string electivaName = "TEC";
                    if ((i + 1) < 10)
                        electivaName += "0";

                    electivaName += (i + 1).ToString();

                    var electiva = pensum.Where(m => m.ElectivaName.Equals(electivaName)).FirstOrDefault();

                    pensumOneElectivaPerColumn.Add(electiva);
                }
            }

            foreach(var materia in pensumOneElectivaPerColumn)
                maxUV += materia.UV;

            return maxUV;
        }





    }
}
