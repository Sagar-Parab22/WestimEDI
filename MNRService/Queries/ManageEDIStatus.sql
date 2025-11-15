BEGIN TRY
    -- The core UPDATE statement from the procedure
    UPDATE mnrtEDILog
    SET
        Status = @Status,
        SentDate = GETDATE(), -- Set SentDate to the current time
        [FileName] = RIGHT(@FileName, 50) -- Store only the last 50 characters of the filename
    WHERE
        ID = @EDILogId;

    PRINT 'Update successful.'; -- Optional success message

END TRY
BEGIN CATCH
    PRINT 'Error occurred during update.'; -- Optional error message

    -- Log the error to the ErrorLog table
    INSERT INTO ErrorLog (
        [Desc],
        SpName, -- Use a placeholder name
        Line,
        [Date]
    )
    SELECT
        ERROR_MESSAGE(),
        'DirectQuery_ManageEDIStatus', -- Placeholder name
        ERROR_LINE(),
        GETDATE();
END CATCH;