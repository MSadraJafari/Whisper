using System;
namespace Model
{
    public class DataModel
    {
        public DataModel()
        {
            time = DateTime.Now;
        }

        public string message { get; set; }
        public string sender { get; set; }
        public string receiver { get; set; }
        public DateTime time { get; set; }
        public string fileName { get; set; }
        public string fileData { get; set; } 
        public bool isFile { get; set; }
        public string Sign { get; set; }
    }
}
