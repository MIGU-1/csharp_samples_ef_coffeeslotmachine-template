using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using CoffeeSlotMachine.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeSlotMachine.Core.Logic
{
    /// <summary>
    /// Verwaltet einen Bestellablauf. 
    /// </summary>
    public class OrderController : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICoinRepository _coinRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderController()
        {
            _dbContext = new ApplicationDbContext();

            _coinRepository = new CoinRepository(_dbContext);
            _orderRepository = new OrderRepository(_dbContext);
            _productRepository = new ProductRepository(_dbContext);
        }


        /// <summary>
        /// Gibt alle Produkte sortiert nach Namen zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Product> GetProducts() => _productRepository.GetAllProducts();
        /// <summary>
        /// Gibt den aktuellen Inhalt der Kasse, sortiert nach Münzwert absteigend zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Coin> GetCoinDepot() => _coinRepository.GetCoinDepot();


        /// <summary>
        /// Eine Bestellung wird für das Produkt angelegt.
        /// </summary>
        /// <param name="product"></param>
        public Order OrderCoffee(Product product)
        {
            if (GetProducts().Contains(product))
            {
                Order newOrder = new Order(product);
                _orderRepository.AddNewOrder(newOrder);
                return newOrder;
            }
            else
            {
                throw new ArgumentException("Produkt nicht verfügbar");
            }
        }
        /// <summary>
        /// Münze einwerfen. 
        /// Wurde zumindest der Produktpreis eingeworfen, Münzen in Depot übernehmen
        /// und für Order Retourgeld festlegen. Bestellug abschließen.
        /// </summary>
        /// <returns>true, wenn die Bestellung abgeschlossen ist</returns>
        public bool InsertCoin(Order order, int coinValue)
        {
            _coinRepository.AddCoin(coinValue);
            bool isFinsihed = order.InsertCoin(coinValue);

            if (isFinsihed)
            {
                order.FinishPayment(_dbContext.Coins.ToArray());
                _orderRepository.UpdateOrder(order);
                _dbContext.SaveChanges();
            }

            return isFinsihed;
        }


        /// <summary>
        /// Gibt den Inhalt des Münzdepots als String zurück
        /// </summary>
        /// <returns></returns>
        public string GetCoinDepotString()
        {
            StringBuilder sb = new StringBuilder();
            Coin[] coins = _coinRepository.GetCoinDepot()
                                          .OrderByDescending(c => c.CoinValue)
                                          .ToArray();

            for (int i = 0; i < coins.Length; i++)
            {
                sb.Append(coins[i].ToString());

                if (i + 1 < coins.Length)
                    sb.Append(" + ");
            }

            return sb.ToString();
        }
        /// <summary>
        /// Liefert alle Orders inkl. der Produkte zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Order> GetAllOrdersWithProduct()
        {
            return _orderRepository.GetAllWithProduct();
        }
        /// <summary>
        /// IDisposable:
        ///
        /// - Zusammenräumen (zB. des ApplicationDbContext).
        /// </summary>
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
