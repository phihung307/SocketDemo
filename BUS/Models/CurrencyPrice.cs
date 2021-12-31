using System;
using System.Collections.Generic;

namespace BUS.Models
{
    public partial class CurrencyPrice
    {
        public int Id { get; set; }
        public string? Currency { get; set; }
        public decimal? Buy { get; set; }
        public decimal? Sell { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
