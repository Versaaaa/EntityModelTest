using EntityModelTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Exercise0009
{
    internal class Program
    {

        static void Main(string[] args)
        {
            if (Login())
            {
                return;
            }
            Menu();
        }

        static ConsoleKeyInfo GetInput()
        {
            var res = Console.ReadKey();
            Console.WriteLine();
            return res;
        }

        static void Menu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("0- Esci dal programma\n1- Cambia utente\n2- Crea utente\n3- Visualizza ordini\n4- Visualizza dettagli ordine\n5- Fai ordine");
                switch (GetInput().KeyChar)
                {
                    case '0': //return
                        return;

                    case '1': //login
                        Login();
                        break;

                    case '2': // crea utente

                        Console.WriteLine("Inserisci Utente");
                        var user = Console.ReadLine();
                        Console.WriteLine("Inserisci Password");
                        var psw = Console.ReadLine();
                        try
                        {
                            InsertUser(user, psw);
                            Console.WriteLine("Utente creato con successo");
                            Thread.Sleep(1000);
                        }
                        catch (System.Data.Entity.Infrastructure.DbUpdateException)
                        {
                            Console.WriteLine($"Esiste già un utente [{user}]");
                            Console.ReadKey();
                        }
                        break;

                    case '3': // Visualizza ordine
                        Console.Clear();
                        WriteRecord(GetOrders());
                        Console.ReadKey();
                        break;

                    case '4': // Dettagli ordine
                        Console.Clear();
                        Console.WriteLine("Inserisci id");
                        try
                        {
                            var i = int.Parse(Console.ReadLine());
                            Console.Clear();
                            WriteOrderSpecifics(GetOrderSpecifics(i));
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Valore inserito non è del tipo corretto");
                        }
                        catch(System.Reflection.TargetException)
                        {
                            Console.WriteLine("Valore inserito non è presente nel db");
                        }
                        Console.ReadKey();
                        break;

                    case '5': // Fai ordine
                        try
                        {
                            Console.WriteLine("Inserisci Customer");
                            var customer = GetCustomer(Console.ReadLine());
                            var items = new Dictionary<string, List<int>>();
                            bool c1 = true;
                            while (c1)
                            {

                                Console.WriteLine("Inserisci Item");
                                {
                                    var item = GetItem(Console.ReadLine());

                                    items[item.item] = new List<int>();

                                    Console.WriteLine("Inserisci quantità");
                                    items[item.item].Add(int.Parse(Console.ReadLine()));
                                    Console.WriteLine("Inserisci prezzo");
                                    items[item.item].Add(int.Parse(Console.ReadLine()));
                                }

                                bool c2 = true;
                                while (c2)
                                {
                                    Console.Clear();
                                    Console.WriteLine("Vuoi continuare? (y/n)");
                                    switch (GetInput().Key)
                                    {
                                        case ConsoleKey.Y:
                                            c2 = false;
                                            break;
                                        case ConsoleKey.N:
                                            c1 = false;
                                            c2 = false;
                                            break;
                                    }
                                }
                            }

                            InsertOrder(customer.name, items);

                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Console.WriteLine("Valore inserito non presente nel DB");
                            Console.ReadKey();
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        static void InsertUser(string utente, string psw)
        {
            using (var connection = new ordersEntities())
            {
                connection.users.Add(new users() { username = utente, psw = psw});
                connection.SaveChanges();
            }
        }

        static bool Login()
        {
            try
            {
                using (var connection = new ordersEntities())
                {
                    Console.Clear();

                    {
                        
                        if (connection.users.Count() < 1)
                        {
                            Console.WriteLine("Inizializzazione utente admin");
                            connection.users.Add(new users() { username = "admin", psw = "admin" });
                            Thread.Sleep(1000);
                        }
                    }

                    while (true)
                    {
                        Console.Clear();

                        #region Ask for input
                        Console.WriteLine("Inserisci Utente");
                        var user = Console.ReadLine();

                        Console.WriteLine("Inserisci Password");
                        var psw = Console.ReadLine();
                        #endregion

                        if (LoginCheck(user, psw))
                        {
                            Console.WriteLine("Login effettuato con successo");
                            Thread.Sleep(1000);
                            return false;
                        }
                        else
                        {
                            Console.WriteLine("Username o Password errati");
                            Thread.Sleep(1000);

                            bool x = true;
                            while (x)
                            {

                                Console.Clear();

                                Console.WriteLine("Vuoi continuare? (y/n)");
                                switch (GetInput().Key)
                                {
                                    case ConsoleKey.N:
                                        return true;

                                    case ConsoleKey.Y:
                                        {
                                            x = false;
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return true;
            }
        }

        static bool LoginCheck(string username, string psw)
        {
            var res = false;
            using(var connection = new ordersEntities())
            {
                var record = connection.users.Find(username);
                if (!(record == null))
                {
                    if (record.psw.Equals(psw))
                    {
                        res = true;
                    }
                }
            }
            return res;
        }

        static List<Dictionary<string, string>> GetOrders()
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
                        foreach(var order in connection.orderitems.Where(x => x.orderid == record.orderid))
                        {
                            tot += order.price*order.qty;
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

        static List<Dictionary<string, string>> GetOrderSpecifics(int id)
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

        static void WriteRecord(ICollection<Dictionary<string, string>> records)
        {
            foreach (var record in records)
            {
                foreach (var key in record.Keys)
                {
                    Console.WriteLine($"{key} = {record[key]}");
                }
                Console.WriteLine();
            }
        }

        static void WriteOrderSpecifics(ICollection<Dictionary<string, string>> records)
        {
            Console.WriteLine($"customer = {records.ElementAt(0)["customer"]}");
            Console.WriteLine($"order date = {records.ElementAt(0)["order date"]}");
            Console.WriteLine($"-----------------------------------------");

            foreach (var record in records)
            {
                Console.WriteLine($"item = {record["item"]}");
                Console.WriteLine($"quantity = {record["quantity"]}");
                Console.WriteLine($"price = {record["price"]}");
                Console.WriteLine();
            }
        }

        static customers GetCustomer(string customer)
        {
            customers res;

            using (var connection = new ordersEntities())
            {
                res = connection.customers.Find(customer);
            }
            return res == null ? throw new ArgumentOutOfRangeException() : res;
        }

        static items GetItem(string item)
        {
            items res;

            using (var connection = new ordersEntities())
            {
                res = connection.items.Find(item);
            }
            return res == null ? throw new ArgumentOutOfRangeException() : res;
        }

        static void InsertOrder(string customer, Dictionary<string, List<int>> items)
        {
            using (var connection = new ordersEntities())
            {
                try
                {
                    int i = connection.orders.OrderByDescending(x => x.orderid).FirstOrDefault().orderid + 1;

                    connection.orders.Add(new orders() {orderid = i, customername = customer , orderdate = DateTime.Now});

                    foreach (var key in items.Keys)
                    {
                        connection.orderitems.Add(new orderitems() { orderid = i, itemname = key, qty = items[key][0], price = items[key][1] });
                    }

                    connection.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.ReadKey();
                }
            }
        }
    }
}
