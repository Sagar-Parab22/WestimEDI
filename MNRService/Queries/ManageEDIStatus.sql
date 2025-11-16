-- This script has NO error handling.
-- If the UPDATE fails, the script will just stop.

UPDATE mnrtEDILog
SET
    Status = @Status,
    SentDate = NOW(), -- GETDATE() is NOW() in MySQL
    `FileName` = RIGHT(@FileName, 50) -- Use backticks for names, not brackets
WHERE
    ID = @EDILogId;

SELECT 'Update successful.' AS Message; -- PRINT is SELECT in MySQL