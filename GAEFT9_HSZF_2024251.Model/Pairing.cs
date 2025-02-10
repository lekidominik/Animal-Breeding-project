using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAEFT9_HSZF_2024251.Model
{
    public class Pairing
    {
        public int Id { get; set; }
        public int MotherId { get; set; }
        public int FatherId { get; set; }
        public DateTime PairingDate { get; set; }
        public string? Notes { get; set; }

       
        public Animal Mother { get; set; }
        public Animal Father { get; set; }
    }
}
