-- NovaBank Database Reset Script
-- Bu script veritabanını tamamen temizler ve yeniden oluşturur

-- 1. Mevcut veritabanını sil
DROP DATABASE IF EXISTS "NovaBankDb";

-- 2. Yeni veritabanını oluştur
CREATE DATABASE "NovaBankDb";

-- 3. Yeni veritabanına bağlan
\c "NovaBankDb"

-- 4. Tüm tabloları oluştur
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) NOT NULL UNIQUE,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "Email" VARCHAR(100) NOT NULL,
    "FullName" VARCHAR(100),
    "Role" VARCHAR(20) DEFAULT 'Customer',
    "IsActive" BOOLEAN DEFAULT TRUE,
    "VerificationCode" VARCHAR(10),
    "VerificationCodeExpiry" TIMESTAMP,
    "IsVerified" BOOLEAN DEFAULT FALSE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "Customers" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INT NOT NULL,
    "IdentityNumber" VARCHAR(11) NOT NULL UNIQUE,
    "FirstName" VARCHAR(50),
    "LastName" VARCHAR(50),
    "PhoneNumber" VARCHAR(20),
    "Email" VARCHAR(100),
    "Address" TEXT,
    "DateOfBirth" TIMESTAMP,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "Accounts" (
    "Id" SERIAL PRIMARY KEY,
    "CustomerId" INT NOT NULL,
    "AccountNumber" VARCHAR(20) NOT NULL UNIQUE,
    "IBAN" VARCHAR(34) NOT NULL UNIQUE,
    "Balance" DECIMAL(18, 2) DEFAULT 0,
    "CurrencyCode" VARCHAR(3) DEFAULT 'TRY',
    "OpenedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("CustomerId") REFERENCES "Customers"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "Transactions" (
    "Id" SERIAL PRIMARY KEY,
    "AccountId" INT NOT NULL,
    "TransactionType" VARCHAR(20),
    "Amount" DECIMAL(18, 2) NOT NULL,
    "Description" VARCHAR(255),
    "TransactionDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("AccountId") REFERENCES "Accounts"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INT NULL,
    "Action" VARCHAR(100),
    "Details" TEXT,
    "IpAddress" VARCHAR(50),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 5. Test verilerini ekle
-- Password hashes (SHA256):
-- admin123 = 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
-- test123  = ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae
-- demo123  = 85d240b21b6e0f7fce55d73c02e88ae9ffe0df8d71f0eec46b3d53cb7e53e0eb
-- 123456   = 8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92

INSERT INTO "Users" ("Username", "PasswordHash", "Email", "Role", "FullName", "IsActive", "IsVerified", "CreatedAt")
VALUES 
    ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'novabank.com@gmail.com', 'Admin', 'Sistem Yöneticisi', TRUE, TRUE, CURRENT_TIMESTAMP),
    ('test', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'test@novabank.com', 'Customer', 'Test Kullanıcı', TRUE, TRUE, CURRENT_TIMESTAMP),
    ('demo', '85d240b21b6e0f7fce55d73c02e88ae9ffe0df8d71f0eec46b3d53cb7e53e0eb', 'demo@novabank.com', 'Customer', 'Demo Kullanıcı', TRUE, TRUE, CURRENT_TIMESTAMP),
    ('staff', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'staff@novabank.com', 'Staff', 'Banka Personeli', TRUE, TRUE, CURRENT_TIMESTAMP)
ON CONFLICT ("Username") DO NOTHING;

-- Müşteriler
INSERT INTO "Customers" ("UserId", "IdentityNumber", "FirstName", "LastName", "PhoneNumber", "Email", "Address", "DateOfBirth", "CreatedAt")
SELECT 
    u."Id",
    '12345678901',
    'Test',
    'Kullanıcı',
    '05551234567',
    'test@novabank.com',
    'İstanbul, Türkiye',
    '1990-01-15'::TIMESTAMP,
    CURRENT_TIMESTAMP
FROM "Users" u WHERE u."Username" = 'test'
ON CONFLICT ("IdentityNumber") DO NOTHING;

INSERT INTO "Customers" ("UserId", "IdentityNumber", "FirstName", "LastName", "PhoneNumber", "Email", "Address", "DateOfBirth", "CreatedAt")
SELECT 
    u."Id",
    '98765432109',
    'Demo',
    'Kullanıcı',
    '05559876543',
    'demo@novabank.com',
    'Ankara, Türkiye',
    '1985-05-20'::TIMESTAMP,
    CURRENT_TIMESTAMP
FROM "Users" u WHERE u."Username" = 'demo'
ON CONFLICT ("IdentityNumber") DO NOTHING;

-- Hesaplar
INSERT INTO "Accounts" ("CustomerId", "AccountNumber", "IBAN", "Balance", "CurrencyCode", "OpenedDate", "CreatedAt")
SELECT 
    c."Id",
    '1000000001',
    'TR330006200000000001000001',
    50000.00,
    'TRY',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
FROM "Customers" c 
WHERE c."IdentityNumber" = '12345678901'
ON CONFLICT ("AccountNumber") DO NOTHING;

INSERT INTO "Accounts" ("CustomerId", "AccountNumber", "IBAN", "Balance", "CurrencyCode", "OpenedDate", "CreatedAt")
SELECT 
    c."Id",
    '1000000002',
    'TR330006200000000001000002',
    2500.00,
    'USD',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
FROM "Customers" c 
WHERE c."IdentityNumber" = '12345678901'
ON CONFLICT ("AccountNumber") DO NOTHING;

INSERT INTO "Accounts" ("CustomerId", "AccountNumber", "IBAN", "Balance", "CurrencyCode", "OpenedDate", "CreatedAt")
SELECT 
    c."Id",
    '2000000001',
    'TR330006200000000002000001',
    25000.00,
    'TRY',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
FROM "Customers" c 
WHERE c."IdentityNumber" = '98765432109'
ON CONFLICT ("AccountNumber") DO NOTHING;

INSERT INTO "Accounts" ("CustomerId", "AccountNumber", "IBAN", "Balance", "CurrencyCode", "OpenedDate", "CreatedAt")
SELECT 
    c."Id",
    '2000000002',
    'TR330006200000000002000002',
    1000.00,
    'EUR',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
FROM "Customers" c 
WHERE c."IdentityNumber" = '98765432109'
ON CONFLICT ("AccountNumber") DO NOTHING;

-- İşlemler
INSERT INTO "Transactions" ("AccountId", "TransactionType", "Amount", "Description", "TransactionDate", "CreatedAt")
SELECT 
    a."Id",
    'Deposit',
    50000.00,
    'İlk yatırım - Test Hesabı',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
FROM "Accounts" a 
WHERE a."AccountNumber" = '1000000001'
LIMIT 1;

INSERT INTO "Transactions" ("AccountId", "TransactionType", "Amount", "Description", "TransactionDate", "CreatedAt")
SELECT 
    a."Id",
    'Deposit',
    2500.00,
    'USD hesap açılışı',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
FROM "Accounts" a 
WHERE a."AccountNumber" = '1000000002'
LIMIT 1;

INSERT INTO "Transactions" ("AccountId", "TransactionType", "Amount", "Description", "TransactionDate", "CreatedAt")
SELECT 
    a."Id",
    'Deposit',
    25000.00,
    'İlk yatırım - Demo Hesabı',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
FROM "Accounts" a 
WHERE a."AccountNumber" = '2000000001'
LIMIT 1;

-- Audit Log
INSERT INTO "AuditLogs" ("UserId", "Action", "Details", "IpAddress", "CreatedAt")
SELECT 
    u."Id",
    'SystemInit',
    'Veritabanı başlatıldı ve test verileri eklendi.',
    '127.0.0.1',
    CURRENT_TIMESTAMP
FROM "Users" u 
WHERE u."Username" = 'admin'
LIMIT 1;

SELECT 'Veritabanı başarıyla sıfırlandı ve test verileri eklendi!' AS "Durum";

