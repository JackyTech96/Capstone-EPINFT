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

        //GET: Carrello/Checkout
        public ActionResult Checkout()
        {
            var carrello = Session["Carrello"] as Carrello;

            if (carrello == null || carrello.Items.Count == 0)
            {
                TempData["error"] = "Il carrello è vuoto.";
                return RedirectToAction("Index");
            }

            try
            {
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

                            // Rimuovi l'NFT dal carrello
                            carrello.Items.Remove(item);
                        }
                        else
                        {
                            TempData["error"] = "Non puoi acquistare un NFT che possiedi già.";
                            return RedirectToAction("Index", "Home");
                        }
                    }

                    // Salva le modifiche nel database (acquisto)
                    db.SaveChanges();

                    // Aggiornamento del saldo del venditore e creazione della transazione di vendita
                    foreach (var item in carrello.Items.ToList())
                    {
                        // Get the NFTItem from the database
                        var nftItem = db.NFT.FirstOrDefault(n => n.IdNFT == item.NFTItem.IdNFT);

                        if (nftItem != null)
                        {
                            var venditore = db.Utenti.FirstOrDefault(u => u.IdUtente == nftItem.IdProprietario);
                            if (venditore != null)
                            {
                                var walletVenditore = db.Wallets.FirstOrDefault(w => w.IdUtente == venditore.IdUtente);
                                if (walletVenditore != null)
                                {
                                    walletVenditore.Saldo += item.NFTItem.Prezzo;

                                    // Creazione di una transazione per l'elemento corrente (vendita)
                                    var transazioneVendita = new Transazioni
                                    {
                                        IdNFT = nftItem.IdNFT,
                                        IdAcquirente = utente.IdUtente,
                                        IdVenditore = nftItem.IdProprietario,
                                        Importo = item.NFTItem.Prezzo,
                                        DataTransazione = DateTime.Now
                                    };

                                    // Salva la transazione di vendita nel database
                                    db.Transazioni.Add(transazioneVendita);

                                    // Creazione di un record di operazione sul wallet del venditore per registrare il deposito
                                    var operazioneVenditore = new Operazioni
                                    {
                                        IdUtente = venditore.IdUtente,
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
                            }
                            else
                            {
                                TempData["error"] = "Il venditore non è stato trovato.";
                                return RedirectToAction("Index", "Home");
                            }

                            // Rimuovi l'NFT dal carrello
                            carrello.Items.Remove(item);
                        }
                        else
                        {
                            TempData["error"] = "NFTItem non trovato nel database.";
                            return RedirectToAction("Index", "Home");
                        }
                    }

                    // Salva le modifiche nel database (vendita)
                    db.SaveChanges();

                    // Aggiorna la sessione
                    Session["Carrello"] = carrello;

                    TempData["success"] = "Transazione completata con successo.";
                }
                else
                {
                    TempData["error"] = "Utente non valido.";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "Si è verificato un errore durante il checkout del carrello.";
                // Puoi gestire l'eccezione qui, ad esempio, registrandola o visualizzando un messaggio di errore specifico
                // Log dell'eccezione: Logger.Error(ex, "Si è verificato un errore durante il checkout del carrello.");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}