using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http; 

namespace QlikViewManagement.Client
{
    /// <summary>
    /// Must run with Administrator Privileges!!!
    /// Version1: http://www.asp.net/web-api/overview/hosting-aspnet-web-api/self-host-a-web-api
    /// Version2: http://www.codeproject.com/Tips/622260/WCF-Self-Hosting-with-Example
    /// </summary>
    /// <param name="args"></param>
    class Program
    {
        static readonly HttpClient Client = new HttpClient();
        static string ServerHealth()
        {
            var response = new StringBuilder();
            var client = Client.GetAsync("api/health").Result;
            client.EnsureSuccessStatusCode();
            var result = client.Content.ReadAsAsync<string>().Result;
            response.AppendLine(result);
            return response.ToString();
        }

        static string UserManagement(string[] args)
        {
            var response = new StringBuilder();
            var query = string.Format("api/qvusers?sargs={0}", string.Join(",", args));
            var client = Client.GetAsync(query).Result;
            client.EnsureSuccessStatusCode();

            var result = client.Content.ReadAsAsync<string>().Result;
            response.AppendLine(result);
            return response.ToString();
        }

        static string ListAllProducts()
        {
            var response = new StringBuilder();
            var resp = Client.GetAsync("api/products").Result;
            resp.EnsureSuccessStatusCode();

            var products = resp.Content.ReadAsAsync<IEnumerable<QlikViewManagement.Models.Product>>().Result;
            foreach (var p in products)
            {
                response.AppendLine(string.Format("{0} {1} {2} ({3})", p.Id, p.Name, p.Price, p.Category));
            }
            return response.ToString();
        }

        static string ListProduct(int id)
        {
            var response = new StringBuilder();
            var resp = Client.GetAsync(string.Format("api/products/{0}", id)).Result;
            resp.EnsureSuccessStatusCode();

            var product = resp.Content.ReadAsAsync<QlikViewManagement.Models.Product>().Result;
            response.AppendLine(string.Format("ID {0}: {1}", id, product.Name));
            return response.ToString();
        }

        static string ListProducts(string category)
        {
            var response = new StringBuilder();
            var query = string.Format("api/products?category={0}", category);
            var resp = Client.GetAsync(query).Result;
            resp.EnsureSuccessStatusCode();

            var products = resp.Content.ReadAsAsync<IEnumerable<QlikViewManagement.Models.Product>>().Result;
            response.AppendLine(string.Format("Products in '{0}':", category));
            foreach (var product in products)
            {
                response.AppendLine(product.Name);
            }
            return response.ToString();
        }

        static void Main(string[] args)
        {
            Client.BaseAddress = new Uri(ConfigurationManager.AppSettings["ServiceUrl"]);

            var response = new StringBuilder();
            Console.WriteLine(ServerHealth());
            Console.WriteLine(UserManagement(args));
            //Console.WriteLine(ListAllProducts());
            //Console.WriteLine(ListProduct(1));
            //Console.WriteLine(ListProducts("toys"));
            //Console.WriteLine("Press Enter to quit.");

            Console.WriteLine(response.ToString());
            Console.ReadLine();
        }
    }
}
