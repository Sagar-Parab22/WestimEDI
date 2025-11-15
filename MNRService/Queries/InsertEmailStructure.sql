BEGIN TRY
    -- The core INSERT statement
    INSERT INTO dputEmailStructure(
        Sender,
        Receiver,
        Subject,
        Body,
        FilePath,
        SendDate,
        Status,
        IsActive,
        CrtByTokenID,
        CrtDate,
        SenderPassword,
        CommunicationTypeID,
        KeyPath,
        HostKeyFingerprint
    )
    VALUES (
        @Sender,
        @Receiver,
        @Subject,
        @Body,
        @FilePath,
        GETDATE(), -- Use current time
        0,         -- Status (Pending)
        1,         -- IsActive (True)
        1,         -- CrtByTokenID (Assuming 1)
        GETDATE(), -- CrtDate
        @SenderPassword,
        @CommunicationTypeID,
        @KeyPath,
        @HostKeyFingerprint
    );

    -- SET @ErrorMsg = 'S1000'; -- Mimic success code (Optional)
    PRINT 'Insert successful.'; -- Optional success message

END TRY
BEGIN CATCH
    -- SET @ErrorMsg = 'T1100'; -- Mimic error code (Optional)
    PRINT 'Error occurred during insert.'; -- Optional error message

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