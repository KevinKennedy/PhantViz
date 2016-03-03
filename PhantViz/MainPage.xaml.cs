using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using System.Globalization;

namespace PhantViz
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        FeedDataManager feedDataManager;
        Feed feed;

        public MainPage()
        {
            this.InitializeComponent();

            this.feedDataManager = new FeedDataManager()
            {
                FeedFileName="Humidity.json",
                FeedUrlString = "https://data.sparkfun.com/output/G2J4DlppVaILqAn7XnKd.json",
                FeedDisplayName = "Attic Temp and Humidity",
            };

            this.feed = new Feed();
            this.feed.FeedDefinition = new FeedDefinition()
            {
                FeedUrlString = "https://data.sparkfun.com/output/G2J4DlppVaILqAn7XnKd.json",
                FeedDisplayName = "Attic Temp and Humidity",
                FeedFileName = "TempAndHumidity.json",
            };
            this.feed.AddField(new TimeFeedField()
            {
                NameInFeed = "timestamp",
                DisplayName = "Time",
            });
            this.feed.AddField(new FloatFeedField()
            {
                NameInFeed = "humidity",
                DisplayName = "Relative Humidity",
                MinValue = 0.0f,
                MaxValue = 100.0f,
                Color = Colors.Aqua,
            });
            this.feed.AddField(new FloatFeedField()
            {
                NameInFeed = "temperature",
                DisplayName = "Temperature",
                MinValue = 0.0f,
                MaxValue = 100.0f,
                Color = Colors.Red,
            });


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
            //await this.feedDataManager.TryLoadFromFile();
            await this.feed.TryLoadFromFile();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await this.feed.RefreshFromServer();
        }

        private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            int sampleCount = this.feed.SampleCount;

            if (sampleCount <= 0 || sender.ActualWidth <= 0)
            {
                return;
            }

            float leftMargin = 50;
            float bottomMargin = 100;
            float dataWidth = (float)sender.ActualWidth - leftMargin;
            float dataHeight = (float)sender.ActualHeight - bottomMargin;

            var timestampField = this.feed.GetTimestampField();
            DateTime beginRange = timestampField.MinValue;
            DateTime endRange = timestampField.MaxValue;
            long rangeTicks = endRange.Ticks - beginRange.Ticks;
            long ticksPerPixel = (long)((double)rangeTicks / dataWidth); // may cause problems with error accumulation.  Maybe don't use.

            // Draw the axis and labels
            args.DrawingSession.DrawLine(leftMargin, 0, leftMargin, dataHeight, Colors.Blue, 2.0f);
            args.DrawingSession.DrawLine(leftMargin, dataHeight, leftMargin + dataWidth, dataHeight, Colors.Blue, 2.0f);

            foreach(var field in this.feed.Fields)
            {
                FloatFeedField floatField = field as FloatFeedField;

                if(floatField == null)
                {
                    continue;
                }

                float yScaleFactor = dataHeight / (floatField.MaxValue); // BUGBUG - assumes min value is zero
                int nextSampleToDrawIndex = 0;
                float prevX = -1;
                float prevY = 0;
                DateTime previousSampleLocalTime = timestampField[0].ToLocalTime();
                for (float x = 0; x < dataWidth; x++)
                {
                    long lastSampleTick = beginRange.Ticks + (long)(x * ticksPerPixel);

                    while (nextSampleToDrawIndex < sampleCount &&
                        timestampField[nextSampleToDrawIndex].Ticks < lastSampleTick)
                    {
                        DateTime sampleLocalTime = timestampField[nextSampleToDrawIndex].ToLocalTime();
                        if (sampleLocalTime.Day != previousSampleLocalTime.Day)
                        {
                            args.DrawingSession.DrawLine(x + leftMargin, 0, x + leftMargin, dataHeight, Colors.Black);
                            args.DrawingSession.DrawText(sampleLocalTime.ToString("d", CultureInfo.DefaultThreadCurrentUICulture), x + leftMargin, dataHeight, Colors.DarkCyan);
                        }
                        previousSampleLocalTime = sampleLocalTime;

                        float y = dataHeight - (floatField[nextSampleToDrawIndex] * yScaleFactor);
                        if (prevX < 0)
                        {
                            prevX = x + leftMargin;
                            prevY = y;
                        }
                        args.DrawingSession.DrawLine(prevX, prevY, x + leftMargin, y, floatField.Color);
                        prevX = x + leftMargin;
                        prevY = y;
                        nextSampleToDrawIndex++;

                    }
                }
            }

        }

#if no
        private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            int sampleCount = this.feedDataManager.Samples.Count;

            if(sampleCount <= 0 || this.ActualWidth <= 0)
            {
                return;
            }

            float leftMargin = 50;
            float bottomMargin = 100;
            float dataWidth = (float)sender.ActualWidth - leftMargin;
            float dataHeight = (float)sender.ActualHeight - bottomMargin;

            DateTime beginRange = feedDataManager.Samples[0].timestamp;
            DateTime endRange = feedDataManager.Samples[feedDataManager.Samples.Count - 1].timestamp;
            long rangeTicks = endRange.Ticks - beginRange.Ticks;
            long ticksPerPixel = (long)((double)rangeTicks / dataWidth); // can cause problems.  Maybe don't use.

            float maxHumidity = feedDataManager.Samples[0].humidity;
            float maxTemperature = feedDataManager.Samples[0].temperature;
            foreach(var sample in this.feedDataManager.Samples)
            {
                if (sample.humidity > maxHumidity) maxHumidity = sample.humidity;
                if (sample.temperature > maxTemperature) maxTemperature = sample.temperature;
            }

            // Draw the axis and labels
            args.DrawingSession.DrawLine(leftMargin, 0, leftMargin, dataHeight, Colors.Blue, 2.0f);
            args.DrawingSession.DrawLine(leftMargin, dataHeight, leftMargin + dataWidth, dataHeight, Colors.Blue, 2.0f);


            float yScaleFactor = dataHeight / maxHumidity;
            int nextSampleToDrawIndex = 0;
            float prevX = -1;
            float prevY = 0;
            DateTime previousSampleLocalTime = feedDataManager.Samples[0].timestamp.ToLocalTime();
            for(float x = 0; x < dataWidth; x++)
            {
                long lastSampleTick = beginRange.Ticks + (long)(x * ticksPerPixel);

                while(nextSampleToDrawIndex < this.feedDataManager.Samples.Count &&
                    this.feedDataManager.Samples[nextSampleToDrawIndex].timestamp.Ticks < lastSampleTick)
                {
                    DateTime sampleLocalTime = feedDataManager.Samples[nextSampleToDrawIndex].timestamp.ToLocalTime();
                    if (sampleLocalTime.Day != previousSampleLocalTime.Day)
                    {
                        args.DrawingSession.DrawLine(x + leftMargin, 0, x + leftMargin, dataHeight, Colors.Black);
                        args.DrawingSession.DrawText(sampleLocalTime.ToString("d", CultureInfo.DefaultThreadCurrentUICulture), x + leftMargin, dataHeight, Colors.Black);
                    }
                    previousSampleLocalTime = sampleLocalTime;

                    float y = dataHeight - (this.feedDataManager.Samples[nextSampleToDrawIndex].humidity * yScaleFactor);
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
#endif

    }

}
