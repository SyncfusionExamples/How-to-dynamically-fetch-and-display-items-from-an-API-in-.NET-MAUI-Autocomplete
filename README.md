# How-to-dynamically-fetch-and-display-items-from-an-API-in-.NET-MAUI-Autocomplete
This repository contains a sample explaining how to dynamically fetch and display items from an API in .NET MAUI Autocomplete.

**Step 1: Define the User Interface**

In the `MainPage.xaml`, configure the visual interface by placing a `Autocomplete` control inside an outlined `TextInputLayout`. This setup creates an intuitive search input field with a material design outline. We define a customized `ItemTemplate` to display the contact's name, title, city, and country in a structured format, giving users contextual information while selecting.

> The key customization here is attaching a `CustomFilterBehavior`, which triggers API calls based on user input—making the UI reactive and data-driven.

 
 ```
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:filter="clr-namespace:AutocompleteSample.CustomFilter"
             xmlns:vm="clr-namespace:AutocompleteSample.ViewModel"
             xmlns:editors="clr-namespace:Syncfusion.Maui.Inputs;assembly=Syncfusion.Maui.Inputs"
             xmlns:inputlayout="clr-namespace:Syncfusion.Maui.Core;assembly=Syncfusion.Maui.Core"
             x:Class="AutocompleteSample.MainPage">

    <ContentPage.BindingContext>
        <vm:CustomerViewModel/>
    </ContentPage.BindingContext>

    <VerticalStackLayout Padding="30,0" Spacing="25">
        <inputlayout:SfTextInputLayout ContainerType="Outlined"
                                       ShowHint="False"
                                       ReserveSpaceForAssistiveLabels="False"
                                       Padding="0,-2,0,0"
                                       OutlineCornerRadius="8"
                                       IsHintAlwaysFloated="True"
                                       WidthRequest="360"
                                       HeightRequest="60">
            <editors:SfAutocomplete NoResultsFoundText=""
                                    DropDownBackground="WhiteSmoke"
                                    DropDownStroke="Black"
                                    DropDownItemHeight="54"
                                    PlaceholderColor="#283618"
                                    ClearButtonIconColor="#283618"
                                    TextSearchMode="Contains"
                                    Placeholder="Search by Customer Name/Designation/Country"
                                    DisplayMemberPath="ContactName"
                                    ItemsSource="{Binding OrderedItems}">
                <editors:SfAutocomplete.FilterBehavior>
                    <filter:AutocompleteCustomFilter/>
                </editors:SfAutocomplete.FilterBehavior>
                <editors:SfAutocomplete.ItemTemplate>
                    <DataTemplate>
                        <Grid Padding="10" RowDefinitions="Auto,Auto">
                            <HorizontalStackLayout Grid.Row="0" Spacing="2">
                                <Label Text="{Binding ContactName}" 
                                       FontAttributes="Bold"
                                       VerticalOptions="Center"
                                       FontSize="16" 
                                       TextColor="Black"/>
                                <Label Text=" - " VerticalOptions="Center" FontSize="12"/>
                                <Label VerticalOptions="Center"
                                       Text="{Binding ContactTitle}"
                                       FontAttributes="Bold"
                                       FontSize="12"
                                       TextColor="Black"/>
                                <Label Text="&#xe78c;"
                                       VerticalOptions="Center"
                                       FontSize="14"
                                       Padding="4,0"
                                       TextColor="#50D072"
                                       FontFamily="MauiMaterialAssets"/>
                            </HorizontalStackLayout>
                            <HorizontalStackLayout Grid.Row="1" Spacing="2">
                                <Label Text="&#xe71c;"
                                       VerticalOptions="Center"
                                       FontSize="14"
                                       Padding="4,0"
                                       TextColor="Green"
                                       FontFamily="MauiMaterialAssets"/>
                                <Label Text="{Binding City}" 
                                       FontSize="12"
                                       VerticalOptions="Center"/>
                                <Label Text=" , " VerticalOptions="Center" FontSize="12"/>
                                <Label VerticalOptions="Center"
                                       Text="{Binding Country}"
                                       FontSize="12"
                                       TextColor="Black"/>
                            </HorizontalStackLayout>
                        </Grid>
                    </DataTemplate>
                </editors:SfAutocomplete.ItemTemplate>
            </editors:SfAutocomplete>
        </inputlayout:SfTextInputLayout>
    </VerticalStackLayout>
</ContentPage> 
 ```

**Step 2: Set Up Model and ViewModel**

This step establishes the data structure and binding logic:

* The `Customer` class models the shape of the data retrieved from the API. It includes all the properties returned from the Northwind service (like CustomerID, ContactName, City, Country, etc.).
* The `ODataResponse<T>` generic class represents the structure of the `JSON` returned by the API.
* The `CustomerViewModel` holds the CustomerDetails collection and implements INotifyPropertyChanged to support dynamic data binding.

> This MVVM setup ensures clean separation between UI and data logic while enabling the Autocomplete control to reflect data changes in real-time.

 
 ```
// Model class
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

// OData response structure
public class ODataResponse<T>
{
    public List<T> value { get; set; }
}

// ViewModel
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
 ```

**Step 3: Implement Custom Filter Behavior**

Here’s where the dynamic fetching logic comes in:

* The `AutocompleteCustomFilter` class implements `IAutocompleteFilterBehavior`, letting you define a custom matching strategy.
* When the user types text, the overridden `GetMatchingItemsAsync` method is invoked. It uses `HttpClient` to asynchronously call the Northwind OData API, applying a startswith filter on the ContactName, ContactTitle, and Country fields.
* The API response is parsed into a list of Customer objects, and only the top 5 matches are returned to avoid overloading the UI.
* Assign the `AutocompleteCustomFilter` to the `FilterBehavior` property of the Autocomplete control in the XAML layout.

> The use of `CancellationTokenSource` ensures only the latest request is processed, avoiding race conditions or unnecessary API calls when users type quickly.
 
 ```
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
                        // Log or handle exception
                        return new List<Customer>();
                    }
                }
            }
            return new List<Customer>();
        }
    } 
 ```
Output:
![image.png](https://support.syncfusion.com/kb/agent/attachment/article/20441/inline?token=eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjQyMTUzIiwib3JnaWQiOiIzIiwiaXNzIjoic3VwcG9ydC5zeW5jZnVzaW9uLmNvbSJ9.b_YAZtjQfJiZIWCUdp8V-5GNBzAZb2wLe3D7n4C14J4)
 
 ![Android](https://support.syncfusion.com/kb/agent/attachment/article/20441/inline?token=eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjQyMTU2Iiwib3JnaWQiOiIzIiwiaXNzIjoic3VwcG9ydC5zeW5jZnVzaW9uLmNvbSJ9.Wb6-bJsYhy_Dj6Vv_Sa384QZXqRhqN1Pj7lJ6cL9v4g)
