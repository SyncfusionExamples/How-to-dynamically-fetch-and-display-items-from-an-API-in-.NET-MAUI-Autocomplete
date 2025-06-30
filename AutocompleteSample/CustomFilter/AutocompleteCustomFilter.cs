using AutocompleteSample.ViewModel;
using Syncfusion.Maui.Inputs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutocompleteSample.CustomFilter
{
    public class AutocompleteCustomFilter : IAutocompleteFilterBehavior
    {
        private CancellationTokenSource? _cts;

        public async Task<object?> GetMatchingItemsAsync(SfAutocomplete autocomplete, AutocompleteFilterInfo filterInfo)
        {
            // Cancel any ongoing request
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var cancellationToken = _cts.Token;

            if (autocomplete != null)
            {
                var text = filterInfo.Text ?? string.Empty;
                using (HttpClient client = new HttpClient())
                {
                    string filter = $"$filter=startswith(ContactName, '{text}') or startswith(ContactTitle, '{text}') or startswith(Country, '{text}')";
                    string requestUrl = $"https://services.odata.org/V4/Northwind/Northwind.svc/Customers?{filter}&$format=json";
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(requestUrl, cancellationToken);
                        response.EnsureSuccessStatusCode();

                        string jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);

                        var odataResponse = JsonSerializer.Deserialize<ODataResponse<Customer>>(jsonResponse);

                        List<Customer> customers = odataResponse?.value?.Take(5).ToList() ?? new List<Customer>();
                        return customers;
                    }
                    catch (OperationCanceledException)
                    {
                        // Request was cancelled, return empty list
                        return new List<Customer>();
                    }
                    catch (Exception ex)
                    {
                        return new List<Customer>();
                    }
                }
            }
            return new List<Customer>();
        }
    }
}
