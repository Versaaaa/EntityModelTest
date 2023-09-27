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
                            DBWriter.InsertUser(user, psw);
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
                        WriteRecord(DBReader.GetOrders());
                        Console.ReadKey();
                        break;

                    case '4': // Dettagli ordine
                        Console.Clear();
                        Console.WriteLine("Inserisci id");
                        try
                        {
                            var i = int.Parse(Console.ReadLine());
                            Console.Clear();
                            WriteOrderSpecifics(DBReader.GetOrderSpecifics(i));
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
                            var customer = DBReader.GetCustomer(Console.ReadLine());
                            var items = new Dictionary<string, List<int>>();
                            bool c1 = true;
                            while (c1)
                            {

                                Console.WriteLine("Inserisci Item");
                                {
                                    var item = DBReader.GetItem(Console.ReadLine());

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

                            DBWriter.InsertOrder(customer.name, items);

                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Console.WriteLine("Valore inserito non presente nel DB");
                            Console.ReadKey();
                        }
                        catch (FormatException)
                        { 
                            Console.WriteLine("Valore inserito non è del tipo corretto");
                            Console.ReadKey();
                        }
                        catch (ArgumentException)
                        {
                            Console.WriteLine("Impossibile effettuare ordine");
                            Console.ReadKey();
                        }

                        break;

                    default:
                        break;
                }
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
            using(var connection = new ordersEntities())
            {
                try
                {
                    return connection.users.Find(username).psw.Equals(psw);
                }
                catch(NullReferenceException)
                {
                    return false;
                }
            }
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
    }
}
