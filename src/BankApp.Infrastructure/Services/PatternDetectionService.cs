using System;
using System.Collections.Generic;
using System.Linq;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Pattern Detection Service for identifying candlestick patterns and support/resistance levels
    /// </summary>
    public class PatternDetectionService
    {
        private readonly Dictionary<string, List<PatternDetectionResult>> _patternCache;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

        public PatternDetectionService()
        {
            _patternCache = new Dictionary<string, List<PatternDetectionResult>>();
        }

        /// <summary>
        /// Detect Doji candlestick pattern
        /// Doji: Open and close prices are nearly equal, indicating market indecision
        /// </summary>
        public PatternDetectionResult DetectDoji(CandlestickData candle, double threshold = 0.1)
        {
            try
            {
                if (candle == null || candle.High <= candle.Low)
                {
                    return PatternDetectionResult.Invalid("Invalid candle data");
                }

                double bodySize = Math.Abs(candle.Close - candle.Open);
                double totalRange = candle.High - candle.Low;

                if (totalRange == 0)
                {
                    return PatternDetectionResult.Invalid("Zero price range");
                }

                double bodyRatio = bodySize / totalRange;
                
                if (bodyRatio <= threshold)
                {
                    double confidence = Math.Max(0.5, 1.0 - (bodyRatio / threshold));
                    return PatternDetectionResult.Detected(PatternType.Doji, confidence, 
                        $"Doji pattern detected with body ratio {bodyRatio:P2}");
                }

                return PatternDetectionResult.NotDetected();
            }
            catch (Exception ex)
            {
                LogError($"Doji detection error: {ex.Message}");
                return PatternDetectionResult.Error("Pattern detection failed");
            }
        }

        /// <summary>
        /// Detect Hammer candlestick pattern
        /// Hammer: Small body at the top with long lower shadow, potential bullish reversal
        /// </summary>
        public PatternDetectionResult DetectHammer(CandlestickData candle, double bodyThreshold = 0.3, double shadowRatio = 2.0)
        {
            try
            {
                if (candle == null || candle.High <= candle.Low)
                {
                    return PatternDetectionResult.Invalid("Invalid candle data");
                }

                double bodySize = Math.Abs(candle.Close - candle.Open);
                double totalRange = candle.High - candle.Low;
                double lowerShadow = Math.Min(candle.Open, candle.Close) - candle.Low;
                double upperShadow = candle.High - Math.Max(candle.Open, candle.Close);

                if (totalRange == 0)
                {
                    return PatternDetectionResult.Invalid("Zero price range");
                }

                double bodyRatio = bodySize / totalRange;
                
                // Check if body is small enough
                if (bodyRatio > bodyThreshold)
                {
                    return PatternDetectionResult.NotDetected();
                }

                // Check if lower shadow is significantly longer than body
                if (bodySize > 0 && lowerShadow / bodySize >= shadowRatio)
                {
                    // Check if upper shadow is small
                    if (upperShadow <= bodySize)
                    {
                        double confidence = Math.Min(0.95, 0.6 + (lowerShadow / bodySize) * 0.1);
                        return PatternDetectionResult.Detected(PatternType.Hammer, confidence,
                            $"Hammer pattern detected with lower shadow ratio {lowerShadow / bodySize:F1}");
                    }
                }

                return PatternDetectionResult.NotDetected();
            }
            catch (Exception ex)
            {
                LogError($"Hammer detection error: {ex.Message}");
                return PatternDetectionResult.Error("Pattern detection failed");
            }
        }

        /// <summary>
        /// Calculate support and resistance levels based on 20-day high/low
        /// </summary>
        public SupportResistanceResult CalculateSupportResistance(CandlestickData[] data, int period = 20)
        {
            try
            {
                if (data == null || data.Length < period)
                {
                    return SupportResistanceResult.Invalid($"Insufficient data: need {period} candles, got {data?.Length ?? 0}");
                }

                string cacheKey = $"SR_{data.Length}_{period}_{data.Last().Time:yyyyMMdd}";
                
                // Take the last 'period' candles
                var recentCandles = data.TakeLast(period).ToArray();
                
                double resistance = recentCandles.Max(c => c.High);
                double support = recentCandles.Min(c => c.Low);
                
                // Calculate strength based on how many times price touched these levels
                int resistanceTouches = recentCandles.Count(c => Math.Abs(c.High - resistance) / resistance < 0.01);
                int supportTouches = recentCandles.Count(c => Math.Abs(c.Low - support) / support < 0.01);

                return new SupportResistanceResult
                {
                    Support = support,
                    Resistance = resistance,
                    SupportStrength = Math.Min(1.0, supportTouches / 3.0),
                    ResistanceStrength = Math.Min(1.0, resistanceTouches / 3.0),
                    Period = period,
                    IsValid = true,
                    CalculatedAt = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                LogError($"Support/Resistance calculation error: {ex.Message}");
                return SupportResistanceResult.Invalid("Calculation failed");
            }
        }

        /// <summary>
        /// Analyze multiple patterns in candlestick data
        /// </summary>
        public List<PatternDetectionResult> AnalyzePatterns(CandlestickData[] data)
        {
            var results = new List<PatternDetectionResult>();
            
            if (data == null || data.Length == 0)
            {
                return results;
            }

            string cacheKey = $"patterns_{data.Length}_{data.Last().Time:yyyyMMddHHmm}";
            
            if (_patternCache.ContainsKey(cacheKey))
            {
                var cached = _patternCache[cacheKey];
                // Simple cache check - in production, you'd want more sophisticated cache validation
                return cached;
            }

            for (int i = 0; i < data.Length; i++)
            {
                var candle = data[i];
                
                // Detect Doji
                var dojiResult = DetectDoji(candle);
                if (dojiResult.IsDetected)
                {
                    dojiResult.CandleIndex = i;
                    results.Add(dojiResult);
                }

                // Detect Hammer
                var hammerResult = DetectHammer(candle);
                if (hammerResult.IsDetected)
                {
                    hammerResult.CandleIndex = i;
                    results.Add(hammerResult);
                }
            }

            _patternCache[cacheKey] = results;
            return results;
        }

        /// <summary>
        /// Clear pattern cache
        /// </summary>
        public void ClearCache()
        {
            _patternCache.Clear();
        }

        private void LogError(string message)
        {
            // In a real application, you'd log to DEV_LOG.md or your logging system
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] - [PatternDetection] - {message}");
        }
    }

    // Supporting classes and enums
    public enum PatternType
    {
        Doji,
        Hammer,
        ShootingStar,
        Engulfing,
        Harami
    }

    public class PatternDetectionResult
    {
        public PatternType Type { get; set; }
        public bool IsDetected { get; set; }
        public double Confidence { get; set; }
        public string Description { get; set; }
        public int CandleIndex { get; set; }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public static PatternDetectionResult Detected(PatternType type, double confidence, string description)
        {
            return new PatternDetectionResult
            {
                Type = type,
                IsDetected = true,
                Confidence = confidence,
                Description = description,
                IsValid = true
            };
        }

        public static PatternDetectionResult NotDetected()
        {
            return new PatternDetectionResult
            {
                IsDetected = false,
                IsValid = true
            };
        }

        public static PatternDetectionResult Invalid(string errorMessage)
        {
            return new PatternDetectionResult
            {
                IsDetected = false,
                IsValid = false,
                ErrorMessage = errorMessage
            };
        }

        public static PatternDetectionResult Error(string errorMessage)
        {
            return new PatternDetectionResult
            {
                IsDetected = false,
                IsValid = false,
                ErrorMessage = errorMessage
            };
        }
    }

    public class SupportResistanceResult
    {
        public double Support { get; set; }
        public double Resistance { get; set; }
        public double SupportStrength { get; set; }
        public double ResistanceStrength { get; set; }
        public int Period { get; set; }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CalculatedAt { get; set; }

        public static SupportResistanceResult Invalid(string errorMessage)
        {
            return new SupportResistanceResult
            {
                IsValid = false,
                ErrorMessage = errorMessage
            };
        }
    }
}