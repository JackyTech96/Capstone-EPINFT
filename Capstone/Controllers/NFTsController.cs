﻿using System;
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
    public class NFTsController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: NFTs
        public ActionResult Index()
        {
            var nFT = db.NFT.Include(n => n.Collezioni).Include(n => n.FileNFT).Include(n => n.Utenti);
            return View(nFT.ToList());
        }

        // GET: NFTs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NFT nFT = db.NFT.Find(id);
            if (nFT == null)
            {
                return HttpNotFound();
            }
            return View(nFT);
        }

        // GET: NFTs/Create
        public ActionResult Create(int? idCollezione)
        {
            // Ottenere l'ID dell'utente loggato
            string username = User.Identity.Name;
            var utente = db.Utenti.FirstOrDefault(u => u.Username == username);

            // Verifica se l'utente è valido
            if (utente == null)
            {
                return RedirectToAction("Index", "Home"); // Reindirizza se l'utente non è valido
            }

            // Imposta l'ID utente dell'NFT con l'ID dell'utente loggato
            ViewBag.IdUtente = utente.IdUtente;

            // Verifica se è stato passato un ID di collezione valido
            if (idCollezione != null)
            {
                System.Diagnostics.Debug.WriteLine("ID di collezione valido: " + idCollezione);
                // Utilizza l'ID della collezione passato come parametro
                ViewBag.IdCollezione = idCollezione;
                ViewBag.IdFileNFT = new SelectList(db.FileNFT, "IdFileNFT", "NomeFile");
                return View();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ID di collezione non valido");
                // Se l'ID della collezione non è valido, reindirizza a una pagina di errore o fai un'altra gestione
                return RedirectToAction("Index", "Home");
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdFileNFT,NomeNFT,Descrizione,Prezzo")] NFT nFT)
        {
            if (ModelState.IsValid)
            {
                // Ottenere l'ID dell'utente loggato
                string username = User.Identity.Name;
                var utente = db.Utenti.FirstOrDefault(u => u.Username == username);

                if (utente != null)
                {
                    // Imposta l'ID dell'utente nell'NFT
                    nFT.IdUtente = utente.IdUtente;

                    // Recupera l'ID della collezione dalla TempData
                    int? idCollezione = TempData["IdCollezione"] as int?;

                    // Verifica se l'ID della collezione è presente nella TempData
                    if (idCollezione != null)
                    {
                        // Assegna l'ID della collezione all'NFT
                        nFT.IdCollezione = idCollezione.Value;

                        // Imposta IsDisponibile a true
                        nFT.IsDisponibile = true;

                        // Imposta la data di creazione a DateTime.Now
                        nFT.DataCreazione = DateTime.Now;

                        // Salva l'NFT nel database
                        db.NFT.Add(nFT);
                        db.SaveChanges();

                        // Reindirizza all'azione "Details" del controller "Collezione" con l'ID della collezione
                        return RedirectToAction("Details", "Collezioni", new { id = idCollezione.Value });
                    }
                    else
                    {
                        // Se l'ID della collezione non è presente nella TempData, gestisci l'errore e reindirizza
                        TempData["ErrorMessage"] = "ID della collezione non valido.";
                        System.Diagnostics.Debug.WriteLine("ID della collezione non presente nella TempData.");
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    // Se l'utente non è valido, reindirizza a una pagina di errore o fai un'altra gestione
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                // Se lo stato del modello non è valido, stampa i messaggi di errore
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine("Errore di validazione del modello: " + error.ErrorMessage);
                    }
                }
            }

            // Se il modello non è valido o se si verifica un altro errore, torna alla vista Create con il modello
            System.Diagnostics.Debug.WriteLine("Creazione NFT fallita.");
            ViewBag.IdFileNFT = new SelectList(db.FileNFT, "IdFileNFT", "NomeFile", nFT.IdFileNFT);
            ViewBag.IdUtente = new SelectList(db.Utenti, "IdUtente", "Email", nFT.IdUtente);
            return View(nFT);
        }




        // GET: NFTs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NFT nFT = db.NFT.Find(id);
            if (nFT == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdCollezione = new SelectList(db.Collezioni, "IdCollezione", "NomeCollezione", nFT.IdCollezione);
            ViewBag.IdFileNFT = new SelectList(db.FileNFT, "IdFileNFT", "NomeFile", nFT.IdFileNFT);
            ViewBag.IdUtente = new SelectList(db.Utenti, "IdUtente", "Email", nFT.IdUtente);
            return View(nFT);
        }

        // POST: NFTs/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdNFT,IdUtente,IdCollezione,IdFileNFT,NomeNFT,Descrizione,Prezzo,DataCreazione,IsDisponibile")] NFT nFT)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nFT).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdCollezione = new SelectList(db.Collezioni, "IdCollezione", "NomeCollezione", nFT.IdCollezione);
            ViewBag.IdFileNFT = new SelectList(db.FileNFT, "IdFileNFT", "NomeFile", nFT.IdFileNFT);
            ViewBag.IdUtente = new SelectList(db.Utenti, "IdUtente", "Email", nFT.IdUtente);
            return View(nFT);
        }

        // GET: NFTs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NFT nFT = db.NFT.Find(id);
            if (nFT == null)
            {
                return HttpNotFound();
            }
            return View(nFT);
        }

        // POST: NFTs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            NFT nFT = db.NFT.Find(id);
            db.NFT.Remove(nFT);
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
