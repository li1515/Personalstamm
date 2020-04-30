using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Personalstamm

{
    /// <summary>
    /// Record model view
    /// </summary>
    [Serializable]
    public class JsonRecord
    {
        public string Personalnummer { get; set; }
        public string Name { get; set; }
        public float Gehalt { get; set; }
        public DateTime Aenderungsdatum { get; set; }
        public byte[] Bild { get; set; }
    }
}
