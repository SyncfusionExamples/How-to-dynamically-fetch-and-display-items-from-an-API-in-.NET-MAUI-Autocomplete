using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutocompleteSample.ViewModel
{
    //Model
    public class Customer
    {
        public string CustomerID { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
    }

    // Represents the structure of the OData response.
    public class ODataResponse<T>
    {
        public List<T> value { get; set; }
    }

    //ViewModel.cs
    public class CustomerViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Customer> CustomerDetails { get; set; }

        public CustomerViewModel()
        {
            CustomerDetails = new ObservableCollection<Customer>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
