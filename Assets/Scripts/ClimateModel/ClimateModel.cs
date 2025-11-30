using System;
using System.Collections.Generic;
using TTT.DataClasses.HexData;

namespace TTT.ClimateModel
{
    public sealed class ClimatePredictionModel
    {
        private static readonly PhysicsModel physicsModel = new();
        private static readonly MLModel mlModel = new();

        // Scaling factor for metres to and from millimetres
        private static readonly int SEA_LEVEL_SCALE_FACTOR = 1000;

        // 3 months
        private static readonly double NUM_CHANGE_IN_TIME_YEARS = 0.25;

        private static readonly Queue<WorldState> _climateModelWorldStatesQueue =
            new();

        /// <summary>
        /// Entrypoint method for predicting the future climate values sea level and temperature.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static WorldState PredictFutureClimateDataForNextTurn(
            WorldState state
        )
        {
            // Get new values from climate prediction model
            WorldState futureWorldState = PredictFutureTempAndSeaLevel(state);

            // Update climate states queue
            UpdateWorldStatesQueue(futureWorldState);

            return futureWorldState;
        }

        /// <summary>
        /// Resets the internal world queue for storing historical game data (historical data required to make a prediction).
        /// </summary>
        public static void ResetWorldQueue()
        {
            _climateModelWorldStatesQueue.Clear();

            // Populate queue with historical data (worldstate data before the start of the game the model can use to predict)
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 313.18f,
                    SeaLevel = 14.18079369f,
                    Temp = -22.64326f,
                    Year = 1940,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 313.34f,
                    SeaLevel = 14.35222333f,
                    Temp = -12.24326f,
                    Year = 1941,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 313.34f,
                    SeaLevel = 14.35222333f,
                    Temp = -12.24326f,
                    Year = 1942,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 313.84f,
                    SeaLevel = 14.35732167f,
                    Temp = -16.94326f,
                    Year = 1943,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 313.88f,
                    SeaLevel = 14.18043223f,
                    Temp = -5.54326f,
                    Year = 1944,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 314.63f,
                    SeaLevel = 14.60734743f,
                    Temp = -5.84326f,
                    Year = 1945,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 314.63f,
                    SeaLevel = 14.25638287f,
                    Temp = -16.84326f,
                    Year = 1946,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 314.66f,
                    SeaLevel = 14.4138078f,
                    Temp = -10.64326f,
                    Year = 1947,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 314.88f,
                    SeaLevel = 14.48694413f,
                    Temp = -8.94326f,
                    Year = 1948,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 315.95f,
                    SeaLevel = 14.17366973f,
                    Temp = -1.94326f,
                    Year = 1949,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 315.67f,
                    SeaLevel = 14.36671727f,
                    Temp = 1.35674f,
                    Year = 1950,
                }
            );
        }

        /// <summary>
        /// Predicts the future temperature and sea level for the next season, from the current season's CO2 concentration (ppm), global mean temperature (deg C), and global mean sea level (m).
        /// </summary>
        /// <param name="worldState"></param>
        /// <returns></returns>
        private static WorldState PredictFutureTempAndSeaLevel(
            WorldState worldState
        )
        {
            // Scale DOWN the sea level from m to mm by the scale factor (realistic sea level doesn't rise as much as the game shows)
            double currSeaLevelMM =
                worldState.SeaLevel * SEA_LEVEL_SCALE_FACTOR;

            // Separate data class model input in case we add more model inputs in the future
            ClimateModelInput modelInput = new()
            {
                currTemperatureCelsius = worldState.Temp,
                currSeaLevelMM = currSeaLevelMM,
                currAtmosphericCO2ConcentrationPpm = worldState.Pollution,
                changeInTimeYears = NUM_CHANGE_IN_TIME_YEARS,
            };

            // === Future temperature ===

            float futureTemperatureCelsius =
                physicsModel.CalculateFutureTemperature(
                    modelInput.currTemperatureCelsius,
                    modelInput.changeInTimeYears,
                    modelInput.currAtmosphericCO2ConcentrationPpm
                );

            // === Future Sea level ===

            double futureSeaLevelMM;

            // --- temporary fix - because ml model can't predict values outside of the range of its training set yet ---

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
                // Predict future sea level using the ml model
                futureSeaLevelMM = mlModel.PredictFutureSeaLevel(
                    modelInput,
                    _climateModelWorldStatesQueue
                );
            }
            // if sea level is outside of the range of the training dataset
            else
            {
                // calculate future sea level use the physics model
                futureSeaLevelMM = physicsModel.CalculateFutureSeaLevel(
                    modelInput.currSeaLevelMM,
                    modelInput.currTemperatureCelsius,
                    modelInput.changeInTimeYears
                );
            }

            // Scale UP the sea level from mm to m by the scale factor(sea level doesn't actually rise as much as the game shows)
            float futureSeaLevelMetres =
                (float)futureSeaLevelMM / SEA_LEVEL_SCALE_FACTOR;

            WorldState futureWorldState = new()
            {
                Temp = futureTemperatureCelsius,
                SeaLevel = futureSeaLevelMetres,
                Pollution = worldState.Pollution,
                Year = worldState.Year,
            };

            return futureWorldState;
        }

        private static void UpdateWorldStatesQueue(WorldState newWorldState)
        {
            // Dequeue WorldState from 10 years ago
            _climateModelWorldStatesQueue.Dequeue();

            // Enqueue WorldState for the next year
            _climateModelWorldStatesQueue.Enqueue(newWorldState);
        }
    }
}
