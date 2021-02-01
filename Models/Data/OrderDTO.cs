using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace StoreTrainee.Models.Data
{   
    [Table("stOrders")]
    public class OrderDTO
    {   
        [Key]
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public DateTime CreateAt { get; set; }


        [ForeignKey("UserId")]
        public virtual UserDTO Users { get; set; }
    }
}