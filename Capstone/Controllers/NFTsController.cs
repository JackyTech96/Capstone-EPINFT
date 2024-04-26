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
    public class NFTsController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: NFTs
        public ActionResult Index()
        {
            return View();
        }

        // GET: NFTs Utente
        public ActionResult MyNFTs()
        {
            // Ottieni l'ID dell'utente loggato
            var utente = db.Utenti.FirstOrDefault(u => u.Username == User.Identity.Name);

            if (utente != null)
            {
                // Ottieni tutti i NFT dell'utente
                var nftsUtente = db.NFT.Where(n => n.IdProprietario == utente.IdUtente).ToList();
                return View(nftsUtente);
            }
            // Se l'utente non è trovato, reindirizza o mostra un messaggio di errore
            TempData["error"] = "Utente non trovato";
            return RedirectToAction("Index", "Home");
        }

        //GET: NFTs/InVendita
        [HttpGet]
        public ActionResult InVendita(int id)
        {
            var nft = db.NFT.FirstOrDefault(n => n.IdNFT == id);

            if (nft != null)
            {
                return View(nft);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult InVendita(int id, decimal nuovoPrezzo)
        {
            var nft = db.NFT.FirstOrDefault(n => n.IdNFT == id);

            if (nft != null)
            {
                nft.Prezzo = nuovoPrezzo;
                nft.IsDisponibile = true;
                db.SaveChanges();


            }
            TempData["success"] = "NFT in vendita correttamente";
            // Restituisce l'utente alla stessa pagina
            return RedirectToAction("MyNFTs", "NFTs", new { id = id });
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
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //Ottieni l'utente corrente
            var currentUser = db.Utenti.FirstOrDefault(u => u.Username == User.Identity.Name);

            //Ottieni la collezione associata all'ID
            var collezione = db.Collezioni.FirstOrDefault(c => c.IdCollezione == id);

            //Verifica se la collezione esiste e se l'utente corrente è proprietario della collezione
            if (collezione != null && collezione.IdUtente == currentUser.IdUtente)
            {
                // Assegna l'IdCollezione alla TempData anziché alla ViewBag
                TempData["IdCollezione"] = id;
                System.Diagnostics.Debug.WriteLine("Value of IdCollezione in TempData: " + id);
            }
            else
            {
                // Se l'utente corrente non è il creatore della collezione, reindirizza a una pagina di errore o a un'altra pagina appropriata
                TempData["error"] = "Non hai il permesso di creare NFT all'interno di questa collezione.";
                return RedirectToAction("Index", "Home"); // Reindirizza alla home page
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NomeNFT,Descrizione,Prezzo")] NFT nFT, HttpPostedFileBase file, string tipoFile)
        {
            if (ModelState.IsValid)
            {
                // Ottenere l'ID dell'utente loggato
                string username = User.Identity.Name;
                var utente = db.Utenti.FirstOrDefault(u => u.Username == username);

                if (utente != null)
                {
                    // Imposta l'ID dell'utente nell'NFT come proprietario
                    nFT.IdUtente = utente.IdUtente;

                    // Imposta l'ID del proprietario nell'NFT
                    nFT.IdProprietario = utente.IdUtente;

                    // Debug per visualizzare il valore di IdProprietario
                    System.Diagnostics.Debug.WriteLine("Valore di IdProprietario: " + nFT.IdProprietario);

                    // Recupera l'ID della collezione dalla TempData
                    int? idCollezione = TempData["IdCollezione"] as int?;

                    // Verifica se l'ID della collezione è presente nella TempData
                    if (idCollezione != null)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            // Determina la cartella in cui salvare il file
                            string folderName;
                            switch (tipoFile)
                            {
                                case "Immagine":
                                    folderName = "Images";
                                    break;
                                case "Video":
                                    folderName = "Videos";
                                    break;
                                case "Audio":
                                    folderName = "Audios";
                                    break;
                                default:
                                    ModelState.AddModelError("", "Tipo di file non valido.");
                                    return View(nFT); // Ritorna la vista con il modello per mostrare gli errori
                            }

                            //Crea il percorso di salvataggio
                            var uploadFolderPath = Server.MapPath($"/Content/{folderName}/NFT");

                            if (!Directory.Exists(uploadFolderPath))
                            {
                                Directory.CreateDirectory(uploadFolderPath);
                            }

                            var fileName = Path.GetFileName(file.FileName);
                            var filePath = Path.Combine(uploadFolderPath, fileName);
                            file.SaveAs(filePath);

                            // Imposta il NomeFile nell'NFT
                            nFT.NomeFile = fileName;

                            // Imposta il TipoFile nell'NFT
                            nFT.TipoFile = tipoFile;

                            // Assegna l'ID della collezione all'NFT
                            nFT.IdCollezione = idCollezione.Value;

                            // Imposta IsDisponibile a true
                            nFT.IsDisponibile = true;

                            // Imposta la data di creazione a DateTime.Now
                            nFT.DataCreazione = DateTime.Now;

                            // Salva l'NFT nel database
                            db.NFT.Add(nFT);
                            db.SaveChanges();

                            TempData["success"] = "NFT creato con successo!";

                            // Reindirizza all'azione "Details" del controller "Collezione" con l'ID della collezione
                            return RedirectToAction("Details", "Collezioni", new { id = idCollezione.Value });
                        }
                        else
                        {
                            // Se il file non è stato fornito, gestisci l'errore
                            ModelState.AddModelError("", "Devi fornire un file.");
                            return View(nFT); // Ritorna la vista con il modello per mostrare gli errori
                        }
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
                TempData["error"] = "Errore durante la creazione dell'NFT.";
                // Se lo stato del modello non è valido, ritorna la vista con il modello per mostrare gli errori
                ViewBag.IdUtente = new SelectList(db.Utenti, "IdUtente", "Email", nFT.IdUtente);
                return View(nFT);
            }
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
            //ViewBag.IdFileNFT = new SelectList(db.FileNFT, "IdFileNFT", "NomeFile", nFT.IdFileNFT);
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
            //ViewBag.IdFileNFT = new SelectList(db.FileNFT, "IdFileNFT", "NomeFile", nFT.IdFileNFT);
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
