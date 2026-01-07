-- Schema Fix Migration
-- Eksik column'ları ekle

-- 1. Accounts tablosuna AccountType ekle
ALTER TABLE "Accounts" 
ADD COLUMN IF NOT EXISTS "AccountType" VARCHAR(20) DEFAULT 'Checking';

-- 2. Loans tablosunu oluştur (eğer yoksa)
CREATE TABLE IF NOT EXISTS "Loans" (
    "Id" SERIAL PRIMARY KEY,
    "CustomerId" INT NOT NULL,
    "Amount" DECIMAL(18, 2) NOT NULL,
    "TermMonths" INT NOT NULL,
    "InterestRate" DECIMAL(5, 2) NOT NULL,
    "Status" VARCHAR(20) DEFAULT 'Pending',
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("CustomerId") REFERENCES "Customers"("Id") ON DELETE CASCADE
);

-- 3. Portfolios tablosunu oluştur (eğer yoksa)
CREATE TABLE IF NOT EXISTS "Portfolios" (
    "Id" SERIAL PRIMARY KEY,
    "CustomerId" INT NOT NULL,
    "StockSymbol" VARCHAR(10) NOT NULL,
    "Quantity" DECIMAL(18, 4) NOT NULL,
    "AveragePrice" DECIMAL(18, 4) NOT NULL,
    "CurrentPrice" DECIMAL(18, 4) DEFAULT 0,
    "TotalCost" DECIMAL(18, 2) DEFAULT 0,
    "TotalValue" DECIMAL(18, 2) DEFAULT 0,
    "ProfitLoss" DECIMAL(18, 2) DEFAULT 0,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("CustomerId") REFERENCES "Customers"("Id") ON DELETE CASCADE
);

-- 4. CustomerPortfolios tablosunu oluştur (eğer yoksa)
CREATE TABLE IF NOT EXISTS "CustomerPortfolios" (
    "Id" SERIAL PRIMARY KEY,
    "CustomerId" INT NOT NULL,
    "TotalValue" DECIMAL(18, 2) DEFAULT 0,
    "TotalCost" DECIMAL(18, 2) DEFAULT 0,
    "ProfitLoss" DECIMAL(18, 2) DEFAULT 0,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("CustomerId") REFERENCES "Customers"("Id") ON DELETE CASCADE
);

SELECT 'Schema migration completed successfully!' AS "Status";
