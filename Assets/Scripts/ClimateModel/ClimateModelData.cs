using System;

namespace TTT.ClimateModel
{
    public struct ClimateModelInput
    {
        public double currTemperatureCelsius;
        public double currSeaLevelMM;
        public double currAtmosphericCO2ConcentrationPpm;

        public double changeInTimeYears;
    }
}
