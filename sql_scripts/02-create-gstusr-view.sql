--create a view that gives overview of the database content
CREATE OR ALTER VIEW gstusr.vwInfoDb
AS
SELECT
    (
        SELECT COUNT(*)FROM supusr.Friends WHERE Seeded = 1
    ) AS nrSeededFriends,
    (
        SELECT COUNT(*)FROM supusr.Friends WHERE Seeded = 0
    ) AS nrUnseededFriends,
    (
        SELECT COUNT(*)FROM supusr.Friends WHERE AddressId IS NOT NULL
    ) AS nrFriendsWithAddress,
    (
        SELECT COUNT(*)FROM supusr.Addresses WHERE Seeded = 1
    ) AS nrSeededAddresses,
    (
        SELECT COUNT(*)FROM supusr.Addresses WHERE Seeded = 0
    ) AS nrUnseededAddresses,
    (
        SELECT COUNT(*)FROM supusr.Pets WHERE Seeded = 1
    ) AS nrSeededPets,
    (
        SELECT COUNT(*)FROM supusr.Pets WHERE Seeded = 0
    ) AS nrUnseededPets,
    (
        SELECT COUNT(*)FROM supusr.Quotes WHERE Seeded = 1
    ) AS nrSeededQuotes,
    (
        SELECT COUNT(*)FROM supusr.Quotes WHERE Seeded = 0
    ) AS nrUnseededQuotes;

GO

CREATE OR ALTER VIEW gstusr.vwInfoFriends
AS
SELECT a.Country,
       a.City,
       COUNT(*) AS NrFriends
FROM supusr.Friends f
    INNER JOIN supusr.Addresses a
        ON f.AddressId = a.AddressId
GROUP BY a.Country,
         a.City WITH ROLLUP;
GO

CREATE OR ALTER VIEW gstusr.vwInfoPets
AS
SELECT a.Country,
       a.City,
       COUNT(p.PetId) AS NrPets
FROM supusr.Friends f
    INNER JOIN supusr.Addresses a
        ON f.AddressId = a.AddressId
    INNER JOIN supusr.Pets p
        ON p.FriendId = f.FriendId
GROUP BY a.Country,
         a.City WITH ROLLUP;
GO

CREATE OR ALTER VIEW gstusr.vwInfoQuotes
AS
SELECT Author,
       COUNT(Quote) AS NrQuotes
FROM supusr.Quotes
GROUP BY Author;
GO

/* used to test run the views
SELECT * FROM gstusr.vwInfoDb;
SELECT * FROM gstusr.vwInfoFriends;
SELECT * FROM gstusr.vwInfoPets;
SELECT * FROM gstusr.vwInfoQuotes;
*/

/* used to cleanup if needed
DROP VIEW IF EXISTS [gstusr].[vwInfoDb]
DROP VIEW IF EXISTS [gstusr].[vwInfoFriends]
DROP VIEW IF EXISTS [gstusr].[vwInfoPets]
DROP VIEW IF EXISTS [gstusr].[vwInfoQuotes]
GO
*/