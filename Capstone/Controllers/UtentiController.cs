using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Capstone.Models;

namespace Capstone.Controllers
{
    public class UtentiController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        public ActionResult MyProfile()
        {
            // Verifica se l'utente è autenticato
            if (!User.Identity.IsAuthenticated)
            {
                // Se l'utente non è autenticato, reindirizza alla pagina di accesso
                return RedirectToAction("Login", "Account");
            }

            // Ottieni l'IDUtente dell'utente loggato
            string username = User.Identity.Name;
            var utente = db.Utenti.FirstOrDefault(u => u.Username == username);

            if (utente == null)
            {
                // Se l'utente non viene trovato nel database, gestisci l'errore
                return HttpNotFound();
            }

            // Passa i dettagli dell'utente alla vista per visualizzarli
            return View(utente);
        }


        // GET: Utenti
        public ActionResult Index()
        {
            return View(db.Utenti.ToList());
        }

        // GET: Utenti/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Utenti utenti = db.Utenti.Find(id);
            if (utenti == null)
            {
                return HttpNotFound();
            }
            return View(utenti);
        }

        // GET: Utenti/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Utenti/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdUtente,Email,Username,Password,FotoUtente,IsArtista,Ruolo")] Utenti utenti)
        {
            if (ModelState.IsValid)
            {
                db.Utenti.Add(utenti);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(utenti);
        }

        // GET: Utenti/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Utenti utenti = db.Utenti.Find(id);
            if (utenti == null)
            {
                return HttpNotFound();
            }
            return View(utenti);
        }

        // POST: Utenti/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind(Include = "IdUtente,Email,Username")] Utenti utenti, string newPassword, string confirmNewPassword, HttpPostedFileBase uploadImage)
        {
            ModelState.Remove("Password");
            if (id != utenti.IdUtente)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (newPassword != confirmNewPassword)
            {
                ModelState.AddModelError("ConfirmNewPassword", "Le password non corrispondono");
            }

            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(uploadImage.FileName);
                    var filePath = Path.Combine(Server.MapPath("~/Content/Images/Utenti"), fileName);
                    uploadImage.SaveAs(filePath);
                    utenti.FotoUtente = fileName; // Aggiorna il percorso della foto solo se una nuova è stata caricata
                }

                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    // Se è stata inserita una nuova password e confermata correttamente, criptala
                    utenti.Password = HashPassword(newPassword);
                }
                else
                {
                    // Se non è stata inserita una nuova password, non aggiornare il campo password
                    db.Entry(utenti).Property(x => x.Password).IsModified = false;
                }

                db.Entry(utenti).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MyProfile");
            }
            return View(utenti);
        }



        // GET: Utenti/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Utenti utenti = db.Utenti.Find(id);
            if (utenti == null)
            {
                return HttpNotFound();
            }
            return View(utenti);
        }

        // POST: Utenti/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Utenti utenti = db.Utenti.Find(id);
            db.Utenti.Remove(utenti);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        // questo metodo fa lhas della password
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        //Metodo per generare l'indirizzo del wallet per l'utente
        public ActionResult GenerateWallet()
        {
            // Ottenere l'ID dell'utente loggato
            string username = User.Identity.Name;
            var utente = db.Utenti.FirstOrDefault(u => u.Username == username);

            if (utente != null)
            {
                // Generare l'indirizzo del wallet
                string walletAddress = Guid.NewGuid().ToString();

                //Crea un nuovo wallet per l'utente
                var newWallet = new Wallets
                {
                    IdUtente = utente.IdUtente,
                    Address = walletAddress,
                    Saldo = 0 // Imposta il saldo del nuovo wallet a 0
                };

                // Aggiungere il nuovo wallet all'utente nel database
                db.Wallets.Add(newWallet);
                db.SaveChanges();

                // Output di debug per verificare che il wallet sia stato creato correttamente
                System.Diagnostics.Debug.WriteLine("Nuovo wallet creato per l'utente con ID: " + utente.IdUtente + ", indirizzo del wallet: " + walletAddress);

                // Reindirizza all'azione successiva o restituisci una vista opportuna
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Se l'utente non è valido, gestisci l'errore o reindirizza
                return RedirectToAction("Login", "Account");
            }
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
