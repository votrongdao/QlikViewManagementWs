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

        static string QvUserManager(string args)
        {
            StringBuilder response = new StringBuilder();
            var resp = Client.GetAsync(string.Format("api/qvusers/{0}", args)).Result;
            resp.EnsureSuccessStatusCode();

            var products = resp.Content.ReadAsAsync<string>().Result;
            foreach (var p in products)
            {
                response.AppendLine(string.Format("{0}", p));
            }
            return response.ToString();
        }

        static string ListAllProducts()
        {
            StringBuilder response = new StringBuilder();
            HttpResponseMessage resp = Client.GetAsync("api/products").Result;
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
            StringBuilder response = new StringBuilder();
            var resp = Client.GetAsync(string.Format("api/products/{0}", id)).Result;
            resp.EnsureSuccessStatusCode();

            var product = resp.Content.ReadAsAsync<QlikViewManagement.Models.Product>().Result;
            response.AppendLine(string.Format("ID {0}: {1}", id, product.Name));
            return response.ToString();
        }

        static string ListProducts(string category)
        {
            StringBuilder response = new StringBuilder();
            response.AppendLine(string.Format("Products in '{0}':", category));

            string query = string.Format("api/products?category={0}", category);

            var resp = Client.GetAsync(query).Result;
            resp.EnsureSuccessStatusCode();

            var products = resp.Content.ReadAsAsync<IEnumerable<QlikViewManagement.Models.Product>>().Result;
            foreach (var product in products)
            {
                response.AppendLine(product.Name);
            }
            return response.ToString();
        }

        static void Main(string[] args)
        {
            Client.BaseAddress = new Uri(ConfigurationManager.AppSettings["ServiceUrl"]);

            StringBuilder response = new StringBuilder();
            //QvUserManager(string.Join(",", args));
            response.AppendLine(ListAllProducts());
            response.AppendLine(ListProduct(1));
            response.AppendLine(ListProducts("toys"));
            response.AppendLine("Press Enter to quit.");

            Console.WriteLine(response.ToString());
            Console.ReadLine();
        }
    }
}
