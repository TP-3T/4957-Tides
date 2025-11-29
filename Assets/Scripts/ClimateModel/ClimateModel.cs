using System;
using System.Collections.Generic;
using TTT.DataClasses.ClimateModel;
using TTT.DataClasses.HexData;

namespace TTT.ClimateModel
{
    public sealed class ClimatePredictionModel
    {
        private readonly PhysicsModel physicsModel;
        private readonly MLModel mlModel;
        private static readonly int SEA_LEVEL_SCALE_FACTOR = 10;

        // 3 months
        private static readonly double NUM_CHANGE_IN_TIME_YEARS = 0.25;

        /// <summary>
        /// Predicts the future temperature and sea level for the next season, from the current season's CO2 concentration (ppm), global mean temperature (deg C), and global mean sea level (m).
        /// </summary>
        /// <param name="inputDTO"></param>
        /// <returns></returns>
        public ClimateModelOutputDTO PredictFutureTempAndSeaLevel(
            ClimateModelInputDTO inputDTO,
            Queue<WorldState> climateModelWorldStatesQueue
        )
        {
            // Scale DOWN the sea level from m to mm by the scale factor(sea level doesn't actually rise as much as the game shows)
            double currSeaLevelMM =
                inputDTO.currSeaLevelMetres / SEA_LEVEL_SCALE_FACTOR;

            ClimateModelInput modelInput = new()
            {
                currTemperatureCelsius = inputDTO.currTemperatureCelsius,
                currSeaLevelMM = currSeaLevelMM,
                currAtmosphericCO2ConcentrationPpm =
                    inputDTO.currAtmosphericCO2ConcentrationPpm,
                changeInTimeYears = NUM_CHANGE_IN_TIME_YEARS,
            };

            // === Future temperature ===

            double futureTemperatureCelsius =
                physicsModel.CalculateFutureTemperature(
                    modelInput.currTemperatureCelsius,
                    modelInput.changeInTimeYears,
                    modelInput.currAtmosphericCO2ConcentrationPpm
                );

            // === Sea level ===

            double futureSeaLevelMM;

            // --- temporary fix - because model can't predict values outside of the range of its training set yet ---

            // if current sea level is within the range of the training dataset
            if (
                (
                    modelInput.currSeaLevelMM
                    >= MLModel.TRAINING_DATASET_GMSL_LOWER_BOUND
                )
                && (
                    modelInput.currSeaLevelMM
                    <= MLModel.TRAINING_DATASET_GMSL_UPPER_BOUND
                )
            )
            {
                futureSeaLevelMM = mlModel.PredictFutureSeaLevel(
                    modelInput,
                    climateModelWorldStatesQueue
                );
            }
            // if sea level is outside of the range of the training dataset, then use the physics model to calculate it
            else
            {
                futureSeaLevelMM = physicsModel.CalculateFutureSeaLevel(
                    modelInput.currSeaLevelMM,
                    modelInput.currTemperatureCelsius,
                    modelInput.changeInTimeYears
                );
            }

            // Scale UP the sea level from mm to m by the scale factor(sea level doesn't actually rise as much as the game shows)
            double futureSeaLevelMetres =
                futureSeaLevelMM * SEA_LEVEL_SCALE_FACTOR;

            ClimateModelOutputDTO outputDTO = new()
            {
                futureTemperatureCelsius = futureTemperatureCelsius,
                futureSeaLevelMetres = futureSeaLevelMetres,
            };

            return outputDTO;
        }
    }
}
