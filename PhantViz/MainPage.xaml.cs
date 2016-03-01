using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI;

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
            this.Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Prevent memory leaks if this page comes and goes
            this.canvas.RemoveFromVisualTree();
            this.canvas = null;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await feed.TryLoadFromFile();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await this.feed.RefreshFromServer();
        }

        private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            int sampleCount = this.feed.Samples.Count;

            if(sampleCount <= 0 || this.ActualWidth <= 0)
            {
                return;
            }

            float maxHumidity = feed.Samples[0].humidity;
            float maxTemperature = feed.Samples[0].temperature;
            foreach(var sample in this.feed.Samples)
            {
                if (sample.humidity > maxHumidity) maxHumidity = sample.humidity;
                if (sample.temperature > maxTemperature) maxTemperature = sample.temperature;
            }

            float samplesPerPixel = (float)sampleCount / (float)sender.ActualWidth;
            float yScaleFactor = (float)sender.ActualHeight / maxHumidity;
            int nextSampleToDrawIndex = 0;
            float prevX = -1;
            float prevY = 0;
            for(float x = 0; x < sender.ActualWidth; x++)
            {
                int postUltimateSampleIndex = (int) ((float)(x + 1) * samplesPerPixel);
                if (postUltimateSampleIndex > sampleCount) postUltimateSampleIndex = sampleCount;

                for(int index = nextSampleToDrawIndex; index < postUltimateSampleIndex; index++)
                {
                    float y = (float)sender.ActualHeight - (this.feed.Samples[index].humidity * yScaleFactor);
                    if(prevX < 0)
                    {
                        prevX = x;
                        prevY = y;
                    }
                    args.DrawingSession.DrawLine(prevX, prevY, x, y, Colors.Red);
                    prevX = x;
                    prevY = y;
                }

                nextSampleToDrawIndex = postUltimateSampleIndex;
            }
        }
    }

}
