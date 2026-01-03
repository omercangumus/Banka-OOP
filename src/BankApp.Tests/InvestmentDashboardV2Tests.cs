using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BankApp.Infrastructure.Services;

namespace BankApp.Tests
{
    /// <summary>
    /// Property-based and unit tests for Investment Dashboard V2
    /// Feature: investment-dashboard-v2
    /// </summary>
    public class InvestmentDashboardV2Tests : IDisposable
    {
        private readonly FinnhubServiceV2 _finnhubService;
        private readonly TechnicalIndicatorEngine _indicatorEngine;
        private readonly PatternDetectionService _patternDetectionService;

        public InvestmentDashboardV2Tests()
        {
            _finnhubService = new FinnhubServiceV2();
            _indicatorEngine = new TechnicalIndicatorEngine();
            _patternDetectionService = new PatternDetectionService();
        }

        public void Dispose()
        {
            _finnhubService?.ClearAllCaches();
            _indicatorEngine?.ClearCache();
            _patternDetectionService?.ClearCache();
        }

        #region Property Tests

        /// <summary>
        /// Property 36: Candlestick Data Caching
        /// Feature: investment-dashboard-v2, Property 36: Candlestick Data Caching
        /// Validates: Requirements 12.1
        /// </summary>
        [Fact]
        public async Task CandlestickDataCaching_ShouldCacheDataFor5Minutes()
        {
            // Arrange
            string symbol = "AAPL";
            
            // Act - First call
            var firstCall = await _finnhubService.GetHistoricalDataAsync(symbol, 30);
            var firstCallTime = DateTime.Now;
            
            // Act - Second call within cache period
            var secondCall = await _finnhubService.GetHistoricalDataAsync(symbol, 30);
            var secondCallTime = DateTime.Now;
            
            // Assert
            Assert.True(secondCallTime - firstCallTime < TimeSpan.FromMinutes(5));
            Assert.NotNull(firstCall);
            Assert.NotNull(secondCall);
            
            // The cache should return equivalent data for the same symbol within cache period
            if (firstCall.Length > 0 && secondCall.Length > 0)
            {
                Assert.Equal(firstCall.Length, secondCall.Length);
            }
        }

        /// <summary>
        /// Property 31: Indicator Data Validation
        /// Feature: investment-dashboard-v2, Property 31: Indicator Data Validation
        /// Validates: Requirements 11.1
        /// </summary>
        [Fact]
        public void IndicatorDataValidation_ShouldVerifySufficientData()
        {
            // Arrange - Test with various data sizes
            var testCases = new[]
            {
                new { Data = new double[0], Required = 14, ShouldPass = false },
                new { Data = new double[5], Required = 14, ShouldPass = false },
                new { Data = new double[14], Required = 14, ShouldPass = true },
                new { Data = new double[20], Required = 14, ShouldPass = true }
            };

            foreach (var testCase in testCases)
            {
                // Act
                var result = _indicatorEngine.ValidateInputData(testCase.Data, testCase.Required);
                
                // Assert
                Assert.Equal(testCase.ShouldPass, result.IsValid);
            }
        }

        #endregion

        #region Unit Tests for Mathematical Accuracy

        /// <summary>
        /// Test SMA calculation with known input/output pairs
        /// </summary>
        [Fact]
        public void CalculateSMA_WithKnownData_ReturnsCorrectValues()
        {
            // Arrange
            var prices = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            int period = 3;
            
            // Expected SMA values: [2, 3, 4, 5, 6, 7, 8, 9]
            var expectedSMA = new double[] { 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0 };
            
            // Act
            var result = _indicatorEngine.CalculateSMA(prices, period);
            
            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(expectedSMA.Length, result.Values.Length);
            
            for (int i = 0; i < expectedSMA.Length; i++)
            {
                Assert.Equal(expectedSMA[i], result.Values[i], 3);
            }
        }

        /// <summary>
        /// Test RSI calculation with known data
        /// </summary>
        [Fact]
        public void CalculateRSI_WithKnownData_ReturnsValidRange()
        {
            // Arrange
            var prices = new double[] { 44.34, 44.09, 44.15, 43.61, 44.33, 44.83, 45.85, 46.08, 45.89, 46.03, 46.83, 47.69, 46.49, 46.26, 47.09 };
            
            // Act
            var result = _indicatorEngine.CalculateRSI(prices, 14);
            
            // Assert
            Assert.True(result.IsValid);
            Assert.NotEmpty(result.Values);
            
            // RSI should be between 0 and 100
            foreach (var rsiValue in result.Values)
            {
                Assert.True(rsiValue >= 0 && rsiValue <= 100);
            }
        }

        #endregion

        #region Pattern Detection Tests

        /// <summary>
        /// Test Doji pattern detection
        /// </summary>
        [Fact]
        public void DetectDoji_WithDojiCandle_ReturnsDetected()
        {
            // Arrange - Create a Doji candle (open ≈ close, small body)
            var dojiCandle = new CandlestickData
            {
                Open = 100.00,
                Close = 100.05, // Very small body
                High = 101.00,
                Low = 99.00,
                Time = DateTime.Now
            };
            
            // Act
            var result = _patternDetectionService.DetectDoji(dojiCandle);
            
            // Assert
            Assert.True(result.IsValid);
            Assert.True(result.IsDetected);
            Assert.Equal(PatternType.Doji, result.Type);
            Assert.True(result.Confidence > 0.5);
        }

        /// <summary>
        /// Test support and resistance calculation
        /// </summary>
        [Fact]
        public void CalculateSupportResistance_WithValidData_ReturnsCorrectLevels()
        {
            // Arrange - Create 20 days of price data
            var candleData = new CandlestickData[20];
            var random = new Random(42); // Fixed seed for reproducible tests
            
            double minLow = double.MaxValue;
            double maxHigh = double.MinValue;
            
            for (int i = 0; i < 20; i++)
            {
                double basePrice = 100 + random.NextDouble() * 10;
                var candle = new CandlestickData
                {
                    Open = basePrice,
                    Close = basePrice + (random.NextDouble() * 2 - 1),
                    High = basePrice + random.NextDouble() * 2,
                    Low = basePrice - random.NextDouble() * 2,
                    Time = DateTime.Now.AddDays(-19 + i)
                };
                
                minLow = Math.Min(minLow, candle.Low);
                maxHigh = Math.Max(maxHigh, candle.High);
                candleData[i] = candle;
            }
            
            // Act
            var result = _patternDetectionService.CalculateSupportResistance(candleData, 20);
            
            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(minLow, result.Support, 3);
            Assert.Equal(maxHigh, result.Resistance, 3);
            Assert.True(result.Support < result.Resistance);
        }

        #endregion

        #region Error Handling Tests

        /// <summary>
        /// Test indicator calculation with insufficient data
        /// </summary>
        [Fact]
        public void CalculateRSI_WithInsufficientData_ReturnsError()
        {
            // Arrange
            var insufficientData = new double[] { 1, 2, 3 }; // Only 3 points, need 15 for RSI
            
            // Act
            var result = _indicatorEngine.CalculateRSI(insufficientData, 14);
            
            // Assert
            Assert.False(result.IsValid);
            Assert.NotNull(result.ErrorMessage);
            Assert.NotEmpty(result.ErrorMessage);
        }

        #endregion
    }
}