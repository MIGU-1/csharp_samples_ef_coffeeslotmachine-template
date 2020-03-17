using CoffeeSlotMachine.Core.Logic;
using CoffeeSlotMachine.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CoffeeSlotMachine.ControllerTest
{
    [TestClass]
    public class ControllerTest
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            using (ApplicationDbContext applicationDbContext = new ApplicationDbContext())
            {
                applicationDbContext.Database.EnsureDeleted();
                applicationDbContext.Database.Migrate();
            }
        }


        [TestMethod]
        public void T01_GetCoinDepot_CoinTypesCount_ShouldReturn6Types_3perType_SumIs1155Cents()
        {
            using (OrderController controller = new OrderController())
            {
                var depot = controller.GetCoinDepot().ToArray();
                Assert.AreEqual(6, depot.Count(), "Sechs M�nzarten im Depot");
                foreach (var coin in depot)
                {
                    Assert.AreEqual(3, coin.Amount, "Je M�nzart sind drei St�ck im Depot");
                }

                int sumOfCents = depot.Sum(coin => coin.CoinValue * coin.Amount);
                Assert.AreEqual(1155, sumOfCents, "Beim Start sind 1155 Cents im Depot");
            }
        }

        [TestMethod]
        public void T02_GetProducts_9Products_FromCappuccinoToRistretto()
        {
            using (OrderController statisticsController = new OrderController())
            {
                var products = statisticsController.GetProducts().ToArray();
                Assert.AreEqual(9, products.Length, "Neun Produkte wurden erzeugt");
                Assert.AreEqual("Cappuccino", products[0].Name);
                Assert.AreEqual("Ristretto", products[8].Name);
            }
        }

        [TestMethod]
        public void T03_BuyOneCoffee_OneCoinIsEnough_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Cappuccino");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "100 Cent gen�gen");
                Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("20;10;5", order.ReturnCoinValues);

                // Depot �berpr�fen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1220, sumOfCents, "Beim Start sind 1155 Cents + 65 Cents f�r Cappuccino");
                Assert.AreEqual("3*200 + 4*100 + 3*50 + 2*20 + 2*10 + 2*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(100, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual("Cappuccino", orders[0].Product.Name, "Produktname Cappuccino");
            }
        }

        [TestMethod]
        public void T04_BuyOneCoffee_ExactThrowInOneCoin_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Latte");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 50);
                Assert.AreEqual(true, isFinished, "50 Cent gen�gen");
                Assert.AreEqual(50, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(50 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("", order.ReturnCoinValues);

                // Depot �berpr�fen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1205, sumOfCents, "Beim Start sind 1155 Cents + 50 Cents f�r Latte");
                Assert.AreEqual("3*200 + 3*100 + 4*50 + 3*20 + 3*10 + 3*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(50, orders[0].ThrownInCents, "50 Cents wurden eingeworfen");
                Assert.AreEqual("Latte", orders[0].Product.Name, "Produktname Latte");
            }
        }

        [TestMethod]
        public void T05_BuyOneCoffee_MoreCoins_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Doppio");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 10);
                Assert.AreEqual(false, isFinished, "50 Cent gen�gen");
                isFinished = controller.InsertCoin(order, 20);
                Assert.AreEqual(false, isFinished, "50 Cent gen�gen");
                isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "50 Cent gen�gen");
                Assert.AreEqual(130, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(130 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("50", order.ReturnCoinValues);

                // Depot �berpr�fen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1235, sumOfCents, "Beim Start sind 1155 Cents + 80 Cents f�r Doppio");
                Assert.AreEqual("3*200 + 4*100 + 2*50 + 4*20 + 4*10 + 3*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(130, orders[0].ThrownInCents, "50 Cents wurden eingeworfen");
                Assert.AreEqual("Doppio", orders[0].Product.Name, "Produktname Doppio");
            }
        }

        [TestMethod()]
        public void T06_BuyMoreCoffees_OneCoins_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product1 = products.Single(p => p.Name == "Latte");
                var order1 = controller.OrderCoffee(product1);
                bool isFinished = controller.InsertCoin(order1, 50);
                Assert.AreEqual(true, isFinished, "50 Cent gen�gen");
                Assert.AreEqual(50, order1.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(50 - product1.PriceInCents, order1.ReturnCents);
                Assert.AreEqual(0, order1.DonationCents);
                Assert.AreEqual("", order1.ReturnCoinValues);

                var product2 = products.Single(p => p.Name == "Espresso");
                var order2 = controller.OrderCoffee(product2);
                isFinished = controller.InsertCoin(order2, 50);
                Assert.AreEqual(true, isFinished, "50 Cent gen�gen");
                Assert.AreEqual(50, order2.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(50 - product2.PriceInCents, order2.ReturnCents);
                Assert.AreEqual(0, order2.DonationCents);
                Assert.AreEqual("", order2.ReturnCoinValues);

                // Depot �berpr�fen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1255, sumOfCents, "Beim Start sind 1155 Cents + 2*50 Cents f�r die Bestellungen");
                Assert.AreEqual("3*200 + 3*100 + 5*50 + 3*20 + 3*10 + 3*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(2, orders.Length, "Es sind zwei Bestellungen");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(50, orders[0].ThrownInCents, "50 Cents wurden eingeworfen");
                Assert.AreEqual("Latte", orders[0].Product.Name, "Produktname Latte");
                Assert.AreEqual(0, orders[1].DonationCents, "Keine Spende");
                Assert.AreEqual(50, orders[1].ThrownInCents, "50 Cents wurden eingeworfen");
                Assert.AreEqual("Espresso", orders[1].Product.Name, "Produktname Latte");
            }
        }

        [TestMethod()]
        public void T07_BuyMoreCoffees_UntilDonation_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();

                var product1 = products.Single(p => p.Name == "Machiato");
                var order1 = controller.OrderCoffee(product1);
                bool isFinished = controller.InsertCoin(order1, 100);

                var product2 = products.Single(p => p.Name == "Machiato");
                var order2 = controller.OrderCoffee(product2);
                isFinished = controller.InsertCoin(order2, 100);

                var product3 = products.Single(p => p.Name == "Machiato");
                var order3 = controller.OrderCoffee(product3);
                isFinished = controller.InsertCoin(order3, 100);

                var product4 = products.Single(p => p.Name == "Machiato");
                var order4 = controller.OrderCoffee(product4);
                isFinished = controller.InsertCoin(order4, 100);
                Assert.AreEqual(true, isFinished, "100 Cent gen�gen");
                Assert.AreEqual(100, order4.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product4.PriceInCents, order4.ReturnCents);
                Assert.AreEqual(5, order4.DonationCents);
                Assert.AreEqual("10;10", order4.ReturnCoinValues);

                // Depot �berpr�fen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1460, sumOfCents, "Beim Start sind 1155 Cents + 3*75 Cents f�r Bestellungen und 1*80 weil kein R�ckgeld");
                Assert.AreEqual("3*200 + 7*100 + 3*50 + 0*20 + 1*10 + 0*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(4, orders.Length, "Es sind vier Bestellungen");
                Assert.AreEqual(5, orders[3].DonationCents, "Spende von 5 Cent");
                Assert.AreEqual(100, orders[3].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual("Machiato", orders[3].Product.Name, "Produktname Machiato");
            }
        }

    }
}
