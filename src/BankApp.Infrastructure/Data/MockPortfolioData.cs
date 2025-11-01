using System;
using System.Collections.Generic;
using BankApp.Infrastructure.Services; // For AssetType enum

namespace BankApp.Infrastructure.Data
{
    /// <summary>
    /// Mock user portfolio data - Replace with real database queries later
    /// </summary>
    public static class MockPortfolioData
    {
        public static List<PortfolioHolding> GetUserHoldings()
        {
            return new List<PortfolioHolding>
            {
                new PortfolioHolding
                {
                    AssetName = "THYAO",
                    AssetType = AssetType.Stock,
                    Symbol = "THYAO.IS",
                    Quantity = 500,
                    AverageCost = 250.40m,
                    PurchaseDate = DateTime.Now.AddMonths(-6)
                },
                new PortfolioHolding
                {
                    AssetName = "BIST 100",
                    AssetType = AssetType.Index,
                    Symbol = "XU100.IS",
                    Quantity = 1,
                    AverageCost = 9100m,
                    PurchaseDate = DateTime.Now.AddMonths(-3)
                },
                new PortfolioHolding
                {
                    AssetName = "Bitcoin",
                    AssetType = AssetType.Crypto,
                    Symbol = "BTC",
                    Quantity = 0.5m,
                    AverageCost = 600000m,
                    PurchaseDate = DateTime.Now.AddYears(-1)
                },
                new PortfolioHolding
                {
                    AssetName = "Ethereum",
                    AssetType = AssetType.Crypto,
                    Symbol = "ETH",
                    Quantity = 3m,
                    AverageCost = 80000m,
                    PurchaseDate = DateTime.Now.AddMonths(-8)
                },
                new PortfolioHolding
                {
                    AssetName = "Gram Altın",
                    AssetType = AssetType.Gold,
                    Symbol = "GOLD",
                    Quantity = 50,
                    AverageCost = 2100m,
                    PurchaseDate = DateTime.Now.AddMonths(-12)
                },
                new PortfolioHolding
                {
                    AssetName = "USD/TRY",
                    AssetType = AssetType.Forex,
                    Symbol = "USD-TRY",
                    Quantity = 1000,
                    AverageCost = 32.50m,
                    PurchaseDate = DateTime.Now.AddMonths(-10)
                },
                new PortfolioHolding
                {
                    AssetName = "Nakit (TRY)",
                    AssetType = AssetType.Cash,
                    Symbol = "TRY",
                    Quantity = 50000,
                    AverageCost = 1m,
                    PurchaseDate = DateTime.Now
                },
                new PortfolioHolding
                {
                    AssetName = "BES Bireysel Emeklilik",
                    AssetType = AssetType.Pension,
                    Symbol = "BES",
                    Quantity = 1,
                    AverageCost = 120000m,
                    PurchaseDate = DateTime.Now.AddYears(-2)
                }
            };
        }

        public static decimal GetBESContribution()
        {
            return 120000m; // Total BES contribution so far
        }

        public static decimal GetBESMonthlyPayment()
        {
            return 2500m; // Monthly BES payment
        }

        public static int GetBESYears()
        {
            return 2; // Years in BES
        }
    }

    public class PortfolioHolding
    {
        public string AssetName { get; set; } = string.Empty;
        public AssetType AssetType { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal AverageCost { get; set; }
        public DateTime PurchaseDate { get; set; }
        
        // Calculated fields (populated by PortfolioService)
        public decimal CurrentPrice { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal ProfitLoss { get; set; }
        public decimal ProfitLossPercent { get; set; }
    }

    }
