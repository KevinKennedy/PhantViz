using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;
using Windows.Web.Http;

namespace PhantViz
{
    class Feed
    {
        private Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        private List<FeedField> fields = new List<FeedField>();

        public IReadOnlyCollection<FeedField> Fields { get { return this.fields; } }

        public FeedDefinition FeedDefinition { get; set; }

        public void AddField(FeedField field)
        {
            this.fields.Add(field);
        }

        public TimeFeedField GetTimestampField()
        {
            foreach (var field in this.fields)
            {
                if (field.NameInFeed == "timestamp")
                {
                    return (TimeFeedField)field;
                }
            }

            throw new InvalidOperationException("Feed.GetTimestampField: no timestamp field defined");
        }

        public int SampleCount
        {
            get
            {
                if (this.fields == null) return 0;
                if (this.fields.Count == 0) return 0;
                return this.fields[0].Count;
            }
        }

        public async Task<bool> TryLoadFromFile()
        {
            try
            {
                var storageFile = await this.localFolder.GetFileAsync(this.FeedDefinition.FeedFileName);
                var contentString = await FileIO.ReadTextAsync(storageFile);
                this.LoadFromString(contentString);
            }
            catch (System.IO.FileNotFoundException)
            {
                return false;
            }

            return true;
        }

        public async Task RefreshFromServer()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(new Uri("https://data.sparkfun.com/output/G2J4DlppVaILqAn7XnKd.json"));
                var contentString = await response.Content.ReadAsStringAsync();
                this.LoadFromString(contentString);

                // Save this to our local file store
                if (!string.IsNullOrEmpty(this.FeedDefinition.FeedFileName))
                {
                    var existingFile = ((await this.localFolder.TryGetItemAsync(this.FeedDefinition.FeedFileName)) as StorageFile);
                    if (existingFile != null)
                    {
                        await existingFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }

                    var storageFile = await this.localFolder.CreateFileAsync(Path.GetFileName(this.FeedDefinition.FeedFileName));
                    using (var randomAccessStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        using (var fileStream = randomAccessStream.AsStreamForWrite(0))
                        {
                            using (var streamWriter = new StreamWriter(fileStream))
                            {
                                await streamWriter.WriteAsync(contentString);
                            }
                        }
                    }
                }
            }

        }

        public void LoadFromString(string contentString)
        {
            var samples = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(contentString);

            samples.Sort((a, b) => a["timestamp"].CompareTo(b["timestamp"]));

            foreach (var field in this.fields)
            {
                field.Reset(samples.Count);
            }

            foreach (var sample in samples)
            {
                foreach (var field in this.fields)
                {
                    field.Append(sample);
                }
            }

        }

    }
}
