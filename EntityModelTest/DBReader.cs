using EntityModelTest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exercise0009
{
    public static class DBReader
    {
        public static customers GetCustomer(string customer)
        {
            customers res;

            using (var connection = new ordersEntities())
            {
                res = connection.customers.Find(customer);
            }
            return res == null ? throw new ArgumentOutOfRangeException() : res;
        }

        public static items GetItem(string item)
        {
            items res;

            using (var connection = new ordersEntities())
            {
                res = connection.items.Find(item);
            }
            return res == null ? throw new ArgumentOutOfRangeException() : res;
        }

        public static List<Dictionary<string, string>> GetOrders()
        {
            var res = new List<Dictionary<string, string>>();
            using (var connection = new ordersEntities())
            {
                foreach (var record in connection.orders)
                {
                    try
                    {
                        var item = new Dictionary<string, string>();
                        var tot = 0;
                        foreach (var order in connection.orderitems.Where(x => x.orderid == record.orderid))
                        {
                            tot += order.price * order.qty;
                        }
                        item["id"] = record.orderid.ToString();
                        item["customer"] = record.customers.name;
                        item["order date"] = record.orderdate.ToString();
                        item["total"] = tot.ToString();
                        res.Add(item);
                    }
                    catch
                    {
                        connection.orders.Remove(record);
                    }
                }
                connection.SaveChanges();
            }

            return res;
        }

        public static List<Dictionary<string, string>> GetOrderSpecifics(int id)
        {
            var res = new List<Dictionary<string, string>>();

            using (var connection = new ordersEntities())
            {
                var record = connection.orders.Find(id);

                foreach (var order in connection.orderitems.Where(x => x.orderid == record.orderid))
                {
                    var item = new Dictionary<string, string>();
                    item["customer"] = record.customers.name;
                    item["order date"] = record.orderdate.ToString();
                    item["item"] = order.itemname;
                    item["quantity"] = order.qty.ToString();
                    item["price"] = order.price.ToString();
                    res.Add(item);
                }
            }
            return res;
        }
    }
}