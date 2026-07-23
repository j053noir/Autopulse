-- Seed data for AutoPulse
-- To apply this seed, execute this SQL script against your PostgreSQL instance (e.g., via psql or PgAdmin) 
-- after the Entity Framework migrations have been applied.

-- Clean up existing data to avoid conflicts on repeat runs
TRUNCATE TABLE "Bids" CASCADE;
TRUNCATE TABLE "Auctions" CASCADE;
TRUNCATE TABLE "Vehicles" CASCADE;
TRUNCATE TABLE "UserRefreshToken" CASCADE;
TRUNCATE TABLE "Users" CASCADE;

-- 1. Seed Users (with BCrypt.Net.BCrypt.EnhancedHashPassword hash of "Password123!")
-- Hash: $2a$12$hbwcAGrntcUZZWHoueaJ0OS48JQd2hvFvarfpMYa/87PxJVs51X9C
INSERT INTO "Users" (
    "Id", "UserName", "Email", "IsActive", "PasswordHash", "Permissions", "PreferredPaymentMethod", "CreatedAt", "UpdatedAt", "DeletedAt"
) VALUES
('019056d0-2a81-7000-8b1e-50e50f3c5bca', 'john_auctioneer', 'john@autopulse.com', true, '$2a$12$hbwcAGrntcUZZWHoueaJ0OS48JQd2hvFvarfpMYa/87PxJVs51X9C', ARRAY['auctions:create', 'auctions:close', 'auctions:read', 'telemetry:process', 'telemetry:benchmark'], 'bank_transfer', NOW(), NULL, NULL),
('019056d0-2a81-7000-8b1e-50e50f3c5bcb', 'mary_auctioneer', 'mary@autopulse.com', true, '$2a$12$hbwcAGrntcUZZWHoueaJ0OS48JQd2hvFvarfpMYa/87PxJVs51X9C', ARRAY['auctions:create', 'auctions:close', 'auctions:read', 'telemetry:process', 'telemetry:benchmark'], 'credit_card', NOW(), NULL, NULL),
('019056d0-2a81-7000-8b1e-50e50f3c5bcc', 'alex_bidder', 'alex@gmail.com', true, '$2a$12$hbwcAGrntcUZZWHoueaJ0OS48JQd2hvFvarfpMYa/87PxJVs51X9C', ARRAY['auctions:bid', 'auctions:read', 'auctions:read-bids', 'telemetry:process'], 'credit_card', NOW(), NULL, NULL),
('019056d0-2a81-7000-8b1e-50e50f3c5bcd', 'lucas_bidder', 'lucas@yahoo.com', true, '$2a$12$hbwcAGrntcUZZWHoueaJ0OS48JQd2hvFvarfpMYa/87PxJVs51X9C', ARRAY['auctions:bid', 'auctions:read', 'auctions:read-bids', 'telemetry:process'], 'paypal', NOW(), NULL, NULL),
('019056d0-2a81-7000-8b1e-50e50f3c5bce', 'sophia_bidder', 'sophia@outlook.com', true, '$2a$12$hbwcAGrntcUZZWHoueaJ0OS48JQd2hvFvarfpMYa/87PxJVs51X9C', ARRAY['auctions:bid', 'auctions:read', 'auctions:read-bids', 'telemetry:process'], 'credit_card', NOW(), NULL, NULL),
('019056d0-2a81-7000-8b1e-50e50f3c5bcf', 'admin', 'admin@autopulse.com', true, '$2a$12$hbwcAGrntcUZZWHoueaJ0OS48JQd2hvFvarfpMYa/87PxJVs51X9C', ARRAY['auctions:create', 'auctions:bid', 'auctions:delete', 'auctions:read', 'auctions:close', 'auctions:read-bids', 'auctions:read-all-bids', 'users:read', 'users:update', 'telemetry:process', 'telemetry:benchmark'], 'credit_card', NOW(), NULL, NULL);

-- 2. Seed Vehicles (17-char VINs)
INSERT INTO "Vehicles" (
    "Id", "VIN", "Marquee", "Model", "Year", "Mileage", "CreatedAt", "UpdatedAt", "DeletedAt",
    "Title", "BasePriceAmount", "BasePriceCurrencyCode", "MinimumBidIncrementAmount", "MinimumBidIncrementCurrencyCode", "Category", "DocumentStorageKey"
) VALUES
('019056d0-2a81-7000-8b1e-50e50f3c5bd0', '1HGCR2F83JA000001', 'Honda', 'Accord', 2018, 45000, NOW(), NULL, NULL, '2018 Honda Accord', 15000.00, 'USD', 250.00, 'USD', 'sedan', 'documents/honda-accord-2018.pdf'),
('019056d0-2a81-7000-8b1e-50e50f3c5bd1', '4T1BF1FK5LU000002', 'Toyota', 'Camry', 2020, 28000, NOW(), NULL, NULL, '2020 Toyota Camry', 18000.00, 'USD', 300.00, 'USD', 'sedan', 'documents/toyota-camry-2020.pdf'),
('019056d0-2a81-7000-8b1e-50e50f3c5bd2', '1FA6P8CF0H5000003', 'Ford', 'Mustang GT', 2017, 52000, NOW(), NULL, NULL, '2017 Ford Mustang GT', 22000.00, 'USD', 250.00, 'USD', 'sport', 'documents/ford-mustang-2017.pdf'),
('019056d0-2a81-7000-8b1e-50e50f3c5bd3', 'SALWR2V4XHA000004', 'Land Rover', 'Defender', 2022, 12000, NOW(), NULL, NULL, '2022 Land Rover Defender', 55000.00, 'USD', 500.00, 'USD', 'suv', 'documents/land-rover-defender-2022.pdf'),
('019056d0-2a81-7000-8b1e-50e50f3c5bd4', 'WBA3A5C56KF000005', 'BMW', 'M3', 2019, 34000, NOW(), NULL, NULL, '2019 BMW M3', 48000.00, 'USD', 400.00, 'USD', 'sport', 'documents/bmw-m3-2019.pdf');

-- 3. Seed Auctions (Link to Vehicles and Users)
INSERT INTO "Auctions" (
    "Id", "AuctioneerId", "VehicleId", "StartingPriceAmount", "StartingPriceCurrencyCode", "CurrentPriceAmount", "CurrentPriceCurrencyCode", "EndTime", "IsActive", "WinnerId", "CreatedAt", "UpdatedAt", "DeletedAt"
) VALUES
-- Active Auction 1 (Honda Accord) - Starting $15,000, current bids at $16,500
('019056d0-2a81-7000-8b1e-50e50f3c5be0', '019056d0-2a81-7000-8b1e-50e50f3c5bca', '019056d0-2a81-7000-8b1e-50e50f3c5bd0', 15000.00, 'USD', 16500.00, 'USD', NOW() + INTERVAL '2 days', true, NULL, NOW(), NULL, NULL),
-- Active Auction 2 (Toyota Camry) - Starting $18,000, current bids at $18,000 (no bids yet)
('019056d0-2a81-7000-8b1e-50e50f3c5be1', '019056d0-2a81-7000-8b1e-50e50f3c5bca', '019056d0-2a81-7000-8b1e-50e50f3c5bd1', 18000.00, 'USD', 18000.00, 'USD', NOW() + INTERVAL '4 days', true, NULL, NOW(), NULL, NULL),
-- Active Auction 3 (Land Rover Defender) - Starting $55,000, current bids at $58,200
('019056d0-2a81-7000-8b1e-50e50f3c5be2', '019056d0-2a81-7000-8b1e-50e50f3c5bcb', '019056d0-2a81-7000-8b1e-50e50f3c5bd3', 55000.00, 'USD', 58200.00, 'USD', NOW() + INTERVAL '1 day', true, NULL, NOW(), NULL, NULL),
-- Closed Auction 1 (Ford Mustang) - Starting $22,000, ended with winner Alex ($24,500)
('019056d0-2a81-7000-8b1e-50e50f3c5be3', '019056d0-2a81-7000-8b1e-50e50f3c5bcb', '019056d0-2a81-7000-8b1e-50e50f3c5bd2', 22000.00, 'USD', 24500.00, 'USD', NOW() - INTERVAL '1 day', false, '019056d0-2a81-7000-8b1e-50e50f3c5bcc', NOW() - INTERVAL '3 days', NULL, NULL);

-- 4. Seed Bids
INSERT INTO "Bids" (
    "Id", "AuctionId", "BidderId", "BidAmount", "BidCurrencyCode", "CreatedAt", "UpdatedAt", "DeletedAt"
) VALUES
-- Bids for Honda Accord (Active Auction 1)
('019056d0-2a81-7000-8b1e-50e50f3c5bf0', '019056d0-2a81-7000-8b1e-50e50f3c5be0', '019056d0-2a81-7000-8b1e-50e50f3c5bcc', 15500.00, 'USD', NOW() - INTERVAL '6 hours', NULL, NULL),
('019056d0-2a81-7000-8b1e-50e50f3c5bf1', '019056d0-2a81-7000-8b1e-50e50f3c5be0', '019056d0-2a81-7000-8b1e-50e50f3c5bcd', 16000.00, 'USD', NOW() - INTERVAL '4 hours', NULL, NULL),
('019056d0-2a81-7000-8b1e-50e50f3c5bf2', '019056d0-2a81-7000-8b1e-50e50f3c5be0', '019056d0-2a81-7000-8b1e-50e50f3c5bce', 16500.00, 'USD', NOW() - INTERVAL '2 hours', NULL, NULL),

-- Bids for Land Rover Defender (Active Auction 3)
('019056d0-2a81-7000-8b1e-50e50f3c5bf3', '019056d0-2a81-7000-8b1e-50e50f3c5be2', '019056d0-2a81-7000-8b1e-50e50f3c5bce', 56000.00, 'USD', NOW() - INTERVAL '12 hours', NULL, NULL),
('019056d0-2a81-7000-8b1e-50e50f3c5bf4', '019056d0-2a81-7000-8b1e-50e50f3c5be2', '019056d0-2a81-7000-8b1e-50e50f3c5bcd', 58200.00, 'USD', NOW() - INTERVAL '1 hour', NULL, NULL),

-- Bids for Ford Mustang (Closed Auction 1)
('019056d0-2a81-7000-8b1e-50e50f3c5bf5', '019056d0-2a81-7000-8b1e-50e50f3c5be3', '019056d0-2a81-7000-8b1e-50e50f3c5bcd', 23000.00, 'USD', NOW() - INTERVAL '2 days', NULL, NULL),
('019056d0-2a81-7000-8b1e-50e50f3c5bf6', '019056d0-2a81-7000-8b1e-50e50f3c5be3', '019056d0-2a81-7000-8b1e-50e50f3c5bcc', 24500.00, 'USD', NOW() - INTERVAL '1 day 12 hours', NULL, NULL);
