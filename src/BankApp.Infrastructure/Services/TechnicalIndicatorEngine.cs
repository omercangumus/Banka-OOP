using System;
using System.Collections.Generic;
using System.Linq;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Technical Indicator Engine for calculating financial indicators
    /// Native C# implementation for better performance and integration
    /// </summary>
    public class TechnicalIndicatorEngine
    {
        private readonly Dictionary<string, TechnicalIndicatorResult> _indicatorCache;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

        public TechnicalIndicatorEngine()
        {
            _indicatorCache = new Dictionary<string, TechnicalIndicatorResult>();
        }

        /// <summary>
        /// Calculate Simple Moving Average (SMA)
        /// </summary>
        public TechnicalIndicatorResult CalculateSMA(double[] prices, int period)
        {
            string cacheKey = $"SMA_{string.Join(",", prices)}_{period}";
            
            if (_indicatorCache.ContainsKey(cacheKey))
            {
                var cached = _indicatorCache[cacheKey];
                if (DateTime.Now - cached.CalculatedAt < _cacheExpiry)
                {
                    return cached;
                }
            }

            var validation = ValidateInputData(prices, period);
            if (!validation.IsValid)
            {
                return TechnicalIndicatorResult.CreateError(IndicatorType.SMA, validation.ErrorMessage);
            }

            var smaValues = new List<double>();
            
            for (int i = period - 1; i < prices.Length; i++)
            {
                double sum = 0;
                for (int j = i - period + 1; j <= i; j++)
                {
                    sum += prices[j];
                }
                smaValues.Add(sum / period);
            }

            var result = new TechnicalIndicatorResult
            {
                Type = IndicatorType.SMA,
                Values = smaValues.ToArray(),
                Period = period,
                IsValid = true,
                CalculatedAt = DateTime.Now
            };

            _indicatorCache[cacheKey] = result;
            return result;
        }

        /// <summary>
        /// Calculate Exponential Moving Average (EMA)
        /// </summary>
        public TechnicalIndicatorResult CalculateEMA(double[] prices, int period)
        {
            string cacheKey = $"EMA_{string.Join(",", prices)}_{period}";
            
            if (_indicatorCache.ContainsKey(cacheKey))
            {
                var cached = _indicatorCache[cacheKey];
                if (DateTime.Now - cached.CalculatedAt < _cacheExpiry)
                {
                    return cached;
                }
            }

            var validation = ValidateInputData(prices, period);
            if (!validation.IsValid)
            {
                return TechnicalIndicatorResult.CreateError(IndicatorType.EMA, validation.ErrorMessage);
            }

            var emaValues = new List<double>();
            double multiplier = 2.0 / (period + 1);
            
            // First EMA value is SMA
            double sum = 0;
            for (int i = 0; i < period; i++)
            {
                sum += prices[i];
            }
            double ema = sum / period;
            emaValues.Add(ema);

            // Calculate subsequent EMA values
            for (int i = period; i < prices.Length; i++)
            {
                ema = (prices[i] * multiplier) + (ema * (1 - multiplier));
                emaValues.Add(ema);
            }

            var result = new TechnicalIndicatorResult
            {
                Type = IndicatorType.EMA,
                Values = emaValues.ToArray(),
                Period = period,
                IsValid = true,
                CalculatedAt = DateTime.Now
            };

            _indicatorCache[cacheKey] = result;
            return result;
        }

        /// <summary>
        /// Calculate Relative Strength Index (RSI)
        /// </summary>
        public TechnicalIndicatorResult CalculateRSI(double[] prices, int period = 14)
        {
            string cacheKey = $"RSI_{string.Join(",", prices)}_{period}";
            
            if (_indicatorCache.ContainsKey(cacheKey))
            {
                var cached = _indicatorCache[cacheKey];
                if (DateTime.Now - cached.CalculatedAt < _cacheExpiry)
                {
                    return cached;
                }
            }

            var validation = ValidateInputData(prices, period + 1); // RSI needs one extra for price changes
            if (!validation.IsValid)
            {
                return TechnicalIndicatorResult.CreateError(IndicatorType.RSI, validation.ErrorMessage);
            }

            var rsiValues = new List<double>();
            var gains = new List<double>();
            var losses = new List<double>();

            // Calculate price changes
            for (int i = 1; i < prices.Length; i++)
            {
                double change = prices[i] - prices[i - 1];
                gains.Add(change > 0 ? change : 0);
                losses.Add(change < 0 ? Math.Abs(change) : 0);
            }

            if (gains.Count < period)
            {
                return TechnicalIndicatorResult.CreateError(IndicatorType.RSI, "Insufficient data for RSI calculation");
            }

            // Calculate initial average gain and loss
            double avgGain = gains.Take(period).Average();
            double avgLoss = losses.Take(period).Average();

            // Calculate RSI values
            for (int i = period - 1; i < gains.Count; i++)
            {
                if (i > period - 1)
                {
                    avgGain = ((avgGain * (period - 1)) + gains[i]) / period;
                    avgLoss = ((avgLoss * (period - 1)) + losses[i]) / period;
                }

                double rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
                double rsi = 100 - (100 / (1 + rs));
                rsiValues.Add(rsi);
            }

            var result = new TechnicalIndicatorResult
            {
                Type = IndicatorType.RSI,
                Values = rsiValues.ToArray(),
                Period = period,
                IsValid = true,
                CalculatedAt = DateTime.Now,
                AdditionalData = new Dictionary<string, object>
                {
                    ["OverboughtLevel"] = 70.0,
                    ["OversoldLevel"] = 30.0
                }
            };

            _indicatorCache[cacheKey] = result;
            return result;
        }

        /// <summary>
        /// Calculate MACD (Moving Average Convergence Divergence)
        /// </summary>
        public TechnicalIndicatorResult CalculateMACD(double[] prices, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            string cacheKey = $"MACD_{string.Join(",", prices)}_{fastPeriod}_{slowPeriod}_{signalPeriod}";
            
            if (_indicatorCache.ContainsKey(cacheKey))
            {
                var cached = _indicatorCache[cacheKey];
                if (DateTime.Now - cached.CalculatedAt < _cacheExpiry)
                {
                    return cached;
                }
            }

            var validation = ValidateInputData(prices, slowPeriod);
            if (!validation.IsValid)
            {
                return TechnicalIndicatorResult.CreateError(IndicatorType.MACD, validation.ErrorMessage);
            }

            // Calculate fast and slow EMAs
            var fastEMA = CalculateEMA(prices, fastPeriod);
            var slowEMA = CalculateEMA(prices, slowPeriod);

            if (!fastEMA.IsValid || !slowEMA.IsValid)
            {
                return TechnicalIndicatorResult.CreateError(IndicatorType.MACD, "Failed to calculate EMA components");
            }

            // Calculate MACD line (fast EMA - slow EMA)
            var macdLine = new List<double>();
            int startIndex = slowPeriod - fastPeriod;
            
            for (int i = 0; i < slowEMA.Values.Length; i++)
            {
                macdLine.Add(fastEMA.Values[i + startIndex] - slowEMA.Values[i]);
            }

            // Calculate Signal line (EMA of MACD line)
            var signalEMA = CalculateEMA(macdLine.ToArray(), signalPeriod);
            
            // Calculate Histogram (MACD - Signal)
            var histogram = new List<double>();
            int signalStartIndex = signalPeriod - 1;
            
            for (int i = 0; i < signalEMA.Values.Length; i++)
            {
                histogram.Add(macdLine[i + signalStartIndex] - signalEMA.Values[i]);
            }

            var result = new TechnicalIndicatorResult
            {
                Type = IndicatorType.MACD,
                Values = macdLine.ToArray(),
                Period = slowPeriod,
                IsValid = true,
                CalculatedAt = DateTime.Now,
                AdditionalSeries = new Dictionary<string, double[]>
                {
                    ["Signal"] = signalEMA.Values,
                    ["Histogram"] = histogram.ToArray()
                }
            };

            _indicatorCache[cacheKey] = result;
            return result;
        }

        /// <summary>
        /// Calculate Bollinger Bands
        /// </summary>
        public TechnicalIndicatorResult CalculateBollingerBands(double[] prices, int period = 20, double stdDevMultiplier = 2.0)
        {
            string cacheKey = $"BB_{string.Join(",", prices)}_{period}_{stdDevMultiplier}";
            
            if (_indicatorCache.ContainsKey(cacheKey))
            {
                var cached = _indicatorCache[cacheKey];
                if (DateTime.Now - cached.CalculatedAt < _cacheExpiry)
                {
                    return cached;
                }
            }

            var validation = ValidateInputData(prices, period);
            if (!validation.IsValid)
            {
                return TechnicalIndicatorResult.CreateError(IndicatorType.BollingerBands, validation.ErrorMessage);
            }

            var sma = CalculateSMA(prices, period);
            if (!sma.IsValid)
            {
                return TechnicalIndicatorResult.CreateError(IndicatorType.BollingerBands, "Failed to calculate SMA for Bollinger Bands");
            }

            var upperBand = new List<double>();
            var lowerBand = new List<double>();

            for (int i = period - 1; i < prices.Length; i++)
            {
                // Calculate standard deviation for the period
                double sum = 0;
                double mean = sma.Values[i - period + 1];
                
                for (int j = i - period + 1; j <= i; j++)
                {
                    sum += Math.Pow(prices[j] - mean, 2);
                }
                
                double stdDev = Math.Sqrt(sum / period);
                
                upperBand.Add(mean + (stdDevMultiplier * stdDev));
                lowerBand.Add(mean - (stdDevMultiplier * stdDev));
            }

            var result = new TechnicalIndicatorResult
            {
                Type = IndicatorType.BollingerBands,
                Values = sma.Values, // Middle band (SMA)
                Period = period,
                IsValid = true,
                CalculatedAt = DateTime.Now,
                AdditionalSeries = new Dictionary<string, double[]>
                {
                    ["Upper"] = upperBand.ToArray(),
                    ["Lower"] = lowerBand.ToArray()
                }
            };

            _indicatorCache[cacheKey] = result;
            return result;
        }

        /// <summary>
        /// Validate input data for indicator calculations
        /// </summary>
        public ValidationResult ValidateInputData(double[] data, int requiredLength)
        {
            if (data == null || data.Length == 0)
            {
                return ValidationResult.Failure("Input data is null or empty");
            }

            if (data.Length < requiredLength)
            {
                return ValidationResult.Failure($"Insufficient data: {data.Length} points, need {requiredLength}");
            }

            if (data.Any(d => double.IsNaN(d) || double.IsInfinity(d)))
            {
                return ValidationResult.Failure("Data contains invalid values (NaN or Infinity)");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Clear indicator cache
        /// </summary>
        public void ClearCache()
        {
            _indicatorCache.Clear();
        }
    }

    // Supporting classes and enums
    public enum IndicatorType
    {
        SMA,
        EMA,
        RSI,
        MACD,
        BollingerBands
    }

    public class TechnicalIndicatorResult
    {
        public IndicatorType Type { get; set; }
        public double[] Values { get; set; }
        public Dictionary<string, double[]> AdditionalSeries { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; }
        public int Period { get; set; }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CalculatedAt { get; set; }

        public TechnicalIndicatorResult()
        {
            AdditionalSeries = new Dictionary<string, double[]>();
            AdditionalData = new Dictionary<string, object>();
        }

        public static TechnicalIndicatorResult CreateError(IndicatorType type, string errorMessage)
        {
            return new TechnicalIndicatorResult
            {
                Type = type,
                IsValid = false,
                ErrorMessage = errorMessage,
                Values = new double[0],
                CalculatedAt = DateTime.Now
            };
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        public static ValidationResult Failure(string errorMessage)
        {
            return new ValidationResult { IsValid = false, ErrorMessage = errorMessage };
        }
    }
}