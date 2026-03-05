using System;
using System.Collections.Generic;

namespace Practica1.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrdersDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int PickUpPointsId { get; set; }
        public string PickUpPointAddress { get; set; }
        public int UsersId { get; set; }
        public string Code { get; set; }
        public string articl { get; set;}
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}