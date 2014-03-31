using System.Runtime.Serialization;

namespace QlikViewManagement.Models
{
    [DataContract]
    public class Customer
    {
        [DataMember]
        public int CustomerID;
        [DataMember]
        public string CustomerName;
        [DataMember]
        public string Address;
        [DataMember]
        public string EmailId;
    }
}
