using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;

namespace CoffeeSlotMachine.Core.Contracts
{
    public interface ICoinRepository
    {
        IEnumerable<Coin> GetCoinDepot();
        void AddCoin(int coinValue);
    }
}