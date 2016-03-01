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

    class FeedDataManager
    {
        private Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        public string FeedUrlString { get; set; }

        public string FeedDisplayName { get; set; }

        public string FeedFileName { get; set; }

        public List<Sample> Samples { get { return this.allSamples; } }

        private string allData;
        private List<Sample> allSamples = new List<Sample>();


        public async Task<bool> TryLoadFromFile()
        {
            try
            {
                var storageFile = await this.localFolder.GetFileAsync(this.FeedFileName);
                var contentString = await FileIO.ReadTextAsync(storageFile);
                var samples = JsonConvert.DeserializeObject<List<Sample>>(contentString);

                samples.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));

                this.allData = contentString;
                this.allSamples = samples;
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

                var samples = JsonConvert.DeserializeObject<List<Sample>>(contentString);

                samples.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));

                // Save this to our local file store
                if (!string.IsNullOrEmpty(this.FeedFileName))
                {
                    var existingFile = ((await this.localFolder.TryGetItemAsync(this.FeedFileName)) as StorageFile);
                    if(existingFile != null)
                    {
                        await existingFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }

                    var storageFile = await this.localFolder.CreateFileAsync(Path.GetFileName(this.FeedFileName));
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

                this.allData = contentString;
                this.allSamples = samples;
            }

        }
    }
}
