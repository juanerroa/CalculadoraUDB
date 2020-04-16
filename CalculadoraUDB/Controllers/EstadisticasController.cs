using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalculadoraUDB.Models;
using CalculadoraUDB.Services;
using Microsoft.AspNetCore.Mvc;

namespace CalculadoraUDB.Controllers
{
    public class EstadisticasController : Controller
    {
        ISessionUDB sessionUDB;
        Estudiante estudiante;

        public EstadisticasController(ISessionUDB _sessionUDB)
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


            ViewBag.topAprobadas = estudiante.GetTopAprobadas();
            ViewBag.topReprobadas = estudiante.GetTopReprobadas();
            ViewBag.topRecursadas = estudiante.GetTopRecursadas();
            return View(estudiante);
        }
    }
}