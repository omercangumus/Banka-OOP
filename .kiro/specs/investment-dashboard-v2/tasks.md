# Implementation Plan: Investment Dashboard V2 - Technical Analysis Suite

## Overview

This implementation plan transforms the basic Investment Dashboard into a comprehensive technical analysis platform. The approach follows an incremental development strategy, building core infrastructure first, then expanding the dashboard grid, implementing analysis mode, and finally adding advanced features like pattern detection and technical indicators.

## Tasks

- [x] 1. Infrastructure Enhancement and Core Services
  - Extend FinnhubService with candlestick data support and enhanced caching
  - Create technical indicator calculation engine with native C# implementations
  - Set up data validation and error handling framework
  - _Requirements: 4.3, 11.1, 11.2, 11.3, 11.4, 12.1_

- [x]* 1.1 Write property test for Finnhub data caching
  - **Property 36: Candlestick Data Caching**
  - **Validates: Requirements 12.1**

- [x]* 1.2 Write property test for data validation
  - **Property 31: Indicator Data Validation**
  - **Validates: Requirements 11.1**

- [ ] 2. Technical Indicator Engine Implementation
  - [ ] 2.1 Implement core mathematical indicators (SMA, EMA, RSI, MACD, Bollinger Bands)
    - Create TechnicalIndicatorEngine class with calculation methods
    - Add input validation and error handling for insufficient data
    - Implement caching for calculated indicator values
    - _Requirements: 5.1, 5.2, 6.1, 7.1, 8.1, 11.1, 11.2_

  - [ ]* 2.2 Write property test for SMA/EMA calculations
    - **Property 12: SMA Toggle Behavior**
    - **Property 13: EMA Toggle Behavior**
    - **Validates: Requirements 5.3, 5.4**

  - [ ]* 2.3 Write property test for RSI calculation and warnings
    - **Property 18: RSI Insufficient Data Warning**
    - **Validates: Requirements 7.5**

  - [ ]* 2.4 Write unit tests for indicator mathematical accuracy
    - Test known input/output pairs for all indicators
    - Test edge cases and boundary conditions
    - _Requirements: 5.1, 5.2, 6.1, 7.1, 8.1_

- [ ] 3. Pattern Detection Service Implementation
  - [ ] 3.1 Create pattern detection algorithms for Doji and Hammer patterns
    - Implement PatternDetectionService class
    - Add algorithmic detection for candlestick patterns
    - Create support/resistance level calculation (20-day high/low)
    - _Requirements: 9.1, 9.2, 10.1, 10.2_

  - [ ]* 3.2 Write property test for Doji pattern detection
    - **Property 22: Doji Pattern Detection**
    - **Validates: Requirements 9.1**

  - [ ]* 3.3 Write property test for Hammer pattern detection
    - **Property 23: Hammer Pattern Detection**
    - **Validates: Requirements 9.2**

  - [ ]* 3.4 Write property test for support/resistance calculation
    - **Property 27: Resistance Level Calculation**
    - **Property 28: Support Level Calculation**
    - **Validates: Requirements 10.1, 10.2**

- [ ] 4. Enhanced Dashboard Grid Development
  - [ ] 4.1 Refactor main dashboard to support 8-12 assets
    - Modify InvestmentDashboard to use dynamic grid layout
    - Configure asset list with AAPL, TSLA, BTC-USD, ETH-USD, XAU, EUR/USD, GBP/USD, AMZN, GOOGL
    - Implement responsive 3-column grid with scrollable rows
    - _Requirements: 1.1, 1.2, 1.4_

  - [ ]* 4.2 Write property test for dashboard asset display
    - **Property 1: Dashboard Asset Display Consistency**
    - **Validates: Requirements 1.1**

  - [ ] 4.3 Create enhanced AssetCardControl components
    - Design asset cards with current price, percentage change, and mini sparkline
    - Implement color-coded price change indicators (green ▲, red ▼)
    - Add click handlers to open Analysis Mode
    - _Requirements: 2.1, 2.2, 2.3, 2.5_

  - [ ]* 4.4 Write property test for price change visual feedback
    - **Property 5: Price Change Visual Feedback - Positive**
    - **Property 6: Price Change Visual Feedback - Negative**
    - **Validates: Requirements 2.2, 2.3**

  - [ ]* 4.5 Write property test for asset card navigation
    - **Property 8: Analysis Mode Navigation**
    - **Validates: Requirements 2.5**

- [ ] 5. Checkpoint - Core Infrastructure Complete
  - Ensure all tests pass, verify dashboard displays multiple assets correctly
  - Test error handling and data validation mechanisms
  - Ask the user if questions arise about core functionality

- [ ] 6. Chart Integration and WebBrowser Setup
  - [ ] 6.1 Set up WebBrowser control with lightweight-charts integration
    - Create HTML template with TradingView lightweight-charts library
    - Implement ChartBridge class for C#-JavaScript communication
    - Set up chart initialization and data binding methods
    - _Requirements: 4.1, 4.2, 4.5_

  - [ ]* 6.2 Write property test for data format transformation
    - **Property 11: Data Format Transformation**
    - **Validates: Requirements 4.5**

  - [ ] 6.3 Create Analysis Mode form with chart areas
    - Design AnalysisModeForm with 70% main chart and 30% sub-chart areas
    - Add indicator toolbar with toggle buttons
    - Implement back navigation to dashboard
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

  - [ ]* 6.4 Write property test for back navigation
    - **Property 9: Back Navigation Consistency**
    - **Validates: Requirements 3.5**

- [ ] 7. Candlestick Chart Implementation
  - [ ] 7.1 Implement OHLC candlestick visualization
    - Configure lightweight-charts for candlestick display
    - Map Finnhub candlestick data to chart format
    - Add zoom and scroll interaction support
    - _Requirements: 4.2, 4.4, 4.5_

  - [ ]* 7.2 Write property test for OHLC data completeness
    - **Property 10: OHLC Data Completeness**
    - **Validates: Requirements 4.2**

  - [ ] 7.3 Implement real-time data updates for charts
    - Add automatic chart updates when new data arrives
    - Implement efficient data streaming to avoid full redraws
    - _Requirements: 6.5, 10.5, 12.5_

  - [ ]* 7.4 Write property test for dynamic updates
    - **Property 16: Bollinger Bands Dynamic Updates**
    - **Property 30: Support/Resistance Dynamic Updates**
    - **Validates: Requirements 6.5, 10.5**

- [ ] 8. Technical Indicator Overlays Implementation
  - [ ] 8.1 Implement SMA and EMA overlays on main chart
    - Add toggle functionality for SMA/EMA display
    - Ensure visual distinction between different moving averages
    - Connect indicator calculations to chart rendering
    - _Requirements: 5.3, 5.4, 5.5_

  - [ ]* 8.2 Write property test for moving average visual distinction
    - **Property 14: Moving Average Visual Distinction**
    - **Validates: Requirements 5.5**

  - [ ] 8.3 Implement Bollinger Bands overlay
    - Add three-line Bollinger Bands display with shaded area
    - Implement toggle functionality for Bollinger Bands
    - Ensure middle band visual distinction
    - _Requirements: 6.2, 6.3, 6.4_

  - [ ]* 8.4 Write property test for Bollinger Bands toggle
    - **Property 15: Bollinger Bands Toggle Behavior**
    - **Validates: Requirements 6.2**

  - [ ] 8.5 Implement support and resistance level display
    - Add horizontal lines for 20-day high/low levels
    - Implement toggle functionality with distinct colors
    - _Requirements: 10.3, 10.4_

  - [ ]* 8.6 Write property test for support/resistance toggle
    - **Property 29: Support/Resistance Toggle Behavior**
    - **Validates: Requirements 10.3**

- [ ] 9. Sub-Chart Implementation (RSI and MACD)
  - [ ] 9.1 Implement RSI sub-chart display
    - Create RSI chart in 30% height sub-chart area
    - Add horizontal reference lines at 30 (oversold) and 70 (overbought)
    - Implement conditional color changes for RSI line
    - _Requirements: 7.2, 7.3, 7.4_

  - [ ]* 9.2 Write property test for RSI color behavior
    - **Property 17: RSI Color Conditional Behavior**
    - **Validates: Requirements 7.4**

  - [ ] 9.3 Implement MACD sub-chart display
    - Create MACD chart with MACD line, Signal line, and Histogram
    - Implement color-coded histogram (green positive, red negative)
    - Add crossover point highlighting for signal identification
    - _Requirements: 8.3, 8.4, 8.5_

  - [ ]* 9.4 Write property test for MACD histogram coloring
    - **Property 20: MACD Histogram Color Coding**
    - **Validates: Requirements 8.4**

  - [ ]* 9.5 Write property test for MACD crossover highlighting
    - **Property 21: MACD Crossover Highlighting**
    - **Validates: Requirements 8.5**

  - [ ] 9.6 Implement RSI/MACD toggle functionality
    - Add toggle between RSI and MACD in sub-chart area
    - Ensure only one indicator displays at a time
    - _Requirements: 8.2_

  - [ ]* 9.7 Write property test for sub-chart toggle
    - **Property 19: MACD Sub-Chart Toggle**
    - **Validates: Requirements 8.2**

- [ ] 10. Checkpoint - Chart and Indicators Complete
  - Ensure all charts render correctly with technical indicators
  - Test toggle functionality for all indicators
  - Verify real-time updates work properly
  - Ask the user if questions arise about charting functionality

- [ ] 11. Pattern Detection and Visual Markers
  - [ ] 11.1 Implement pattern detection integration with charts
    - Connect pattern detection service to chart display
    - Add visual markers for detected Doji and Hammer patterns
    - Implement real-time pattern detection on new candle updates
    - _Requirements: 9.3, 9.5_

  - [ ]* 11.2 Write property test for pattern visual marking
    - **Property 24: Pattern Visual Marking**
    - **Validates: Requirements 9.3**

  - [ ]* 11.3 Write property test for real-time pattern detection
    - **Property 26: Real-time Pattern Detection**
    - **Validates: Requirements 9.5**

  - [ ] 11.4 Implement interactive pattern markers
    - Make pattern markers clickable to show detailed information
    - Create pattern information popup or tooltip
    - _Requirements: 9.4_

  - [ ]* 11.5 Write property test for pattern marker interactivity
    - **Property 25: Pattern Marker Interactivity**
    - **Validates: Requirements 9.4**

- [ ] 12. Error Handling and Resilience Implementation
  - [ ] 12.1 Implement comprehensive error handling
    - Add error isolation for individual asset failures
    - Implement graceful NaN result handling for indicators
    - Create fallback data mechanisms for network errors
    - _Requirements: 1.5, 11.4, 11.5_

  - [ ]* 12.2 Write property test for error isolation
    - **Property 3: Error Isolation**
    - **Validates: Requirements 1.5**

  - [ ]* 12.3 Write property test for NaN handling
    - **Property 34: NaN Result Handling**
    - **Validates: Requirements 11.4**

  - [ ]* 12.4 Write property test for network error resilience
    - **Property 35: Network Error Resilience**
    - **Validates: Requirements 11.5**

  - [ ] 12.5 Implement enhanced logging to DEV_LOG.md
    - Add structured error logging with timestamps and context
    - Create error categorization and severity levels
    - _Requirements: 11.3_

  - [ ]* 12.6 Write property test for API error logging
    - **Property 33: API Error Logging**
    - **Validates: Requirements 11.3**

- [ ] 13. Performance Optimization and Caching
  - [ ] 13.1 Implement advanced caching strategies
    - Add intelligent cache management for different data types
    - Implement cache-first loading for asset switching
    - Add data preloading for adjacent time periods
    - _Requirements: 12.4, 12.5_

  - [ ]* 13.2 Write property test for asset switching cache strategy
    - **Property 39: Asset Switching Cache Strategy**
    - **Validates: Requirements 12.5**

  - [ ]* 13.3 Write property test for data preloading
    - **Property 38: Data Preloading Behavior**
    - **Validates: Requirements 12.4**

  - [ ] 13.4 Implement asynchronous processing
    - Make all indicator calculations asynchronous
    - Ensure UI thread remains responsive during heavy calculations
    - _Requirements: 12.3_

  - [ ]* 13.5 Write property test for asynchronous calculations
    - **Property 37: Asynchronous Indicator Calculations**
    - **Validates: Requirements 12.3**

- [ ] 14. Integration and Final Wiring
  - [ ] 14.1 Connect all components and test end-to-end functionality
    - Wire dashboard grid to analysis mode navigation
    - Ensure data flows correctly from Finnhub through indicators to charts
    - Test complete user workflows from dashboard to detailed analysis
    - _Requirements: 1.3, 2.4, 2.5_

  - [ ]* 14.2 Write property test for data fetching completeness
    - **Property 2: Data Fetching Completeness**
    - **Validates: Requirements 1.3**

  - [ ]* 14.3 Write property test for sparkline time period consistency
    - **Property 7: Sparkline Time Period Consistency**
    - **Validates: Requirements 2.4**

  - [ ] 14.4 Implement final UI polish and user experience enhancements
    - Add loading states and progress indicators
    - Implement smooth transitions between views
    - Add keyboard shortcuts and accessibility features
    - _Requirements: 2.1, 3.1, 3.2, 3.3, 3.4_

  - [ ]* 14.5 Write integration tests for complete user workflows
    - Test dashboard to analysis mode navigation
    - Test indicator toggling and chart interactions
    - Test error scenarios and recovery mechanisms
    - _Requirements: Multiple workflow requirements_

- [ ] 15. Final Checkpoint - Complete System Validation
  - Ensure all tests pass including property-based tests
  - Verify performance meets requirements (smooth interactions, responsive UI)
  - Test with real market data and various market conditions
  - Ask the user if questions arise about final system behavior

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP development
- Each task references specific requirements for traceability
- Property tests validate universal correctness properties across all inputs
- Unit tests validate specific examples, edge cases, and integration points
- Checkpoints ensure incremental validation and user feedback opportunities
- The implementation follows a layered approach: infrastructure → dashboard → analysis → advanced features
- All technical indicator calculations are implemented natively in C# for better performance and integration
- Chart integration uses WebBrowser control with TradingView lightweight-charts for professional financial visualization