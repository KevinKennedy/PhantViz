using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Newtonsoft.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PhantViz
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var client = new HttpClient();

            var response = await client.GetAsync(new Uri("https://data.sparkfun.com/output/G2J4DlppVaILqAn7XnKd.json"));
            var contentString = await response.Content.ReadAsStringAsync();

            var readings = JsonConvert.DeserializeObject<List<Reading>>(contentString);
            //var serializer = new JsonSerializer();
            //serializer.Deserialize<SerializationContainer>(new Json())


        }
    }

    public class Reading
    {
        public float temperature { get; set; }
        public float humidity { get; set; }
        public DateTime timestamp { get; set; }
    }

    [JsonObject]
    public class SerializationContainer
    {
        [JsonProperty(ItemIsReference = true, Order = 0)]
        public List<Reading> Readings { get; set; }
    }
    }
