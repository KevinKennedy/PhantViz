using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace PhantViz
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        FeedDataManager feed;

        public MainPage()
        {
            this.InitializeComponent();

            feed = new FeedDataManager()
            {
                FeedFileName="Humidity.json",
                FeedUrlString = "https://data.sparkfun.com/output/G2J4DlppVaILqAn7XnKd.json",
                FeedDisplayName = "Attic Temp and Humidity",
            };

            this.Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await feed.TryLoadFromFile();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await this.feed.RefreshFromServer();

            //var client = new HttpClient();

            //var response = await client.GetAsync(new Uri("https://data.sparkfun.com/output/G2J4DlppVaILqAn7XnKd.json"));
            //var contentString = await response.Content.ReadAsStringAsync();

            //var readings = JsonConvert.DeserializeObject<List<Sample>>(contentString);

        }
    }

}
