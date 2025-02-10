using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAEFT9_HSZF_2024251.Application
{
    public interface IReportService
    {
        void SaveAnimalReport(int animalId);
        void SaveSpeciesStatistics();
    }
}
