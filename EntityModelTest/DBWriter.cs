using EntityModelTest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exercise0009
{
    public static class DBWriter
    {

        public static void InsertOrder(string customer, Dictionary<string, List<int>> items)
        {
            using (var connection = new ordersEntities())
            {
                int i = connection.orders.OrderByDescending(x => x.orderid).FirstOrDefault().orderid + 1;

                connection.orders.Add(new orders() { orderid = i, customername = customer, orderdate = DateTime.Now });

                foreach (var key in items.Keys)
                {
                    var qty = items[key][0];
                    var price = items[key][1];

                    if (qty < 1 || price < 0)
                    {
                        throw new ArgumentException();
                    }
                    connection.orderitems.Add(new orderitems() { orderid = i, itemname = key, qty = qty, price = price });
                }
                connection.SaveChanges();
            }
        }

        public static void InsertUser(string utente, string psw)
        {
            using (var connection = new ordersEntities())
            {
                connection.users.Add(new users() { username = utente, psw = psw });
                connection.SaveChanges();
            }
        }
    }
}