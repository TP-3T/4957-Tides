namespace TTT.DataClasses.ClimateModel
{
    public struct ClimateModelInputDTO
    {
        public double currTemperatureCelsius;
        public double currSeaLevelMetres;
        public double currAtmosphericCO2ConcentrationPpm;
        public double changeInTimeYears;
    }

    public struct ClimateModelOutputDTO
    {
        public double futureTemperatureCelsius;
        public double futureSeaLevelMetres;
    }
}
