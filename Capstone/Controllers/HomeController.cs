using Capstone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BCrypt.Net;
using System.Web.Security;

namespace Capstone.Controllers
{
    public class HomeController : Controller
    {
        private readonly ModelDbContext context = new ModelDbContext();
        public ActionResult Index()
        {
            return View();
        }

        // Action per visualizzare il blog
        public ActionResult Blog()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // GET
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Utenti nuovoUtente)
        {
            if (ModelState.IsValid)
            {
                // Verifica se l'username è già stato utilizzato
                if (context.Utenti.Any(u => u.Username == nuovoUtente.Username))
                {
                    ModelState.AddModelError("Username", "Username già in uso. Scegli un altro username.");
                    return View(); // Torna alla vista di registrazione con un messaggio di errore
                }

                // Imposta il valore predefinito per Ruolo e IsArtista

                //nuovoUtente.Ruolo= "user";
                nuovoUtente.IsArtista = false;
                nuovoUtente.Password = HashPassword(nuovoUtente.Password);
                // Se l'username è unico, procedi con la registrazione
                context.Utenti.Add(nuovoUtente);
                context.SaveChanges();

                return RedirectToAction("Login");
            }

            //    // Se il modello non è valido, torna alla vista di registrazione con gli errori di validazione
            return View(nuovoUtente);
        }


        // GET
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var user = context.Utenti.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                if (VerifyPassword(password, user.Password))
                {
                    FormsAuthentication.SetAuthCookie(username, false);
                    //ViewBag.AuthSuccess = "Login effettuato con successo!";

                    // Crea un cookie con il nome dell'immagine dell'utente
                    var cookie = new HttpCookie("Avatar", user.FotoUtente);
                    Response.Cookies.Add(cookie);
                    TempData["success"] = "Login effettuato con successo!";
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.AuthError = "Credenziali non valide!";
            TempData["error"] = "Credenziali non valide!";
            return View();
        }


        // Metodo per l'hash della password
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Metodo per la verifica della password
        private bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        //Logout
        public ActionResult Logout() 
        {
            FormsAuthentication.SignOut();
             return RedirectToAction("Index", "Home");
         }

    }
}