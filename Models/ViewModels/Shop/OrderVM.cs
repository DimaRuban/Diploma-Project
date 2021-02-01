using StoreTrainee.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoreTrainee.Models.ViewModels.Shop
{
    public class OrderVM
    {
        public OrderVM()
        {
        }

        public OrderVM(OrderDTO row)
        {
            OrderId =row.OrderId;
            UserId = row.UserId;
            CreateAt = row.CreateAt;
        }

        public int OrderId { get; set; }

        public int UserId { get; set; }

        public DateTime CreateAt { get; set; }
    }
}