using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CalculadoraUDB.Services
{
    public interface IPortalUDB
    {
        public Task<bool> TryToLogin(string carnet, string password);
        public Dictionary<string, string> GetHtmls();
        public void LogOut();
        public Task<string> GetImageAsBase64Url(string url);
    }

    public class PortalUDB : IPortalUDB
    {
        public string LOGIN_URL = "https://admacad.udb.edu.sv/Estudiantes/Default.aspx";
        private Dictionary<string, string> htmls;
        public IHttpContextAccessor context;
        public Byte[] imageEstudiante;
        public CookieContainer sessionCookies;

        public async Task<bool> TryToLogin(string carnet, string password)
        {
            sessionCookies = await GetSessionCookies(carnet, password);
            if (sessionCookies != null)
            {
                htmls = new Dictionary<string, string>();
                var procExpediente = Task.Run(() => { htmls.Add("expediente", getHTMLFromUrl("https://admacad.udb.edu.sv/Estudiantes/Estudiante/Expediente.aspx").Result); });
                var procPensum = Task.Run(() => { htmls.Add("pensum", getHTMLFromUrl("https://admacad.udb.edu.sv/Estudiantes/Estudiante/Pensum.aspx").Result); });
                var procPerfil = Task.Run(() => { htmls.Add("perfil", getHTMLFromUrl("https://admacad.udb.edu.sv/Estudiantes/Estudiante/MiPerfil.aspx").Result); });
                var procTutor = Task.Run(() => { htmls.Add("tutor", getHTMLFromUrl("https://admacad.udb.edu.sv/Estudiantes/Estudiante/Tutoria.aspx").Result); });
                var procImageEstudiante = Task.Run(() => { htmls.Add("imageEstudiante", GetImageAsBase64Url("https://admacad.udb.edu.sv/Estudiantes/Estudiante/ReturnImg.aspx").Result); });
                await Task.WhenAll(procExpediente, procPensum, procPerfil, procTutor, procImageEstudiante);

                string tutor = htmls.GetValueOrDefault("tutor");
                var doc = new HtmlDocument();
                doc.LoadHtml(htmls.GetValueOrDefault("tutor"));
                string urlImagenTutor = "https://admacad.udb.edu.sv/Estudiantes/Estudiante/" + doc.GetElementbyId("ContentPlaceHolder1_imgTutor").GetAttributes("src").SingleOrDefault().Value;
                htmls.Add("imageTutor", await GetImageAsBase64Url(urlImagenTutor));

                return true;
            }
            else
            {
                return false;
            }
        }

        public void LogOut()
        {
            htmls = null;
        }

        private async Task<string> getHTMLFromUrl(string url)
        {
            var webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "GET";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.CookieContainer = sessionCookies;
            var response = await webRequest.GetResponseAsync().ConfigureAwait(false);
            var responseReader = new StreamReader(response.GetResponseStream());
            return responseReader.ReadToEnd();
        }



        private async Task<CookieContainer> GetSessionCookies(string carnet, string password)
        {
            Dictionary<string, string> hiddenParams = GetHiddenParametersList();

            var baseAddress = new Uri(LOGIN_URL);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                //usually i make a standard request without authentication, eg: to the home page.
                //by doing this request you store some initial cookie values, that might be used in the subsequent login request and checked by the server
                var homePageResult = client.GetAsync("/");
                homePageResult.Result.EnsureSuccessStatusCode();

                var content = new FormUrlEncodedContent(new[]
                {
                    //the name of the form values must be the name of <input /> tags of the login form, in this case the tag is <input type="text" name="username">
                    new KeyValuePair<string, string>("__EVENTTARGET", hiddenParams["__EVENTTARGET"]),
                    new KeyValuePair<string, string>("__EVENTARGUMENT", hiddenParams["__EVENTARGUMENT"]),
                    new KeyValuePair<string, string>("__VIEWSTATE", hiddenParams["__VIEWSTATE"]),
                    new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", hiddenParams["__VIEWSTATEGENERATOR"]),
                    new KeyValuePair<string, string>("__EVENTVALIDATION", hiddenParams["__EVENTVALIDATION"]),
                    new KeyValuePair<string, string>("TxtUsuario", carnet),
                    new KeyValuePair<string, string>("TxtClave", password),
                    new KeyValuePair<string, string>("BtnIniciar", hiddenParams["btnLogin"]),
                });
                var loginResult = await client.PostAsync(LOGIN_URL, content);
                loginResult.EnsureSuccessStatusCode();
                string responseUri = loginResult.RequestMessage.RequestUri.ToString();

                if (responseUri.Equals("https://admacad.udb.edu.sv/Estudiantes/Estudiante/Inicio.aspx"))
                    return cookieContainer;
                else
                    return null;
            }
        }

        private Dictionary<string, string> GetHiddenParametersList()
        {
            byte[] response;

            WebClient webClient = new WebClient();
            response = webClient.DownloadData(LOGIN_URL);

            Dictionary<string, string> hiddenParams = new Dictionary<string, string>();

            hiddenParams.Add("__EVENTTARGET", ExtractHiddenParams(Encoding.ASCII.GetString(response), "__EVENTTARGET"));
            hiddenParams.Add("__EVENTARGUMENT", ExtractHiddenParams(Encoding.ASCII.GetString(response), "__EVENTARGUMENT"));
            hiddenParams.Add("__VIEWSTATE", ExtractHiddenParams(Encoding.ASCII.GetString(response), "__VIEWSTATE"));
            hiddenParams.Add("__VIEWSTATEGENERATOR", ExtractHiddenParams(Encoding.ASCII.GetString(response), "__VIEWSTATEGENERATOR"));
            hiddenParams.Add("__EVENTVALIDATION", ExtractHiddenParams(Encoding.ASCII.GetString(response), "__EVENTVALIDATION"));
            hiddenParams.Add("btnLogin", ExtractHiddenParams(Encoding.ASCII.GetString(response), "btnLogin"));

            string postData = String.Format(
               "__EVENTTARGET={0}&" +
               "__EVENTARGUMENT={1}&" +
               "__VIEWSTATE={2}&" +
               "__VIEWSTATEGENERATOR={3}&" +
               "__EVENTVALIDATION={4}&",
               "btnLogin={5}",
               hiddenParams["__EVENTTARGET"],
               hiddenParams["__EVENTARGUMENT"],
               hiddenParams["__VIEWSTATE"],
               hiddenParams["__VIEWSTATEGENERATOR"],
               hiddenParams["__EVENTVALIDATION"],
               hiddenParams["btnLogin"]);
            return hiddenParams;
        }

        private string ExtractHiddenParams(string s, string param)
        {
            try
            {
                string viewStateNameDelimiter = param;
                string valueDelimiter = "value=\"";

                int viewStateNamePosition = s.IndexOf(viewStateNameDelimiter);
                int viewStateValuePosition = s.IndexOf(valueDelimiter, viewStateNamePosition);

                int viewStateStartPosition = viewStateValuePosition + valueDelimiter.Length;
                int viewStateEndPosition = s.IndexOf("\"", viewStateStartPosition);

                return Uri.EscapeUriString(
                         s.Substring(
                            viewStateStartPosition,
                            viewStateEndPosition - viewStateStartPosition
                         )
                );
            }
            catch (ArgumentOutOfRangeException) { return ""; }
        }

        public Dictionary<string, string> GetHtmls()
        {
            return htmls;
        }

        public async Task<string> GetImageAsBase64Url(string url)
        {
            var webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "GET";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.CookieContainer = sessionCookies;
            var response = await webRequest.GetResponseAsync().ConfigureAwait(false);
            string ct = response.ContentType;
            Stream objStream = response.GetResponseStream();
            BinaryReader breader = new BinaryReader(objStream);
            byte[] buffer = breader.ReadBytes((int)response.ContentLength);

            return "data:image/aspx;base64," + Convert.ToBase64String(buffer);
        }



    }
}
