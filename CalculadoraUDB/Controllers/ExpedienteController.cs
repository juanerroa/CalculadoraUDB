using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalculadoraUDB.Models;
using CalculadoraUDB.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CalculadoraUDB.Controllers
{
    public class ExpedienteController : Controller
    {
        ISessionUDB sessionUDB;
        Estudiante estudiante;

        public ExpedienteController(ISessionUDB _sessionUDB)
        {
            sessionUDB = _sessionUDB;
            estudiante = sessionUDB.GetEstudiante();
        }

        public IActionResult Index()
        {
            try
            {
                if (!sessionUDB.IsLogged())
                    return RedirectToAction("Index", "Login");
            }
            catch (Exception) { return RedirectToAction("Index", "Login"); }

            return View(estudiante);
        }

        [HttpPost]
        public JsonResult GetJsonMateriasExpedientes(DTParameters parameters)
        {
            var data = estudiante.Expediente.ConsolidadoMaterias.ToList();

            int totalRows = data.Count();
            if (!string.IsNullOrEmpty(parameters.Search.Value)) //filter
            {
                data = data.Where(e => e.Codigo.ToLower().Contains(parameters.Search.Value) ||
                     e.Asignatura.ToLower().Contains(parameters.Search.Value) ||
                     e.Anio.ToString().ToLower().Contains(parameters.Search.Value) ||
                     e.Resultado.ToLower().Contains(parameters.Search.Value))
                    .ToList();
            }
            int totalRowsFiltered = data.Count();

            string sort = UppercaseFirst(parameters.SortOrder); //sorting
            if (sort != null)
                data = data.AsQueryable().OrderBy(sort).ToList();


            data = data.Skip(parameters.Start).Take(parameters.Length).ToList(); //Paging


            return Json(new { data = data, draw = parameters.Draw, recordsTotal = totalRows, recordsFiltered = totalRowsFiltered });
        }

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        [HttpPost]
        public async Task<string> AgregarMateria(MateriaExpediente materia)
        {
            object resultado = null;
            if (estudiante.Avance >= 100)
                resultado = new { clase = "alert alert-danger", texto = "Materia no incluida - Ya ha alcazando su avance maximo en la carrera." };
            else
            {
                estudiante.Expediente.ConsolidadoMaterias.Add(materia);
                await estudiante.ActualizarResultados();
                sessionUDB.ActualizarEstudiante(estudiante);
                resultado = new { clase = "alert alert-success", texto = "Materia agregada con exito" };
            }
            return JsonConvert.SerializeObject(resultado);
        }

        [HttpPost]
        public async Task<string> EditarMateria(MateriaExpediente materia, MateriaExpediente materiaBeforeUpdate)
        {
            object resultado = null;
            var exp = estudiante.Expediente.ConsolidadoMaterias.Where(m => m.Anio == materiaBeforeUpdate.Anio &&
                                                      m.Codigo.Equals(materiaBeforeUpdate.Codigo) &&
                                                      m.Ciclo == materiaBeforeUpdate.Ciclo);
            var materiaExp = exp.SingleOrDefault();
            materiaExp.Anio = materia.Anio;
            materiaExp.Ciclo = materia.Ciclo;
            materiaExp.Codigo = materia.Codigo;
            materiaExp.Asignatura = materia.Asignatura;
            materiaExp.Matricula = materia.Matricula;
            materiaExp.Nota = materia.Nota;
            materiaExp.Resultado = materia.Resultado;

            if (estudiante.Expediente.ConsolidadoMaterias.Where(m => m.Codigo.Equals(materia.Codigo) && (m.Resultado.Equals("Aprobada") || m.Resultado.Equals("Equivalencia"))).Count() > 1)
            {
                materiaExp = materiaBeforeUpdate;
                resultado = new { clase = "alert alert-danger", texto = "No puede tener una materia aprobada dos veces en el expediente." };
            }
            else
            {
                await estudiante.ActualizarResultados();
                sessionUDB.ActualizarEstudiante(estudiante);
                resultado = new { clase = "alert alert-success", texto = "Materia modificada con exito" };
            }
            return JsonConvert.SerializeObject(resultado);
        }

        [HttpPost]
        public async Task<string> EliminarMateria(MateriaExpediente materia)
        {
            var materiaExp = estudiante.Expediente.ConsolidadoMaterias.Where(m => m.Anio == materia.Anio &&
                                                      m.Codigo.Equals(materia.Codigo) &&
                                                      m.Ciclo == materia.Ciclo).SingleOrDefault();
            estudiante.Expediente.ConsolidadoMaterias.Remove(materiaExp);

            await estudiante.ActualizarResultados();
            sessionUDB.ActualizarEstudiante(estudiante);

            var resultado = new { clase = "alert alert-success", texto = "Materia eliminada con exito" };
            return JsonConvert.SerializeObject(resultado);
        }

        [HttpPost]
        public string GetJsonResultados()
        {
            string cum = "10";
            string promedio = "0";
            string avance = "0";
            int uvGanadas = 0;

            if (estudiante.UvGanadas != 0)
            {
                cum = Math.Round(estudiante.Cum, 2, MidpointRounding.ToEven).ToString("#.#");
                promedio = Math.Round(estudiante.Promedio, 2, MidpointRounding.ToEven).ToString("#.#");
                avance = Math.Round(estudiante.Avance, 2, MidpointRounding.ToEven).ToString("#.#");
                uvGanadas = estudiante.UvGanadas;
            }

            var resultado =
                new
                {
                    Cum = cum,
                    Promedio = promedio,
                    Avance = avance,
                    UvGanadas = uvGanadas
                };

            return JsonConvert.SerializeObject(resultado);
        }

        [HttpPost]
        public string GetJsonMateriasFaltantes()
        {
            var pensum = estudiante.Pensum.Where(m => (m.UV + estudiante.UvGanadas) <= estudiante.MaxUV).ToList();
            var materias = new List<MateriaPensum>();

            foreach (var materia in pensum)
            {
                var materiaInExp = estudiante.Expediente.ConsolidadoMaterias.Where(m => m.Codigo.Equals(materia.Codigo)).ToList();

                if (!materiaInExp.Any())
                    materias.Add(materia);
                else if (!materiaInExp.Where(m => m.Resultado.Equals("Aprobada")).Any() && !materiaInExp.Where(m => m.Resultado.Equals("Equivalencia")).Any())
                    materias.Add(materia);
            }

            return JsonConvert.SerializeObject(materias);
        }

        [HttpPost]
        public async Task<string> ReestablecerExpediente()
        {
            estudiante.Expediente.ConsolidadoMaterias = estudiante.Expediente.ExpedienteBackUp.ToList();
            await estudiante.ActualizarResultados();
            sessionUDB.ActualizarEstudiante(estudiante);

            return JsonConvert.SerializeObject("ok");
        }

        [HttpPost]
        public string GetCumDeseado(double cumDeseado)
        {
            object resultado = new { clase = "", texto = "" };
            if (estudiante.Avance >= 100)
                resultado = new { clase = "alert alert-info", texto = "Usted ya ha llegado al maximo de avance en la carrera." };
            else
            {
                double nota = estudiante.GetNotaForCumDeseado(cumDeseado);

                if (nota > 10)
                    resultado = new { clase = "alert alert-danger", texto = "Lo siento, ya no es posible que usted alcance CUM de " + cumDeseado };
                else
                    resultado = new { clase = "alert alert-info", texto = "Usted necesita obtener nota de " + nota.ToString("#.##") + " en cada una de las asignaturas para alcancer CUM de " + cumDeseado };

            }

            return JsonConvert.SerializeObject(resultado);
        }

        [HttpPost]
        public string GetChartData()
        {
            int[] anios = estudiante.Expediente.ConsolidadoMaterias.Select(m => m.Anio).Distinct().ToArray();
            List<int> aprobadas = new List<int>();
            List<int> retiradas = new List<int>();
            List<int> reprobadas = new List<int>();

            foreach (var anio in anios)
            {
                int aprobadasAnio = estudiante.Expediente.ConsolidadoMaterias.Where(m => m.Anio == anio && (m.Resultado.Equals("Aprobada") || m.Resultado.Equals("Equivalencia"))).Count();
                int retiradasAnio = estudiante.Expediente.ConsolidadoMaterias.Where(m => m.Anio == anio && (m.Resultado.Equals("Retirada") || m.Resultado.Equals("Ret. Total"))).Count();
                int reprobadasAnio = estudiante.Expediente.ConsolidadoMaterias.Where(m => m.Anio == anio && m.Resultado.Equals("Reprobada")).Count();

                aprobadas.Add(aprobadasAnio);
                retiradas.Add(retiradasAnio);
                reprobadas.Add(reprobadasAnio);
            }


            Chart chart = new Chart();
            chart.anios = anios;
            chart.Aprobadas = aprobadas.ToArray();
            chart.Retiradas = retiradas.ToArray();
            chart.Reprobadas = reprobadas.ToArray();

            return JsonConvert.SerializeObject(chart);
        }
    }
}