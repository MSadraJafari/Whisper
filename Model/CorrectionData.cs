using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class CorrectionData
    {
        public CorrectionData()
        {
            time = DateTime.Now;
        }

        public string message { get; set; }
        public string sender { get; set; }
        public string receiver { get; set; }
        public DateTime time { get; set; }
        public string Sign { get; set; }
    }
}
