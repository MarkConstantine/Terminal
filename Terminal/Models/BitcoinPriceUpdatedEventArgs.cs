using System;

namespace Terminal.Models
{
    public class BitcoinPriceUpdatedEventArgs : EventArgs
    {
        public decimal Price { get; set; }
        public decimal ChangePercent24Hr { get; set; }
    }
}
