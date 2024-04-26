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
            // Ottieni il carrello dalla sessione o crea un nuovo carrello vuoto se non è presente
            var carrello = Session["Carrello"] as Carrello ?? new Carrello();
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
                    //Se l'articolo è già presente nel carrello, non aggiornare la quantità
                    TempData["error"] = "L'articolo è già presente nel carrello.";
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
                TempData["error"] = "NFT non trovato nel database.";
            }

            TempData["success"] = "L'NFT è stato aggiunto al carrello.";

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
                    TempData["error"] = "NFT non presente nel carrello.";
                }
            }
            else
            {
                // Gestisci il caso in cui il carrello non esiste (ad esempio, visualizza un messaggio di errore)
                TempData["error"] = "Il Carrello è vuoto.";
            }

            TempData["success"] = "L'NFT è stato rimosso dal carrello.";
            return RedirectToAction("Index");
        }

        public ActionResult Checkout()
        {
            var carrello = Session["Carrello"] as Carrello;

            if (carrello == null || carrello.Items.Count == 0)
            {
                TempData["error"] = "Il carrello è vuoto.";
                return RedirectToAction("Index");
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Calcolo guadagno dell'Admin
                    decimal guadagnoAdmin = CalcolaGuadagnoAdmin(carrello);

                    // Aggiornamento del saldo del wallet dell'admin
                    AggiornaSaldoAdmin(guadagnoAdmin);

                    var utente = db.Utenti.FirstOrDefault(u => u.Username == User.Identity.Name);

                    if (utente != null)
                    {
                        foreach (var item in carrello.Items.ToList())
                        {
                            // Controllo se l'utente non è il proprietario dell'NFT
                            if (item.NFTItem.IdProprietario != utente.IdUtente)
                            {
                                // Get the NFTItem from the database
                                var nftItem = db.NFT.FirstOrDefault(n => n.IdNFT == item.NFTItem.IdNFT);

                                if (nftItem != null)
                                {
                                    // Update the IdProprietario and IsDisponibile fields
                                    nftItem.IdProprietario = utente.IdUtente;
                                    nftItem.IsDisponibile = false;

                                    // Salva le modifiche nell'entità NFTItem nel database
                                    db.SaveChanges();
                                }
                                else
                                {
                                    TempData["error"] = "NFTItem non trovato nel database.";
                                    return RedirectToAction("Index", "Home");
                                }

                                // Calcolo dell'importo delle royalties
                                decimal royalties = item.NFTItem.Prezzo * nftItem.Collezioni.Royalties.Value;

                                // Aggiorna il saldo del creatore della collezione
                                var walletCreatore = db.Wallets.FirstOrDefault(w => w.IdUtente == nftItem.Collezioni.IdUtente);
                                if (walletCreatore != null)
                                {
                                    walletCreatore.Saldo += royalties;

                                    // Aggiungi un record di operazione sul wallet del creatore per registrare le royalties
                                    var operazioneRoyalties = new Operazioni
                                    {
                                        IdUtente = utente.IdUtente,
                                        IdWallet = walletCreatore.IdWallet,
                                        Tipo = "Royalties",
                                        Importo = royalties,
                                        DataOperazione = DateTime.Now
                                    };
                                    db.Operazioni.Add(operazioneRoyalties);
                                }
                                else
                                {
                                    TempData["error"] = "Il wallet del creatore della collezione non è stato trovato.";
                                    return RedirectToAction("Index", "Home");
                                }

                                // Creazione di una transazione per l'elemento corrente (acquisto)
                                var transazioneAcquisto = new Transazioni
                                {
                                    IdNFT = nftItem.IdNFT,
                                    IdAcquirente = utente.IdUtente,
                                    IdVenditore = item.NFTItem.IdProprietario,
                                    Importo = item.NFTItem.Prezzo,
                                    DataTransazione = DateTime.Now
                                };

                                // Salva la transazione di acquisto nel database
                                db.Transazioni.Add(transazioneAcquisto);

                                // Recupera il wallet dell'acquirente e aggiorna il saldo
                                var walletAcquirente = db.Wallets.FirstOrDefault(w => w.IdUtente == utente.IdUtente);
                                if (walletAcquirente != null)
                                {
                                    walletAcquirente.Saldo -= item.NFTItem.Prezzo; // Sottrai il prezzo del NFT dal saldo del wallet

                                    // Crea un record di operazione sul wallet per registrare l'acquisto
                                    var operazioneAcquirente = new Operazioni
                                    {
                                        IdUtente = utente.IdUtente,
                                        IdWallet = walletAcquirente.IdWallet,
                                        Tipo = "Acquisto",
                                        Importo = item.NFTItem.Prezzo,
                                        DataOperazione = DateTime.Now
                                    };
                                    db.Operazioni.Add(operazioneAcquirente);
                                }
                                else
                                {
                                    TempData["error"] = "Il wallet dell'acquirente non è stato trovato.";
                                    return RedirectToAction("Index", "Home");
                                }

                                // Recupera il wallet del venditore e aggiorna il saldo
                                var walletVenditore = db.Wallets.FirstOrDefault(w => w.IdUtente == item.NFTItem.IdProprietario);
                                if (walletVenditore != null)
                                {
                                    walletVenditore.Saldo += item.NFTItem.Prezzo; // Aggiungi il prezzo del NFT al saldo del wallet del venditore

                                    // Crea un record di operazione sul wallet per registrare la vendita
                                    var operazioneVenditore = new Operazioni
                                    {
                                        IdUtente = item.NFTItem.IdProprietario,
                                        IdWallet = walletVenditore.IdWallet,
                                        Tipo = "Vendita",
                                        Importo = item.NFTItem.Prezzo,
                                        DataOperazione = DateTime.Now
                                    };
                                    db.Operazioni.Add(operazioneVenditore);
                                }
                                else
                                {
                                    TempData["error"] = "Il wallet del venditore non è stato trovato.";
                                    return RedirectToAction("Index", "Home");
                                }

                                // Rimuovi l'NFT dal carrello
                                carrello.Items.Remove(item);
                            }
                            else
                            {
                                TempData["error"] = "Non puoi acquistare un NFT che possiedi già.";
                                return RedirectToAction("Index", "Home");
                            }
                        }

                        db.SaveChanges();
                        transaction.Commit();
                    }
                    else
                    {
                        TempData["error"] = "Utente non trovato.";
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["error"] = "Si è verificato un errore durante il checkout: " + ex.Message;
                    return RedirectToAction("Index", "Home");
                }
            }

            TempData["success"] = "Checkout completato con successo!";
            return RedirectToAction("Index", "Home");
        }

        // Metodo per calcolare il guadagno dell'admin basato sul carrello
        private decimal CalcolaGuadagnoAdmin(Carrello carrello)
        {
            decimal guadagnoTotale = 0;

            // Calcola il guadagno totale dell'admin sommando le percentuali di guadagno di ogni transazione
            foreach (var item in carrello.Items)
            {
                decimal guadagnoTransazione = item.NFTItem.Prezzo * 0.05m;
                guadagnoTotale += guadagnoTransazione;
            }

            return guadagnoTotale;
        }

        // Metodo per aggiornare il saldo del wallet dell'admin
        private void AggiornaSaldoAdmin(decimal guadagnoAdmin)
        {
            var admin = db.Utenti.FirstOrDefault(u => u.Ruolo == "admin");

            if (admin != null)
            {
                admin.Wallets.FirstOrDefault().Saldo += guadagnoAdmin;
            }
            else
            {
                // Gestisci il caso in cui l'utente admin non sia presente nel database
                throw new Exception("Utente admin non trovato nel database.");
            }
        }
    }
}