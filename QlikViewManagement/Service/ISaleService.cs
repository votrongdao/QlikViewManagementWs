using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using QlikViewManagement.Models;

namespace QlikViewManagement.Service
{
    [ServiceContract]
    interface ISaleService
    {
        [OperationContract]
        bool InsertCustomer(Customer obj);

        [OperationContract]
        List<Customer> GetAllCustomer();

        [OperationContract]
        bool DeleteCustomer(int Cid);

        [OperationContract]
        bool UpdateCustomer(Customer obj);
    }

    public class SaleService : ISaleService
    {
        public bool InsertCustomer(Customer obj)
        {
            cutomerList.Add(obj);
            return true;
        }

        public List<Customer> GetAllCustomer()
        {
            return cutomerList;
        }

        public bool DeleteCustomer(int Cid)
        {
            var item = cutomerList.First(x => x.CustomerID == Cid);

            cutomerList.Remove(item);
            return true;
        }

        public bool UpdateCustomer(Customer obj)
        {
            var list = cutomerList;
            //cutomerList.Where(p => p.CustomerID == obj.CustomerID).Update(p => p.CustomerName = obj.CustomerName);
            return true;
        }

        public static List<Customer> cutomerList = new List<Customer>()
         {
        new Customer {CustomerID = 1, CustomerName="Sujeet", 
        Address="Pune", EmailId="test@yahoo.com" },
        new Customer {CustomerID = 2, CustomerName="Rahul", 
        Address="Pune", EmailId="test@yahoo.com" },
        new Customer {CustomerID = 3, CustomerName="Mayur", 
        Address="Pune", EmailId="test@yahoo.com"}
         };


    }
}
