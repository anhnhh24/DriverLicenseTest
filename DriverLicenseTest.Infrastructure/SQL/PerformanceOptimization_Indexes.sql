-- =============================================
-- PERFORMANCE OPTIMIZATION INDEXES
-- Driver License Test Database
-- =============================================

USE [DriverLicenseTestDB];
GO

-- =============================================
-- 1. QUESTIONS TABLE INDEXES
-- =============================================

-- Index for QuestionNumber (already exists as UNIQUE, check if it's there)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Questions_QuestionNumber' AND object_id = OBJECT_ID('Questions'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_Questions_QuestionNumber
    ON Questions(QuestionNumber);
    PRINT 'Created index: IX_Questions_QuestionNumber';
END
ELSE
    PRINT 'Index IX_Questions_QuestionNumber already exists';

-- Index for CategoryId (for filtering by category)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Questions_CategoryId' AND object_id = OBJECT_ID('Questions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Questions_CategoryId
    ON Questions(CategoryId)
    INCLUDE (QuestionNumber, QuestionText, DifficultyLevel, IsElimination, CreatedAt);
    PRINT 'Created index: IX_Questions_CategoryId';
END
ELSE
    PRINT 'Index IX_Questions_CategoryId already exists';

-- Index for IsElimination (for filtering elimination questions)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Questions_IsElimination' AND object_id = OBJECT_ID('Questions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Questions_IsElimination
    ON Questions(IsElimination)
    INCLUDE (QuestionNumber, CategoryId, QuestionText);
    PRINT 'Created index: IX_Questions_IsElimination';
END
ELSE
    PRINT 'Index IX_Questions_IsElimination already exists';

-- Index for CreatedAt (for ordering by creation date)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Questions_CreatedAt' AND object_id = OBJECT_ID('Questions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Questions_CreatedAt
    ON Questions(CreatedAt DESC)
    INCLUDE (QuestionId, QuestionNumber, CategoryId);
    PRINT 'Created index: IX_Questions_CreatedAt';
END
ELSE
    PRINT 'Index IX_Questions_CreatedAt already exists';

-- Composite index for common query pattern
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Questions_CategoryId_QuestionNumber' AND object_id = OBJECT_ID('Questions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Questions_CategoryId_QuestionNumber
    ON Questions(CategoryId, QuestionNumber);
    PRINT 'Created index: IX_Questions_CategoryId_QuestionNumber';
END
ELSE
    PRINT 'Index IX_Questions_CategoryId_QuestionNumber already exists';

-- =============================================
-- 2. CATEGORIES TABLE INDEXES
-- =============================================

-- Index for OrderIndex (already exists, check if it's there)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Categories_OrderIndex' AND object_id = OBJECT_ID('Categories'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Categories_OrderIndex
    ON Categories(OrderIndex);
    PRINT 'Created index: IX_Categories_OrderIndex';
END
ELSE
    PRINT 'Index IX_Categories_OrderIndex already exists';

-- =============================================
-- 3. TRAFFIC SIGNS TABLE INDEXES
-- =============================================

-- Index for SignCode
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TrafficSigns_SignCode' AND object_id = OBJECT_ID('TrafficSigns'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TrafficSigns_SignCode
    ON TrafficSigns(SignCode);
    PRINT 'Created index: IX_TrafficSigns_SignCode';
END
ELSE
    PRINT 'Index IX_TrafficSigns_SignCode already exists';

-- Index for SignType and IsActive (common filter combination)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TrafficSigns_SignType_IsActive' AND object_id = OBJECT_ID('TrafficSigns'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TrafficSigns_SignType_IsActive
    ON TrafficSigns(SignType, IsActive)
    INCLUDE (SignId, SignCode, SignName, ImageUrl);
  PRINT 'Created index: IX_TrafficSigns_SignType_IsActive';
END
ELSE
    PRINT 'Index IX_TrafficSigns_SignType_IsActive already exists';

-- Index for IsActive only
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TrafficSigns_IsActive' AND object_id = OBJECT_ID('TrafficSigns'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TrafficSigns_IsActive
    ON TrafficSigns(IsActive)
    INCLUDE (SignCode, SignName, SignType);
    PRINT 'Created index: IX_TrafficSigns_IsActive';
END
ELSE
    PRINT 'Index IX_TrafficSigns_IsActive already exists';

-- =============================================
-- 4. LICENSE TYPES TABLE INDEXES
-- =============================================

-- Index for LicenseCode (already exists as UNIQUE, check if it's there)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LicenseTypes_LicenseCode' AND object_id = OBJECT_ID('LicenseTypes'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_LicenseTypes_LicenseCode
    ON LicenseTypes(LicenseCode);
    PRINT 'Created index: IX_LicenseTypes_LicenseCode';
END
ELSE
    PRINT 'Index IX_LicenseTypes_LicenseCode already exists';

-- =============================================
-- 5. ANSWER OPTIONS TABLE INDEXES
-- =============================================

-- Index for QuestionId (already exists as FK, check if it's there)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AnswerOptions_QuestionId' AND object_id = OBJECT_ID('AnswerOptions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AnswerOptions_QuestionId
    ON AnswerOptions(QuestionId)
    INCLUDE (OptionOrder, IsCorrect);
    PRINT 'Created index: IX_AnswerOptions_QuestionId';
END
ELSE
    PRINT 'Index IX_AnswerOptions_QuestionId already exists';

-- Composite index for QuestionId and OptionOrder
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AnswerOptions_QuestionId_OptionOrder' AND object_id = OBJECT_ID('AnswerOptions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AnswerOptions_QuestionId_OptionOrder
    ON AnswerOptions(QuestionId, OptionOrder);
    PRINT 'Created index: IX_AnswerOptions_QuestionId_OptionOrder';
END
ELSE
    PRINT 'Index IX_AnswerOptions_QuestionId_OptionOrder already exists';

-- =============================================
-- 6. USERS TABLE INDEXES
-- =============================================

-- Index for Email (already exists as UNIQUE, check if it's there)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUsers_Email' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_AspNetUsers_Email
    ON AspNetUsers(Email) WHERE Email IS NOT NULL;
    PRINT 'Created index: IX_AspNetUsers_Email';
END
ELSE
    PRINT 'Index IX_AspNetUsers_Email already exists';

-- Index for CreatedAt
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUsers_CreatedAt' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AspNetUsers_CreatedAt
    ON AspNetUsers(CreatedAt DESC);
    PRINT 'Created index: IX_AspNetUsers_CreatedAt';
END
ELSE
    PRINT 'Index IX_AspNetUsers_CreatedAt already exists';

-- =============================================
-- UPDATE STATISTICS
-- =============================================

UPDATE STATISTICS Questions WITH FULLSCAN;
UPDATE STATISTICS Categories WITH FULLSCAN;
UPDATE STATISTICS TrafficSigns WITH FULLSCAN;
UPDATE STATISTICS LicenseTypes WITH FULLSCAN;
UPDATE STATISTICS AnswerOptions WITH FULLSCAN;
UPDATE STATISTICS AspNetUsers WITH FULLSCAN;

PRINT '';
PRINT '=============================================';
PRINT 'Performance optimization indexes created successfully!';
PRINT 'Statistics updated for all tables.';
PRINT '=============================================';
GO