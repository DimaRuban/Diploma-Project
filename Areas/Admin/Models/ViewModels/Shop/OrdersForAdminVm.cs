﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoreTrainee.Areas.Admin.Models.ViewModels.Shop
{
    public class OrdersForAdminVm
    {
        public int OrderNumber { get; set; }

        public string UserName { get; set; }

        public decimal Total { get; set; }

        public Dictionary<string,int> ProductAndQuantity { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Phone { get; set; }

        public string Adress { get; set; }

        
    }
}