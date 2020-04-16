using CalculadoraUDB.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalculadoraUDB.Services
{
    public interface ISessionUDB
    {
        public bool IsLogged();
        public void SetLogin(Estudiante estudiante);
        public void ActualizarEstudiante(Estudiante estudiante);
        public void SetLogout();
        public Estudiante GetEstudiante();
    }

    public class SessionUDB : ISessionUDB
    {
        private IHttpContextAccessor context;

        public SessionUDB(IHttpContextAccessor _context)
        {
            context = _context;
        }

        public Estudiante GetEstudiante()
        {
            try
            {
                string estudianteJson = context.HttpContext.Session.GetString("estudianteJson");
                Estudiante estudiante = JsonConvert.DeserializeObject<Estudiante>(estudianteJson);
                return estudiante;
            }
            catch (Exception) { return null;  }
        }

        public bool IsLogged()
        {
            return bool.Parse(context.HttpContext.Session.GetString("logged"));
        }

        public void SetLogin(Estudiante estudiante)
        {
            string estudianteJson = JsonConvert.SerializeObject(estudiante);
            context.HttpContext.Session.SetString("logged", "True");
            context.HttpContext.Session.SetString("estudianteJson", estudianteJson);
        }

        public void ActualizarEstudiante(Estudiante estudiante)
        {
            string estudianteJson = JsonConvert.SerializeObject(estudiante);
            context.HttpContext.Session.SetString("estudianteJson", estudianteJson);
        }

        public void SetLogout()
        {
            context.HttpContext.Session.SetString("logged", "False");
        }
    }
}
