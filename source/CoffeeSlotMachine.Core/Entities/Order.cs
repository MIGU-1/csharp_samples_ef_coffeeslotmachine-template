using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CoffeeSlotMachine.Core.Entities
{
    /// <summary>
    /// Bestellung verwaltet das bestellte Produkt, die eingeworfenen Münzen und
    /// die Münzen die zurückgegeben werden.
    /// </summary>
    public class Order : EntityObject
    {
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        /// <summary>
        /// Datum und Uhrzeit der Bestellung
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Werte der eingeworfenen Münzen als Text. Die einzelnen 
        /// Münzwerte sind durch ; getrennt (z.B. "10;20;10;50")
        /// </summary>
        public String ThrownInCoinValues { get; set; }

        /// <summary>
        /// Zurückgegebene Münzwerte mit ; getrennt
        /// </summary>
        public String ReturnCoinValues { get; set; }

        /// <summary>
        /// Summe der eingeworfenen Cents.
        /// </summary>
        public int ThrownInCents { get; set; }

        /// <summary>
        /// Summe der Cents die zurückgegeben werden
        /// </summary>
        public int ReturnCents { get; set; }

        /// <summary>
        /// Kann der Automat mangels Kleingeld nicht
        /// mehr herausgeben, wird der Rest als Spende verbucht
        /// </summary>
        public int DonationCents { get; set; }


        public Order()
        {
            Time = DateTime.Now;
            ThrownInCoinValues = "";
            ReturnCoinValues = "";
        }
        public Order(Product product)
            : this()
        {
            Product = product;
        }


        /// <summary>
        /// Münze wird eingenommen.
        /// </summary>
        /// <param name="coinValue"></param>
        /// <returns>isFinished ist true, wenn der Produktpreis zumindest erreicht wurde</returns>
        public bool InsertCoin(int coinValue)
        {
            ThrownInCoinValues = $"{ThrownInCoinValues}{coinValue}";
            ThrownInCents += coinValue;
            int diff = ThrownInCents - Product.PriceInCents;

            if (diff >= 0)
            {
                ReturnCents = diff;
                return true;
            }
            else
            {
                ThrownInCoinValues = $"{ThrownInCoinValues};";
                return false;
            }
        }
        /// <summary>
        /// Übernahme des Einwurfs in das Münzdepot.
        /// Rückgabe des Retourgeldes aus der Kasse. Staffelung des Retourgeldes
        /// hängt vom Inhalt der Kasse ab.
        /// </summary>
        /// <param name="coins">Aktueller Zustand des Münzdepots</param>
        public IEnumerable<Coin> FinishPayment(IEnumerable<Coin> coins)
        {
            int retCents = ReturnCents;

            Coin[] coinsArray = coins
                                .OrderByDescending(c => c.CoinValue)
                                .ToArray();

            if (retCents > 0)
            {
                for (int i = 0; i < coinsArray.Length; i++)
                {
                    while (coinsArray[i].Amount > 0 && retCents - coinsArray[i].CoinValue >= 0)
                    {
                        ReturnCoinValues = $"{ReturnCoinValues}{coinsArray[i].CoinValue};";
                        retCents -= coinsArray[i].CoinValue;
                        coinsArray[i].Amount--;
                    }
                }
            }

            if (ReturnCoinValues.Length > 0)
                ReturnCoinValues = ReturnCoinValues.Remove(ReturnCoinValues.Length - 1);

            DonationCents = retCents;

            return coinsArray.OrderBy(c => c.CoinValue);
        }
    }
}
