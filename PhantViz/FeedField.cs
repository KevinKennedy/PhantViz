using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace PhantViz
{
    public abstract class FeedField
    {
        public Color Color { get; set; }
        public string NameInFeed { get; set; }
        public string DisplayName { get; set; }
        public string MinValueDisplay { get; protected set; }
        public string MaxValueDisplay { get; protected set; }
        public abstract void Reset(int sampleCount);
        public abstract void Append(IDictionary<string, string> row);
        public abstract int Count { get; }
    }

    public class FloatFeedField : FeedField
    {
        private List<float> values = new List<float>();

        private float minValue;
        public float MinValue
        {
            get
            {
                return this.minValue;
            }

            set
            {
                this.minValue = value;
                this.MinValueDisplay = this.minValue.ToString();
            }
        }

        private float maxValue;
        public float MaxValue
        {
            get
            {
                return this.maxValue;
            }

            set
            {
                this.maxValue = value;
                this.MaxValueDisplay = this.maxValue.ToString();
            }
        }

        public override void Reset(int sampleCount)
        {
            this.values.Clear();
            this.values.Capacity = sampleCount;
        }

        public override void Append(IDictionary<string, string> row)
        {
            var value = float.Parse(row[this.NameInFeed]);
            this.values.Add(value);
        }

        public override int Count
        {
            get
            {
                return this.values.Count;
            }
        }

        public float this[int index]
        {
            get
            {
                return this.values[index];
            }
        }
    }

    public class TimeFeedField : FeedField
    {
        private List<DateTime> values = new List<DateTime>();

        private DateTime minValue;
        private DateTime maxValue;
        
        public DateTime MinValue { get { return this.minValue; } set { this.minValue = value; this.MinValueDisplay = this.minValue.ToString(); } }
        public DateTime MaxValue { get { return this.maxValue; } set { this.maxValue = value; this.MaxValueDisplay = this.maxValue.ToString(); } }

        public override void Reset(int sampleCount)
        {
            this.values.Clear();
            this.values.Capacity = sampleCount;
        }

        public override void Append(IDictionary<string, string> row)
        {
            var value = DateTime.Parse(row[this.NameInFeed]).ToUniversalTime();

            if(this.values.Count == 0)
            {
                this.MinValue = this.MaxValue = value;
            }
            else if(value < this.minValue)
            {
                this.MinValue = value;
            }
            else if(value > this.maxValue)
            {
                this.MaxValue = value;
            }

            this.values.Add(value);
        }

        public override int Count
        {
            get
            {
                return this.values.Count;
            }
        }
        public DateTime this[int index]
        {
            get
            {
                return this.values[index];
            }
        }
    }
}
