using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizLayerGenerator
{
    public class Campo
    {
        public string columna { get; set; }
        public string tipo { get; set; }
        public string length { get; set; }
        public string scale { get; set; }
        public string nullable { get; set; }
        public string pk { get; set; }
        public string fkOriginTable { get; set; }

        public Campo()
        {
            
        }
        public Campo(string columna, string tipo, string pk, string fkOriginTable)
        {
            this.columna = columna;
            this.tipo = tipo;
            this.pk = pk;
            this.fkOriginTable = fkOriginTable;

        }
    }
}
