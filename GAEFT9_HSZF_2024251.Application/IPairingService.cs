using GAEFT9_HSZF_2024251.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAEFT9_HSZF_2024251.Application
{
    public interface IPairingService
    {
        void AddPairing(int motherId, int fatherId, DateTime? pairingDate, string? notes = null);
        IEnumerable<Pairing> GetPairings();
        IEnumerable<Pairing> CheckPregnancyWarnings(int pregnancyDurationDays);
        IEnumerable<Pairing> CheckPregnancyWarnings();
        bool IsPairingAllowed(int motherId, int fatherId);
    }
}

