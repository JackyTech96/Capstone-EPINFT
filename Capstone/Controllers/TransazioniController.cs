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
    public class TransazioniController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: Transazioni
        public ActionResult Index()
        {
            var transazioni = db.Transazioni.Include(t => t.NFT).Include(t => t.Utenti).Include(t => t.Utenti1);
            return View(transazioni.ToList());
        }

        public ActionResult GetDailyTransactionsCount()
        {
            try
            {
                // Recupera la data corrente
                DateTime today = DateTime.Today;

                // Conta il numero di transazioni registrate per oggi
                int dailyTransactionsCount = db.Transazioni.Count(t => t.DataTransazione.Year == today.Year && t.DataTransazione.Month == today.Month && t.DataTransazione.Day == today.Day);

                // Restituisci il numero di transazioni del giorno come JSON
                return Json(dailyTransactionsCount, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Gestione degli errori
                System.Diagnostics.Debug.WriteLine("Errore durante il recupero del numero di transazioni del giorno: " + ex.Message);
                return Json(0, JsonRequestBehavior.AllowGet); // Restituisci 0 in caso di errore
            }
        }


        // GET: Transazioni/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transazioni transazioni = db.Transazioni.Find(id);
            if (transazioni == null)
            {
                return HttpNotFound();
            }
            return View(transazioni);
        }

        // GET: Transazioni/Create
        public ActionResult Create()
        {
            ViewBag.IdNFT = new SelectList(db.NFT, "IdNFT", "NomeFile");
            ViewBag.IdAcquirente = new SelectList(db.Utenti, "IdUtente", "Email");
            ViewBag.IdVenditore = new SelectList(db.Utenti, "IdUtente", "Email");
            return View();
        }

        // POST: Transazioni/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdTransazione,IdNFT,IdAcquirente,IdVenditore,Importo,DataTransazione")] Transazioni transazioni)
        {
            if (ModelState.IsValid)
            {
                db.Transazioni.Add(transazioni);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdNFT = new SelectList(db.NFT, "IdNFT", "NomeFile", transazioni.IdNFT);
            ViewBag.IdAcquirente = new SelectList(db.Utenti, "IdUtente", "Email", transazioni.IdAcquirente);
            ViewBag.IdVenditore = new SelectList(db.Utenti, "IdUtente", "Email", transazioni.IdVenditore);
            return View(transazioni);
        }

        // GET: Transazioni/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transazioni transazioni = db.Transazioni.Find(id);
            if (transazioni == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdNFT = new SelectList(db.NFT, "IdNFT", "NomeFile", transazioni.IdNFT);
            ViewBag.IdAcquirente = new SelectList(db.Utenti, "IdUtente", "Email", transazioni.IdAcquirente);
            ViewBag.IdVenditore = new SelectList(db.Utenti, "IdUtente", "Email", transazioni.IdVenditore);
            return View(transazioni);
        }

        // POST: Transazioni/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdTransazione,IdNFT,IdAcquirente,IdVenditore,Importo,DataTransazione")] Transazioni transazioni)
        {
            if (ModelState.IsValid)
            {
                db.Entry(transazioni).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdNFT = new SelectList(db.NFT, "IdNFT", "NomeFile", transazioni.IdNFT);
            ViewBag.IdAcquirente = new SelectList(db.Utenti, "IdUtente", "Email", transazioni.IdAcquirente);
            ViewBag.IdVenditore = new SelectList(db.Utenti, "IdUtente", "Email", transazioni.IdVenditore);
            return View(transazioni);
        }

        // GET: Transazioni/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transazioni transazioni = db.Transazioni.Find(id);
            if (transazioni == null)
            {
                return HttpNotFound();
            }
            return View(transazioni);
        }

        // POST: Transazioni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transazioni transazioni = db.Transazioni.Find(id);
            db.Transazioni.Remove(transazioni);
            db.SaveChanges();
            return RedirectToAction("Index");
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
