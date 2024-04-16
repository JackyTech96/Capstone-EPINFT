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
    [Authorize]
    public class CollezioniController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: Collezioni
        public ActionResult Index()
        {
            var collezioni = db.Collezioni.Include(c => c.Categorie).Include(c => c.Utenti);
            return View(collezioni.ToList());
        }

        // GET: Collezioni/MyCollections
        public ActionResult MyCollections()
        {
            // Ottenere l'username dell'utente loggato
            string username = User.Identity.Name;

            // Trovare l'utente nel database utilizzando l'username
            var utente = db.Utenti.FirstOrDefault(u => u.Username == username);

            if (utente != null)
            {
                // Trova tutte le collezioni create dall'utente loggato
                var collezioniUtente = db.Collezioni.Where(c => c.IdUtente == utente.IdUtente).ToList();

                return View(collezioniUtente);
            }
            else
            {
                // Se l'utente non è trovato, gestisci l'errore appropriato
                TempData["error"] = "Utente non trovato.";
                return RedirectToAction("Index");
            }
        }

        // Collezioni per categoria
        public ActionResult GetCollectionsByCategory(string category)
        {
            // Se la categoria è "Tutti" o è nulla, restituisci tutte le collezioni
            if (string.IsNullOrEmpty(category) || category == "Tutti")
            {
                var collections = db.Collezioni.ToList();
                return PartialView("_CollectionsPartial", collections);
            }
            else
            {
                // Altrimenti, restituisci le collezioni corrispondenti alla categoria specificata
                var collections = db.Collezioni.Where(c => c.Categorie.NomeCategoria == category).ToList();
                return PartialView("_CollectionsPartial", collections);
            }
        }

        //Azione per recuperare il numero di collezioni presenti nel database
        [HttpGet]
        public ActionResult GetTotalCollections()
        {
            // Recupera il numero totale di collezioni dal database
            var totalCollections = db.Collezioni.Count();

            // Esempio di debug per verificare il numero totale di collezioni
            System.Diagnostics.Debug.WriteLine("Numero totale di collezioni: " + totalCollections);


            // Restituisci il numero totale di collezioni in formato JSON
            return Json(totalCollections, JsonRequestBehavior.AllowGet);
        }



        // GET: Collezioni/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var collezioneWithNFTs = db.Collezioni
                .Where(c => c.IdCollezione == id)
                .Include(c => c.NFT) // Carica anche i NFT associati alla collezione
                .FirstOrDefault();

            if (collezioneWithNFTs == null)
            {
                return HttpNotFound();
            }

            //Verifica se l'utente corrente e' l'autore della collezione
            var currentUser = db.Utenti.FirstOrDefault(u => u.Username == User.Identity.Name);
            var isCreator = collezioneWithNFTs.IdUtente == currentUser.IdUtente;

            //Passa l'informazione se l'utente corrente e' l'autore della collezione
            ViewBag.IsCreator = isCreator;

            return View(collezioneWithNFTs);
        }


        // GET: Collezioni/Create
        public ActionResult Create()
        {
            ViewBag.IdCategoria = new SelectList(db.Categorie, "IdCategoria", "NomeCategoria");
            ViewBag.IdUtente = new SelectList(db.Utenti, "IdUtente", "Email");
            return View();
        }

        // POST: Collezioni/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdCategoria,NomeCollezione,Descrizione, FotoCollezione")] Collezioni collezioni, HttpPostedFileBase uploadedFile, string RoyaltiesPercentage)
        {
            if (ModelState.IsValid)
            {
                //Ottenere l'IdUtente dell'utente loggato
                string username = User.Identity.Name;
                var utente = db.Utenti.FirstOrDefault(u => u.Username == username);

                //Verifica se l'utente è valido
                if (utente != null)
                {
                    //Imposta l'IdUtente della collezione
                    collezioni.IdUtente = utente.IdUtente;
                    //Imposta la data di creazione della collezione
                    collezioni.DataCreazione = DateTime.Now;
                    //Imposta IsArtista su true per l'utente loggato che crea la collezione
                    utente.IsArtista = true;

                    //Verifica se l'utente ha caricato un file
                    if (uploadedFile != null && uploadedFile.ContentLength > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("File caricato");
                        //Salva il file sul server
                        var fileName = Path.GetFileName(uploadedFile.FileName);
                        var filePath = Path.Combine(Server.MapPath("~/Content/Images/Collezioni"), fileName);
                        uploadedFile.SaveAs(filePath);

                        // Imposta il nome del file nella collezione
                        collezioni.FotoCollezione = fileName;
                    }

                    //Converta la Royalties in un formato decimal
                    if (!string.IsNullOrEmpty(RoyaltiesPercentage))
                    {
                        //Rimuovi il simbolo "%" se presente
                        RoyaltiesPercentage = RoyaltiesPercentage.Trim('%');

                        //Converte in decimal
                        if (decimal.TryParse(RoyaltiesPercentage, out decimal royalties))
                        {
                            //Assicurati che la percentuale di royalties sia compresa tra 0 e 100
                            if (royalties >= 0 && royalties <= 100)
                            {
                                //Converti la percentuale in formato decimal
                                collezioni.Royalties = royalties / 100;
                            }
                            else
                            {
                                //Gestisci errore: percentuale non valida
                                ModelState.AddModelError("RoyaltiesPercentage", "La percentuale di royalties deve essere compresa tra 0 e 100");
                                ViewBag.IdCategoria = new SelectList(db.Categorie, "IdCategoria", "NomeCategoria", collezioni.IdCategoria);
                                return View(collezioni);
                            }
                        }
                        else
                        {
                            // Gestisci errore: formato percentuale non valido
                            ModelState.AddModelError("RoyaltiesPercentage", "Formato percentuale non valido.");
                            ViewBag.IdCategoria = new SelectList(db.Categorie, "IdCategoria", "NomeCategoria", collezioni.IdCategoria);
                            return View(collezioni);
                        }
                    }
                    else
                    {
                        // Gestisci errore: campo Royalties vuoto
                        collezioni.Royalties = 0;
                    }

                    // Aggiunge la collezione al database
                    db.Collezioni.Add(collezioni);
                    db.SaveChanges();
                    TempData["success"] = "La collezione è stata creata con successo!";
                    return RedirectToAction("Index");
                }
            }

            System.Diagnostics.Debug.WriteLine("Modello non valido");
            // Se il modello non è valido, restituisce la vista
            ViewBag.IdCategoria = new SelectList(db.Categorie, "IdCategoria", "NomeCategoria", collezioni.IdCategoria);
            ViewBag.IdUtente = new SelectList(db.Utenti, "IdUtente", "Email", collezioni.IdUtente);
            TempData["error"] = "Si è verificato un errore durante la creazione della collezione.";
            return View(collezioni);
        }

        // GET: Collezioni/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Collezioni collezioni = db.Collezioni.Find(id);
            if (collezioni == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdCategoria = new SelectList(db.Categorie, "IdCategoria", "NomeCategoria", collezioni.IdCategoria);
            ViewBag.IdUtente = new SelectList(db.Utenti, "IdUtente", "Email", collezioni.IdUtente);
            return View(collezioni);
        }

        // POST: Collezioni/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdCollezione,IdUtente,IdCategoria,NomeCollezione,Descrizione,DataCreazione,Royalties,FotoCollezione")] Collezioni collezioni)
        {
            if (ModelState.IsValid)
            {
                db.Entry(collezioni).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdCategoria = new SelectList(db.Categorie, "IdCategoria", "NomeCategoria", collezioni.IdCategoria);
            ViewBag.IdUtente = new SelectList(db.Utenti, "IdUtente", "Email", collezioni.IdUtente);
            return View(collezioni);
        }

        // GET: Collezioni/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Collezioni collezioni = db.Collezioni.Find(id);
            if (collezioni == null)
            {
                return HttpNotFound();
            }
            return View(collezioni);
        }

        // POST: Collezioni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Collezioni collezioni = db.Collezioni.Find(id);
            db.Collezioni.Remove(collezioni);
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
