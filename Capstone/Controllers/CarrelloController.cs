using Capstone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Capstone.Controllers
{
    public class CarrelloController : Controller
    {
        private ModelDbContext db = new ModelDbContext();
        // GET: Carrello
        public ActionResult Index()
        {
            //Ottieni il carrello dalla sessione
            var carrello = Session["Carrello"] as Carrello;
            return View(carrello);
        }

        //Aggiungi un elemento al carrello
        public ActionResult AggiungiAlCarrello(int idNFT, int quantita)
        {
            //Ottieni l'NFT dal database
            NFT NFTSelezionato = db.NFT.Find(idNFT);

            //Verifica se l'NFT esiste nel database
            if (NFTSelezionato != null)
            {
                //Ottieni il carrello dalla sessione
                Carrello carrello = Session["Carrello"] as Carrello ?? new Carrello();

                //Controlla se l'NFT esiste già nel carrello
                CarrelloItem ItemEsistente = carrello.Items.FirstOrDefault(x => x.NFTItem.IdNFT == idNFT);

                if (ItemEsistente != null)
                {
                    //Aggiorna la quantità
                    ItemEsistente.Quantita += quantita;
                }
                else
                {
                    //Aggiungi l'NFT al carrello
                    carrello.Items.Add(new CarrelloItem { NFTItem = NFTSelezionato, Quantita = quantita });
                }

                //Aggiorna la sessione
                Session["Carrello"] = carrello;
            }
            else
            {
                // Gestisci il caso in cui l'NFT non esiste nel database (ad esempio, visualizza un messaggio di errore)
                TempData["Errore"] = "NFT non trovato nel database.";
            }

            return RedirectToAction("Index");
        }

        //Elimina un elemento dal carrello
        public ActionResult RimuoviDalCarrello(int idNFT)
        {
            //Ottieni il carrello dalla sessione
            Carrello carrello = Session["Carrello"] as Carrello;
            if (carrello != null)
            {
                //Controlla se l'NFT esiste nel carrello
                var itemDaRimuovere = carrello.Items.FirstOrDefault(x => x.NFTItem.IdNFT == idNFT);

                if (itemDaRimuovere != null)
                {
                    //Rimuovi l'NFT dal carrello
                    carrello.Items.Remove(itemDaRimuovere);
                    //Aggiorna la sessione
                    Session["Carrello"] = carrello;
                }
                else
                {
                    // Gestisci il caso in cui l'NFT non esiste nel carrello (ad esempio, visualizza un messaggio di errore)
                    TempData["Errore"] = "NFT non presente nel carrello.";
                }
            }
            else
            {
                // Gestisci il caso in cui il carrello non esiste (ad esempio, visualizza un messaggio di errore)
                TempData["Errore"] = "Il Carrello è vuoto.";
            }
            return RedirectToAction("Index");
        }
    }
}