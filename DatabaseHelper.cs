using System;
using System.Collections.Generic;
using Npgsql;

namespace Practica1.Models
{
    public class DatabaseHelper
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=1;Database=Practica1";

        public List<Product> GetProducts()
        {
            var products = new List<Product>();
            
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                
                string query = @"
                    SELECT p.id, p.articl, p.title_id, t.name as title_name,
                        p.unit_of_measurement, p.price, p.suplitiers_id, s.name as supplier_name,
                        p.manufactures_id, m.name as manufacture_name,
                        p.categories_products, c.name as category_name,
                        p.discount, p.qty, p.description, p.image
                    FROM products p
                    LEFT JOIN title t ON p.title_id = t.id
                    LEFT JOIN suplitiers s ON p.suplitiers_id = s.id
                    LEFT JOIN manufactures m ON p.manufactures_id = m.id
                    LEFT JOIN categories_products c ON p.categories_products = c.id;";
                
                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            Artcl = reader.GetString(1),
                            TitleId = reader.GetInt32(2),
                            TitleName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            UnitOfMeasurement = reader.GetString(4),
                            Price = reader.GetDecimal(5),
                            SuppliersId = reader.GetInt32(6),
                            SupplierName = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            ManufacturesId = reader.GetInt32(8),
                            ManufactureName = reader.IsDBNull(9) ? "" : reader.GetString(9),
                            CategoriesProducts = reader.GetInt32(10),
                            CategoryName = reader.IsDBNull(11) ? "" : reader.GetString(11),
                            Discount = reader.GetInt32(12),
                            Qty = reader.GetInt32(13),
                            Description = reader.IsDBNull(14) ? "" : reader.GetString(14),
                            Image = reader.IsDBNull(15) ? "" : reader.GetString(15)
                        });
                    }
                }
            }
            
            return products;
        }

        public void DeleteProduct(int productId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                
                string checkOrdersQuery = "SELECT COUNT(*) FROM orders_datalse WHERE products_id = @productId";
                using (var checkCmd = new NpgsqlCommand(checkOrdersQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@productId", productId);
                    long orderCount = (long)checkCmd.ExecuteScalar();
                    
                    if (orderCount > 0)
                    {
                        throw new Exception("Нельзя удалить товар, так как он присутствует в заказах");
                    }
                }
                
                string imageFileName = null;
                string getImageQuery = "SELECT image FROM products WHERE id = @id";
                using (var getImgCmd = new NpgsqlCommand(getImageQuery, conn))
                {
                    getImgCmd.Parameters.AddWithValue("@id", productId);
                    var result = getImgCmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        imageFileName = result.ToString();
                    }
                }
                
                string deleteQuery = "DELETE FROM products WHERE id = @id";
                using (var cmd = new NpgsqlCommand(deleteQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@id", productId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Order> GetOrders()
        {
            var orders = new List<Order>();
            
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                
                string query = @"
                    SELECT 
                        o.id, o.orders_date, o.delivery_date, o.pic_up_poins_id,
                        pp.mailing_address, pp.city, pp.street, pp.street_number,
                        o.users_id, o.code, o.status_id, s.name as status_name, p.articl
                    FROM orders o
                    LEFT JOIN pic_up_poins pp ON o.pic_up_poins_id = pp.id
                    LEFT JOIN status s ON o.status_id = s.id
					Left Join orders_datalse od ON od.orders_id = o.id
					Left Join products p ON od.products_id = p.id
                    ORDER BY o.orders_date DESC;";
                
                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string address = "";
                        
                        if (!reader.IsDBNull(4) && !string.IsNullOrEmpty(reader.GetString(4)))
                        {
                            address = reader.GetString(4);
                        }
                        else
                        {
                            string city = reader.IsDBNull(5) ? "" : reader.GetString(5);
                            string street = reader.IsDBNull(6) ? "" : reader.GetString(6);
                            string streetNumber = reader.IsDBNull(7) ? "" : reader.GetString(7);
                            
                            if (!string.IsNullOrEmpty(city))
                                address = city;
                            if (!string.IsNullOrEmpty(street))
                                address += (string.IsNullOrEmpty(address) ? "" : ", ") + street;
                            if (!string.IsNullOrEmpty(streetNumber))
                                address += (string.IsNullOrEmpty(address) ? "" : ", ") + streetNumber;
                        }

                        if (string.IsNullOrEmpty(address))
                            address = "—";

                        var order = new Order
                        {
                            Id = reader.GetInt32(0),
                            OrdersDate = reader.GetDateTime(1),
                            DeliveryDate = reader.GetDateTime(2),
                            PickUpPointsId = reader.GetInt32(3),
                            PickUpPointAddress = address,
                            UsersId = reader.GetInt32(8),
                            Code = reader.IsDBNull(9) ? "" : reader.GetString(9),
                            StatusId = reader.GetInt32(10),
                            StatusName = reader.IsDBNull(11) ? "" : reader.GetString(11),
                            articl = reader.GetString(12)
                        };
                        
                        orders.Add(order);
                    }
                }
            }
            
            return orders;
        }

        public List<OrderItem> GetOrderItems(int orderId)
        {
            var items = new List<OrderItem>();
            
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                
                string query = @"
                    SELECT 
                        od.id, od.orders_id, od.products_id, od.qty,
                        p.articl, p.price, t.name as product_name
                    FROM orders_datalse od
                    LEFT JOIN products p ON od.products_id = p.id
                    LEFT JOIN title t ON p.title_id = t.id
                    WHERE od.orders_id = @orderId;";
                
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new OrderItem
                            {
                                Id = reader.GetInt32(0),
                                OrderId = reader.GetInt32(1),
                                ProductId = reader.GetInt32(2),
                                Quantity = reader.GetInt32(3),
                                ProductName = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                Price = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5)
                            });
                        }
                    }
                }
            }
            
            return items;
        }
        
        public List<Status> GetStatuses()
        {
            var statuses = new List<Status>();
            
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                
                string query = "SELECT id, name FROM status ORDER BY id";
                
                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        statuses.Add(new Status
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            
            return statuses;
        }

        public List<PickUpPoint> GetPickUpPoints()
        {
            var points = new List<PickUpPoint>();
            
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                
                string query = "SELECT id, mailing_address, city, street, street_number FROM pic_up_poins";
                
                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        points.Add(new PickUpPoint
                        {
                            Id = reader.GetInt32(0),
                            MailingAddress = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            City = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Street = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            StreetNumber = reader.IsDBNull(4) ? "" : reader.GetString(4)
                        });
                    }
                }
            }
            
            return points;
        }
    }

    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PickUpPoint
    {
        public int Id { get; set; }
        public string MailingAddress { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string StreetNumber { get; set; }

        public string FullAddress
        {
            get
            {
                if (!string.IsNullOrEmpty(MailingAddress))
                    return MailingAddress;
                
                string address = "";
                if (!string.IsNullOrEmpty(City))
                    address = City;
                if (!string.IsNullOrEmpty(Street))
                    address += (string.IsNullOrEmpty(address) ? "" : ", ") + Street;
                if (!string.IsNullOrEmpty(StreetNumber))
                    address += (string.IsNullOrEmpty(address) ? "" : ", ") + StreetNumber;
                
                return string.IsNullOrEmpty(address) ? "—" : address;
            }
        }
    }
}