using System;
using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CoffeeSlotMachine.Persistence
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddNewOrder(Order newOrder) => _dbContext.Orders.Add(newOrder);
        public void UpdateOrder(Order order) => _dbContext.Orders.Update(order);
        public IEnumerable<Order> GetAllWithProduct() => _dbContext.Orders;
    }
}
