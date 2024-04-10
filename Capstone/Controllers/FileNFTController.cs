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
    public class FileNFTController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: FileNFT
        public ActionResult Index()
        {
            return View(db.FileNFT.ToList());
        }

        // GET: FileNFT/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileNFT fileNFT = db.FileNFT.Find(id);
            if (fileNFT == null)
            {
                return HttpNotFound();
            }
            return View(fileNFT);
        }
        // GET: FileNFT/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Assegna l'IdCollezione alla TempData anziché alla ViewBag
            TempData["IdCollezione"] = id;
            System.Diagnostics.Debug.WriteLine("Value of IdCollezione in TempData: " + id);

            return View();
        }

        // POST: FileNFT/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HttpPostedFileBase file, string tipoFile)
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
                        return View();
                }

                // Crea il percorso di salvataggio
                var uploadFolderPath = Server.MapPath($"~/Content/{folderName}/NFT");
                if (!Directory.Exists(uploadFolderPath))
                {
                    Directory.CreateDirectory(uploadFolderPath);
                }

                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadFolderPath, fileName);
                file.SaveAs(filePath);

                // Aggiungi il file al database
                var fileNFT = new FileNFT { NomeFile = fileName, TipoFile = tipoFile };
                db.FileNFT.Add(fileNFT);
                db.SaveChanges();

                // Recupera l'ID della collezione dalla TempData
                int? idCollezione = TempData["IdCollezione"] as int?;

                // Controlla se l'ID della collezione è valido
                if (idCollezione.HasValue)
                {
                    // Reindirizza all'azione successiva, passando l'ID della collezione come parametro
                    return RedirectToAction("Create", "NFTs", new { idCollezione = idCollezione.Value });
                }
                else
                {
                    // Se l'ID della collezione non è valido, gestisci l'errore
                    ModelState.AddModelError("", "ID della collezione non valido.");
                    return View();
                }
            }

            // Se il file non è stato fornito, gestisci l'errore e restituisci la vista
            ModelState.AddModelError("", "Devi fornire un file.");
            return View();
        }


        // GET: FileNFT/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileNFT fileNFT = db.FileNFT.Find(id);
            if (fileNFT == null)
            {
                return HttpNotFound();
            }
            return View(fileNFT);
        }

        // POST: FileNFT/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdFileNFT,NomeFile,TipoFile")] FileNFT fileNFT)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fileNFT).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fileNFT);
        }

        // GET: FileNFT/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileNFT fileNFT = db.FileNFT.Find(id);
            if (fileNFT == null)
            {
                return HttpNotFound();
            }
            return View(fileNFT);
        }

        // POST: FileNFT/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FileNFT fileNFT = db.FileNFT.Find(id);
            db.FileNFT.Remove(fileNFT);
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
