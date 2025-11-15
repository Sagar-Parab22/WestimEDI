SET NOCOUNT ON;

-- Use a temporary table to hold active EDI setups
DROP TABLE IF EXISTS #TMPEDI;
CREATE TABLE #TMPEDI (
    SRNO INT IDENTITY(1, 1),
    EDIId INT,
    GeoId INT,
    OperatorId INT,
    Frequency CHAR(1),
    ToAddress VARCHAR(MAX)
);

/* INSERTING MNREDI DETAILS INTO TEMP TABLE */
INSERT INTO #TMPEDI (EDIId, GeoId, OperatorId, Frequency, ToAddress)
SELECT EDI.Id, EDI.SiteID, EDI.OperatorId, EDI.Frequency, EDI.EmailIDs
FROM mnrmEDISetup EDI -- Assuming MNREDI is your EDI setup table
WHERE EDI.IsActive = 1;

/* Variable declarations */
DECLARE @TotalRecord INT, @SRNO INT = 1;
DECLARE @EDILogId BIGINT, @TotCont INT;
DECLARE @EDIId INT, @GeoId INT, @OperatorId INT, @Frequency CHAR(1), @ToAddress VARCHAR(500);
DECLARE @GeoCode VARCHAR(30), @OperatorCode VARCHAR(30), @FREQTIME INT, @LASTSENT DATETIME;

SET @TotalRecord = (SELECT COUNT(*) FROM #TMPEDI);

/* Loop through each active EDI setup */
WHILE (@TotalRecord >= @SRNO)
BEGIN
    SELECT
        @EDIId = EDIId,
        @GeoId = TMP.GeoId,
        @OperatorId = TMP.OperatorId,
        @Frequency = TMP.Frequency,
        @ToAddress = TMP.ToAddress,
        @GeoCode = GEO.Code, -- Assuming GeoCode column exists in DMGeo
        @OperatorCode = OPR.Code -- Assuming OperatorCode exists in GMOperator
    FROM #TMPEDI TMP
    INNER JOIN comiSite GEO ON GEO.ID = TMP.GeoId -- Assuming DMGeo table
    INNER JOIN comiCustomer OPR ON OPR.ID = TMP.OperatorId -- Assuming GMOperator table
    WHERE SRNO = @SRNO;

    -- Determine frequency interval in minutes
    SET @FREQTIME = (CASE (@Frequency)
                        WHEN '1' THEN 60   -- Assuming 1=Hourly
                        WHEN '2' THEN 240  -- Assuming 2=Every 4 hours
                        WHEN '3' THEN 1440 -- Assuming 3=Daily
                        WHEN '4' THEN 5    -- Assuming 4=Every 5 minutes
                        WHEN '5' THEN 15   -- Assuming 5=Every 15 minutes
                        ELSE 1440          -- Default to Daily if frequency is unknown/invalid
                    END);

    -- Get the last sent time for this EDI setup, using a baseline date if never sent
    SET @LASTSENT = (SELECT MAX(ISNULL(SentDate, '1900-01-01')) FROM mnrtEDILog WHERE EDIId = @EDIId);

    /* Check if it's time to send again based on frequency and if there are unprocessed estimates */
    IF EXISTS (
        SELECT 1
        FROM mnrtEstimate MTE -- Assuming MTEstimate table
        LEFT JOIN mnrtEDIDetailLog DTLS ON DTLS.EstimateID = MTE.ID AND DTLS.IsActive = 1
        -- LEFT JOIN MNREDILog MEL ON MEL.EDILogId=DTLS.EDILogId AND MEL.IsActive=1 -- Join seemed redundant for EXISTS check
        WHERE DTLS.EstimateId IS NULL -- Only estimates not yet logged in details
          AND MTE.Category = 'C'
          AND MTE.Date IS NOT NULL
          AND MTE.OperatorId = @OperatorId
          AND MTE.SiteID = @GeoId
          AND MTE.Status NOT IN ('D', 'R') -- Exclude Draft/Rejected
          -- AND MTE.IsActive = 'Y' -- Add this check if necessary for estimates
    )
   -- AND (DATEADD(MI, @FREQTIME, @LASTSENT) <= dbo.Fn_GetDate(@GeoId) OR @LASTSENT = '1900-01-01') -- Check frequency vs. current Geo time
    BEGIN
        -- Create a new header log entry
        INSERT INTO mnrtEDILog (EDIId, Email, Status, CrtBy, CrtDate, CrtIp, IsActive)
        VALUES (@EDIId, @ToAddress, 'P', 'SYSTEM', GETDATE(), 'System', 1);

        -- Get the ID of the log entry just created (safer than IDENT_CURRENT)
        SET @EDILogId = SCOPE_IDENTITY();

        IF @EDILogId IS NOT NULL -- Proceed only if the header insert was successful
        BEGIN
            -- Insert details for all matching estimates into the detail log
            INSERT INTO mnrtEDIDetailLog (EDILogId, EstimateId, ContainerNo, IsoSzType, ActivityDate, operator, GrossWt, CrtByTokenID, CrtDate, IsActive)
            SELECT @EDILogId, MTE.ID, MTE.ContainerNo, ISO.ISOSzType, MTE.Date,
                   OPR.Code, MTE.ContPayload,
                   0, GETDATE(), 1
            FROM mnrtEstimate MTE
            INNER JOIN sysmOperatorFinanceSizeType ISO ON ISO.ID = MTE.SizeTypeID -- Assuming GMOpsFinSzType table
            INNER JOIN comiCustomer OPR ON OPR.ID = MTE.OperatorId
            LEFT JOIN mnrtEDIDetailLog DTLS_Check ON DTLS_Check.EstimateID = MTE.ID AND DTLS_Check.IsActive = 1 -- Re-check inside insert select to avoid race conditions
            -- LEFT JOIN MNREDILog MEL ON MEL.EDILogId=DTLS_Check.EDILogId AND MEL.IsActive=1 -- Redundant check?
            WHERE DTLS_Check.EstimateId IS NULL -- Ensure it wasn't added by another process between EXIST check and INSERT
              AND MTE.Date IS NOT NULL
              AND MTE.Category = 'C'
              AND MTE.OperatorId = @OperatorId
              AND MTE.SiteID = @GeoId
              AND MTE.Status NOT IN ('D', 'R');
              -- AND MTE.IsActive = 'Y'; -- Add if needed

            -- Update the total container count in the header log
            SET @TotCont = (SELECT COUNT(1) FROM mnrtEDIDetailLog WHERE EDILogId = @EDILogId);

            UPDATE mnrtEDILog
            SET TotalContainer = @TotCont
            WHERE ID = @EDILogId; -- Assuming EDILogId is the primary key
        END
    END;

    SET @SRNO = @SRNO + 1;
END;

-- Clean up the temporary table
DROP TABLE IF EXISTS #TMPEDI;

SET NOCOUNT OFF;