using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalculadoraUDB.Models;
using CalculadoraUDB.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CalculadoraUDB.Controllers
{
    public class LoginController : Controller
    {
        IPortalUDB portalUDB;
        ISessionUDB sessionUDB;
        IHTMLParser parser;
        public LoginController(IPortalUDB _portalUDB, ISessionUDB _sessionUDB, IHTMLParser _parser)
        {
            portalUDB = _portalUDB;
            sessionUDB = _sessionUDB;
            parser = _parser;
        }

        public IActionResult Index()
        {
            try
            {
                if (sessionUDB.IsLogged())
                    return RedirectToAction("Index", "Expediente");
            }
            catch (Exception) { }
            return View();
        }

        [HttpPost]
        public async Task<string> TryToLogin(string carnet, string password)
        {
            bool logged = await portalUDB.TryToLogin(carnet, password);
            Estudiante estudiante = parser.getEstudianteModelFromHtml(portalUDB.GetHtmls());
            await estudiante.ActualizarResultados();
            sessionUDB.ActualizarEstudiante(estudiante);

            if (logged)
            {
                sessionUDB.SetLogin(estudiante);
                return JsonConvert.SerializeObject("logged");
            }
            else
            {
                sessionUDB.SetLogout();
                return JsonConvert.SerializeObject("error");
            }
        }

        public IActionResult Logout()
        {
            sessionUDB.SetLogout();
            return RedirectToAction(nameof(Index));
        }
    }
}