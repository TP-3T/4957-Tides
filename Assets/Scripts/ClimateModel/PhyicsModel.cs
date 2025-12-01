using System;

namespace TTT.ClimateModel
{
    /// <summary>
    /// Climate prediction model class.
    ///
    /// Has a public method to calculate the future global temperature and future global sea level, for any amount of time skip value (the input DTO is in years but can represent anything less than a year as a fractional value).
    ///
    /// ===== Some theory (ignore below if just need inputs and outputs) =====
    ///
    /// This is a physics based, energy balance climate prediction model.
    ///
    /// ASR (absorbed shortwave radiation - from the sun)
    /// OLR (outgoing longwave radiation - from Earth)
    ///
    /// ASR == OLR at climate equilibrium. Earth is constantly trying to achieve this.
    ///
    /// Heat gets trapped in atmosphere when atmosphere decreases its ability to allow radiation through (caused by increase in CO2 concentration and other atmospheric gases).
    ///
    /// As a result OLR decreases.
    ///
    // Energy balance of Earth is achieved when ASR == OLR, global warming occurs when OLR has to increase to be equal to ASR.
    /// </summary>
    public sealed class PhysicsModel
    {
        private static readonly int _NUM_SECONDS_IN_MIN = 60;
        private static readonly int _NUM_SECONDS_IN_HOUR = 60;
        private static readonly int _NUM_HOURS_IN_DAY = 24;
        private static readonly int _NUM_DAYS_IN_YEAR = 365;
        private static readonly int _NUM_SECONDS_IN_YEAR =
            _NUM_SECONDS_IN_MIN
            * _NUM_SECONDS_IN_HOUR
            * _NUM_HOURS_IN_DAY
            * _NUM_DAYS_IN_YEAR;
        private static readonly double _CELSIUS_KELVIN_CONVERSION_VALUE =
            273.15;

        // Empirical constant for CO2 forcing, derived from radiative transfer calculations that is specific to CO2
        private static readonly double _RADIATIVE_FORCING_EMPIRICAL_CONSTANT =
            5.35;

        // 1950 (our pre-industrial) CO2 concentration in ppm
        private static readonly double _1950_CO2_PPM = 309.41f;

        // Pre-industrial temperature in Kelvin
        private static readonly double _PRE_INDUSTRIAL_TEMP_KELVIN = 288.0;

        // Calculate heat capacity of the Earth system

        // --- heat capacity of the ocean ---

        // specific heat of seawater in J/kg/K
        private static readonly int _c_ocean = 3850;

        // average density of seawater in kg/m3
        private static readonly int _AVG_SEAWATER_DENSITY = 1025;

        // depth of water in m (here representative of the mixed layer)
        private static readonly int _DEPTH_OF_WATER_METRES = 70;

        // heat capacity of the ocean (J/K)
        private static readonly int _C_ocean =
            _c_ocean * _AVG_SEAWATER_DENSITY * _DEPTH_OF_WATER_METRES;

        // --- heat capacity of the atmosphere ---

        // specific heat of the atmosphere at constant pressure in J/kg/K
        private static readonly int _c_atmosphere = 1004;

        // weight (pressure) of atmospheric column in Pa
        private static readonly int _ATMOSPHERIC_COL_WEIGHT_Pa = 100000;

        // acceleration due to gravity in m/s^2
        private static readonly double _ACCEL_DUE_TO_GRAVITY_M_PER_S_SQUARED =
            9.81;

        // heat capacity of the atmosphere (J/K)
        private static readonly double C_atm =
            _c_atmosphere
            * (
                _ATMOSPHERIC_COL_WEIGHT_Pa
                / _ACCEL_DUE_TO_GRAVITY_M_PER_S_SQUARED
            );

        // total heat capacity of the earth system (J/K)
        private static readonly double _TOTAL_HEAT_CAPACITY_OF_EARTH_SYSTEM =
            _C_ocean + C_atm;

        private static readonly double _HOT_TEMP_THRESHOLD_K = 300.0;
        private static readonly double _WARM_TEMP_THRESHOLD_K = 240.0;

        // cold threshold not needed, anything below warm threshold considered cold

        // planet albedo, the fraction of reflected radiation relative to the insolation - https://glossarytest.ametsoc.net/wiki/Planetary_albedo
        private static readonly double _NO_ALBEDO_FRACTION = 0.1;
        private static readonly double _HIGH_ALBEDO_FRACTION = 0.7;

        // the total amount of solar radiation received at the earth's surface - https://glossary.ametsoc.org/wiki/Insolation
        private static readonly int _INSOLATION = 340;

        // Net climate feedback parameter
        // The amount of response to a change in global surface temperature (W/m²/K)
        // (to account for water vapour, clouds, and the planet emitting heat)
        // This is an approximation
        private static readonly double _LAMBDA_W_PER_M2_PER_K = 1.1;

        // Empirical sensitivity coefficients derived from IPCC observations
        // Units: mm/year per °C of warming

        // Thermal expansion sensitivity
        private static readonly double _THERMAL_EXPANSION_SENSITIVITY = 1.5;

        // Ice melt sensitivity
        // Includes: glaciers, Greenland ice sheet, Antarctic ice sheet
        private static readonly double _ICE_MELT_SENSITIVITY = 1.5;

        // Combined total sensitivity (thermal + ice)
        private static readonly double _TOTAL_SENSITIVITY =
            _THERMAL_EXPANSION_SENSITIVITY - _ICE_MELT_SENSITIVITY;

        private static readonly double _MIN_TEMP_ANOMALY = 0.0;

        private static readonly double _NO_SEA_LEVEL_RISE_VALUE = 0.0;

        // public ClimateModelOutput CalculateFutureClimateValues(
        //     ClimateModelInput input
        // )
        // {
        //     double changeInTimeSeconds =
        //         input.changeInTimeYears * _NUM_SECONDS_IN_YEAR;

        //     // Calculate future temperature
        //     double futureTempCelsius = CalculateFutureTemperature(
        //         input.currTemperatureCelsius,
        //         changeInTimeSeconds,
        //         input.currAtmosphericCO2ConcentrationPpm
        //     );

        //     // Calculate future sea level
        //     double futureSeaLevelMM = CalculateFutureSeaLevel(
        //         input.currSeaLevelMM,
        //         input.currTemperatureCelsius,
        //         input.changeInTimeYears
        //     );

        //     ClimateModelOutput output = new()
        //     {
        //         futureTemperatureCelsius = futureTempCelsius,
        //         futureSeaLevelMM = futureSeaLevelMM,
        //     };

        //     return output;
        // }

        /// <summary>
        /// Calculate the future value of temperature (Celsius) from the current global average temperature (celcius) and CO2 radiative forcing (Watts / metre^2), as well as the amount of time elapsed in years.
        /// </summary>
        /// <param name="currTempKelvin"></param>
        /// <param name="changeInTimeSeconds"></param>
        /// <param name="currAtmosphericCO2ConcentrationPpm"></param>
        /// <returns>futureTempCelsius a float</returns>
        public float CalculateFutureTemperature(
            double currTemperatureCelsius,
            double changeInTimeYears,
            double currAtmosphericCO2ConcentrationPpm
        )
        {
            double currTempKelvin = CelsiusToKelvin(currTemperatureCelsius);

            double changeInTimeSeconds = YearsToSeconds(changeInTimeYears);

            double CO2RadiativeForcingWattsPerSquareMetre =
                CalculateRadiativeForcing(currAtmosphericCO2ConcentrationPpm);

            double netRadiativeImbalance = CalculateNetRadiativeImbalance(
                currTempKelvin,
                CO2RadiativeForcingWattsPerSquareMetre
            );

            double changeInTemp =
                changeInTimeSeconds
                / _TOTAL_HEAT_CAPACITY_OF_EARTH_SYSTEM
                * netRadiativeImbalance;

            double futureTempKelvin = currTempKelvin + changeInTemp;

            double futureTempCelsius = KelvinToCelsius(futureTempKelvin);

            return (float)futureTempCelsius;
        }

        /// <summary>
        /// Calculates the future sea level (mm) from the future temperature (Kelvin).
        /// Not a real calculation, just for the game.
        /// It uses future temperature since sea level takes a while to show affects from increases in CO2 or temperature.
        /// </summary>
        /// <param name="futureTempKelvin"></param>
        /// <param name="changeInTimeYears"></param>
        /// <returns></returns>
        public double CalculateFutureSeaLevel(
            double currSeaLevelMM,
            double futureTemperatureCelsius,
            double changeInTimeYears
        )
        {
            double currTempKelvin = CelsiusToKelvin(futureTemperatureCelsius);

            // Calculate temperature anomaly relative to pre-industrial baseline
            double tempAnomaly = currTempKelvin - _PRE_INDUSTRIAL_TEMP_KELVIN;

            // No sea level rise if temperature is at or below pre-industrial levels
            if (tempAnomaly <= _MIN_TEMP_ANOMALY)
            {
                return _NO_SEA_LEVEL_RISE_VALUE;
            }

            // Calculate base annual rate of sea level rise
            double annualRate = _TOTAL_SENSITIVITY * tempAnomaly;

            // Calculate total rise over the time period
            double totalSeaLevelRiseMM = annualRate * changeInTimeYears;

            double futureSeaLevelMM = currSeaLevelMM + totalSeaLevelRiseMM;

            return futureSeaLevelMM;
        }

        private static double YearsToSeconds(double years)
        {
            double seconds = years * _NUM_SECONDS_IN_YEAR;
            return seconds;
        }

        /// <summary>
        /// Converts Celsius to Kelvin.
        /// </summary>
        /// <param name="currTemperatureCelsius"></param>
        /// <returns></returns>
        private static double CelsiusToKelvin(double currTemperatureCelsius)
        {
            // Convert C to kelvin
            double currTempKelvin =
                currTemperatureCelsius + _CELSIUS_KELVIN_CONVERSION_VALUE;

            return currTempKelvin;
        }

        /// <summary>
        /// Converts Kelvin to Celsius.
        /// </summary>
        /// <param name="currTemperatureCelsius"></param>
        /// <returns></returns>
        private static double KelvinToCelsius(double currTempKelvin)
        {
            // Convert C to kelvin
            double currTemperatureCelsius =
                currTempKelvin - _CELSIUS_KELVIN_CONVERSION_VALUE;

            return currTemperatureCelsius;
        }

        /// <summary>
        /// Calculates the radiative forcing value (Watts / m^2) based on the current CO2 concentration (ppm).
        /// </summary>
        /// <param name="currCO2ConcentrationPpm">The current CO2 concentration in parts per million.</param>
        /// <returns></returns>
        private static double CalculateRadiativeForcing(
            double currCO2ConcentrationPpm
        )
        {
            // Calculate CO2 radiative forcing  - the amount of change in Earth's energy balance due to CO2 (W / m^2)
            // if positive value, global warming is occuring
            double CO2RadiativeForcingWattsPerSquareMetre =
                _RADIATIVE_FORCING_EMPIRICAL_CONSTANT
                * Math.Log(currCO2ConcentrationPpm / _1950_CO2_PPM);

            return CO2RadiativeForcingWattsPerSquareMetre;
        }

        private static double CalculateNetRadiativeImbalance(
            double currTempKelvin,
            double CO2RadiativeForcingWattsPerSquareMetre
        )
        {
            double changeInASR =
                ASR(currTempKelvin) - ASR(_PRE_INDUSTRIAL_TEMP_KELVIN);

            double changeInTempFromPreindustrial =
                currTempKelvin - _PRE_INDUSTRIAL_TEMP_KELVIN;

            double netRadiativeImbalance =
                changeInASR
                + CO2RadiativeForcingWattsPerSquareMetre
                - (_LAMBDA_W_PER_M2_PER_K * changeInTempFromPreindustrial);

            return netRadiativeImbalance;
        }

        /// <summary>
        /// Calculates the Absorbed Shortwave Radiation - the amount of solar energy absorbed by Earth.
        /// </summary>
        /// <param name="tempKelvin"></param>
        /// <returns>absorbedShortwaveRadiation a double</returns>
        private static double ASR(double tempKelvin)
        {
            // reflected_flux = albedo * insolation
            // Equation for ASR: ASR = insolation - reflected_flux = (1 - albedo) * insolation
            double albedo = EstimateTotalAlbedoFromTemperature(tempKelvin);
            double absorbedShortwaveRadiation = (1 - albedo) * _INSOLATION;

            return absorbedShortwaveRadiation;
        }

        /// <summary>
        /// A rough estimation of the ice albedo (reflectivity of ice / snow) based on the current global surface temperature (ice might not be part of the game but to make the updated temperature more realistic, since solar radiation reflection is large contributer to climate).
        /// </summary>
        /// <returns>
        /// a double representing an estimate of the total albedo based on  temperature
        /// </returns>
        private static double EstimateTotalAlbedoFromTemperature(
            double tempKelvin
        )
        {
            if (tempKelvin >= _HOT_TEMP_THRESHOLD_K)
            {
                // low albedo -> low amount of solar radiation reflection -> hotter surface temp
                return _NO_ALBEDO_FRACTION;
            }
            else if (tempKelvin > _WARM_TEMP_THRESHOLD_K)
            {
                // moderate albedo -> moderate amount of solar radiation reflection -> warm surface temp
                return _NO_ALBEDO_FRACTION
                    + (_HIGH_ALBEDO_FRACTION - _NO_ALBEDO_FRACTION)
                        * Math.Pow(tempKelvin - 300.0, 2)
                        / Math.Pow(240.0 - 300.0, 2);
            }
            else
            {
                // high albedo -> high amount of solar radiation reflection -> colder surface temp
                return _HIGH_ALBEDO_FRACTION;
            }
        }
    }
}
