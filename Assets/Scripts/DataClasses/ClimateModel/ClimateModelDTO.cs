namespace TTT.DataClasses.ClimateModel
{
    public struct ClimateModelInputDTO
    {
        public double currTemperatureCelsius;
        public double currSeaLevelMetres;
        public double currAtmosphericCO2ConcentrationPpm;

        // currently not adjustable due to the way model was trained (trained to predict values 3 months later)
        // public int changeInTimeYears;
    }

    public struct ClimateModelOutputDTO
    {
        public double futureTemperatureCelsius;
        public double futureSeaLevelMetres;
    }
}
