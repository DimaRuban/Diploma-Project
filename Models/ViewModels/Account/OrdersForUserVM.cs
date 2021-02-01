﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoreTrainee.Models.ViewModels.Account
{
    public class OrdersForUserVM
    {
        public int OrderNumber { get; set; }

        public decimal Total { get; set; }

        public Dictionary<string, int> ProductAndQuantity { get; set; }

        public DateTime CreatedAt { get; set; }
        
    }
}