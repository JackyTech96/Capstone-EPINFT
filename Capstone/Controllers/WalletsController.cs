using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Capstone.Models;

namespace Capstone.Controllers
{
    public class WalletsController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: Wallets
        public ActionResult Index()
        {
            var wallets = db.Wallets.Include(w => w.Utenti);
            return View(wallets.ToList());
        }

        // GET: Wallets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Wallets wallets = db.Wallets.Find(id);
            if (wallets == null)
            {
                return HttpNotFound();
            }
            return View(wallets);
        }

        // GET: Wallets/Create
        public ActionResult Create()
        {
            ViewBag.IdUtente = new SelectList(db.Utenti, "IdUtente", "Email");
            return View();
        }

        // POST: Wallets/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdWallet,IdUtente,Address,Saldo")] Wallets wallets)
        {
            if (ModelState.IsValid)
            {
                db.Wallets.Add(wallets);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdUtente = new SelectList(db.Utenti, "IdUtente", "Email", wallets.IdUtente);
            return View(wallets);
        }

        // GET: Wallets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Wallets wallets = db.Wallets.Find(id);
            if (wallets == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdUtente = new SelectList(db.Utenti, "IdUtente", "Email", wallets.IdUtente);
            return View(wallets);
        }

        // POST: Wallets/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdWallet,IdUtente,Address,Saldo")] Wallets wallets)
        {
            if (ModelState.IsValid)
            {
                db.Entry(wallets).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdUtente = new SelectList(db.Utenti, "IdUtente", "Email", wallets.IdUtente);
            return View(wallets);
        }

        // GET: Wallets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Wallets wallets = db.Wallets.Find(id);
            if (wallets == null)
            {
                return HttpNotFound();
            }
            return View(wallets);
        }

        // POST: Wallets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Wallets wallets = db.Wallets.Find(id);
            db.Wallets.Remove(wallets);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult MyWallet()
        {
            // Ottenere l'ID dell'utente loggato
            string username = User.Identity.Name;
            var utente = db.Utenti.FirstOrDefault(u => u.Username == username);

            if (utente != null)
            {
                // Ottenere il wallet dell'utente
                var wallet = utente.Wallets.FirstOrDefault();

                if (wallet != null)
                {
                    // Passa i dettagli del wallet alla vista per visualizzarli
                    return View(utente);
                }
            }

            // Se l'utente non ha un wallet, gestisci l'errore
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public ActionResult ProcessTransaction(decimal amount, string type)
        {
            // Ottenere l'ID dell'utente loggato
            string username = User.Identity.Name;
            var utente = db.Utenti.FirstOrDefault(u => u.Username == username);

            if (utente != null)
            {
                // Ottenere il wallet dell'utente
                var wallet = utente.Wallets.FirstOrDefault();

                if (wallet != null)
                {
                    // Effettuare operazioni di deposito o prelievo
                    if (type == "Deposito")
                    {
                        wallet.Saldo += amount;
                    }
                    else if (type == "Prelievo" && wallet.Saldo >= amount)
                    {
                        wallet.Saldo -= amount;
                    }
                    else if (type == "Acquisto" && wallet.Saldo >= amount)
                    {
                        wallet.Saldo -= amount;
                    }
                    else if (type == "Vendita")
                    {
                        wallet.Saldo += amount;
                    }
                    else
                    {
                        // Gestire il caso in cui il saldo non sia sufficiente per il prelievo
                        TempData["ErrorMessage"] = "Saldo non sufficiente per il prelievo.";
                        return RedirectToAction("MyWallet");
                    }

                    // Aggiungere un'operazione al wallet
                    var operazione = new Operazioni
                    {
                        IdUtente = utente.IdUtente,
                        IdWallet = wallet.IdWallet,
                        Tipo = type,
                        Importo = amount,
                        DataOperazione = DateTime.Now
                    };
                    db.Operazioni.Add(operazione);
                    db.SaveChanges();

                    // Imposta il messaggio di successo
                    TempData["SuccessMessage"] = "Operazione completata con successo.";

                    // Reindirizza all'azione MyWallet
                    return RedirectToAction("MyWallet");
                }
            }

            // Se l'utente o il wallet non sono validi, reindirizza all'azione di login
            return RedirectToAction("Login", "Account");
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
