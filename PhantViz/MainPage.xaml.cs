using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI;
using System.Globalization;

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

            float leftMargin = 50;
            float bottomMargin = 100;
            float dataWidth = (float)sender.ActualWidth - leftMargin;
            float dataHeight = (float)sender.ActualHeight - bottomMargin;

            DateTime beginRange = feed.Samples[0].timestamp;
            DateTime endRange = feed.Samples[feed.Samples.Count - 1].timestamp;
            long rangeTicks = endRange.Ticks - beginRange.Ticks;
            long ticksPerPixel = (long)((double)rangeTicks / dataWidth); // can cause problems.  Maybe don't use.

            float maxHumidity = feed.Samples[0].humidity;
            float maxTemperature = feed.Samples[0].temperature;
            foreach(var sample in this.feed.Samples)
            {
                if (sample.humidity > maxHumidity) maxHumidity = sample.humidity;
                if (sample.temperature > maxTemperature) maxTemperature = sample.temperature;
            }


            float yScaleFactor = dataHeight / maxHumidity;
            int nextSampleToDrawIndex = 0;
            float prevX = -1;
            float prevY = 0;
            DateTime previousSampleLocalTime = feed.Samples[0].timestamp.ToLocalTime();
            for(float x = 0; x < dataWidth; x++)
            {
                long lastSampleTick = beginRange.Ticks + (long)(x * ticksPerPixel);

                while(nextSampleToDrawIndex < this.feed.Samples.Count &&
                    this.feed.Samples[nextSampleToDrawIndex].timestamp.Ticks < lastSampleTick)
                {
                    DateTime sampleLocalTime = feed.Samples[nextSampleToDrawIndex].timestamp.ToLocalTime();
                    if (sampleLocalTime.Day != previousSampleLocalTime.Day)
                    {
                        args.DrawingSession.DrawLine(x + leftMargin, 0, x + leftMargin, dataHeight, Colors.Black);
                        args.DrawingSession.DrawText(sampleLocalTime.ToString("d", CultureInfo.DefaultThreadCurrentUICulture), x + leftMargin, dataHeight, Colors.Black);
                    }
                    previousSampleLocalTime = sampleLocalTime;

                    float y = dataHeight - (this.feed.Samples[nextSampleToDrawIndex].humidity * yScaleFactor);
                    if (prevX < 0)
                    {
                        prevX = x + leftMargin;
                        prevY = y;
                    }
                    args.DrawingSession.DrawLine(prevX, prevY, x + leftMargin, y, Colors.Red);
                    prevX = x + leftMargin;
                    prevY = y;
                    nextSampleToDrawIndex++;

                }
            }
        }
    }

}
