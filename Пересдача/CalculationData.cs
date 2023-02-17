using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Text;
using System.Linq;

namespace Lab
{
    public class CalculationData
    {
        public int Id { get; set; }

        [NotMapped]
        public double[] X
        {
            get
            {
                string[] tab = this.XData.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                return tab.Select(str => double.Parse(str)).ToArray();
            }
            set
            {
                var xStr = new StringBuilder();

                foreach (var item in value)
                {
                    xStr.Append(item + " ");
                }

                this.XData = xStr.ToString();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string XData { get; set; }

        [NotMapped]
        public double[] Y
        {
            get
            {
                string[] tab = this.YData.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                return tab.Select(str => double.Parse(str)).ToArray();
            }
            set
            {
                var xStr = new StringBuilder();

                foreach (var item in value)
                {
                    xStr.Append(item + " ");
                }

                this.YData = xStr.ToString();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string YData { get; set; }

        [NotMapped]
        public double[] Z
        {
            get
            {
                string[] tab = this.ZData.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                return tab.Select(str => double.Parse(str)).ToArray();
            }
            set
            {
                var xStr = new StringBuilder();

                foreach (var item in value)
                {
                    xStr.Append(item + " ");
                }

                this.ZData = xStr.ToString();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string ZData { get; set; }
    }
}
