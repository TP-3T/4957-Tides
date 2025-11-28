namespace TTT.ClimateModel
{
    // Difference from these input/output objects and the dto is that this contains changeInTimeYears as input param, and sea level is in MM
    public struct ClimateModelInput
    {
        public double currTemperatureCelsius;
        public double currSeaLevelMM;
        public double currAtmosphericCO2ConcentrationPpm;

        public double changeInTimeYears;
    }

    public struct ClimateModelOutput
    {
        public double futureTemperatureCelsius;
        public double futureSeaLevelMM;
    }
}
