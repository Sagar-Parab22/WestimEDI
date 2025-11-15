
-- CTE to replicate the @Total_Amt table variable logic
WITH Total_Amt_CTE AS (
    SELECT
        MTED.EstimateID,
        SUM(MTED.LabourHours * Qty) AS TotalLHrs,
        SUM(EXTotalValue - MTED.EXPartPrice) AS Total,
        MMR.Mode
    FROM mnrtEDIDetailLog EDI
    INNER JOIN mnrtEstimate MTE ON MTE.ID = EDI.EstimateId
    INNER JOIN mnrtEstimateDetail MTED ON MTED.EstimateID = MTE.ID AND (MTED.IsActive = 1 OR MTED.JobOrderStatus = 'C')
    INNER JOIN mnrmRepairSetItem MMR ON MMR.ID = MTED.RepairSetItemId
    WHERE EDI.EDILogId = @EdiLogId
    GROUP BY MTED.EstimateID, MMR.Mode
)
-- Main SELECT Query
SELECT
    --==================== CTL SECTION =====================
    MNL.EDIId, CC.Code ,

    --==================== HD1 SECTION =====================
    ISNULL(RIGHT((SPACE(36) +RIGHT(REPLACE(MTE.Number,'.',''),10)),36),'') AS EstimateNo,
    REPLACE(CONVERT(VARCHAR(10), MTE.Date,103),'/','') AS ESTIMATEDT,
    MTE.ContainerNo,

CASE
    WHEN LEN(ISNULL(CAST(MMR.Mode AS NVARCHAR(50)), N'')) = 1 -- Check length after handling NULL and casting
    THEN RIGHT(N'00' + ISNULL(CAST(MMR.Mode AS NVARCHAR(50)), N''), 2) -- Pad single char with one leading zero using N'00'
    ELSE ISNULL(CAST(MMR.Mode AS NVARCHAR(50)), N'') -- Otherwise, return the (cleaned) original value or empty string
END AS Mode,
    (CASE WHEN MTED.Responsible='O' THEN '2' ELSE '3' END ) AS DMGRESP,
    MTED.LocationCode AS LocationCode,

    --==================== HD2 SECTION =====================
    Right('00000000000000000000'+CAST (REPLACE(ISNULL(TA.TotalLHrs,0.0),'.','')AS Varchar(20)),4) AS TotalLHrs,
    0.0 AS OTHRS, 0.0 AS DTHRS, 0.0 AS MHRS, MTED.LabourValue, Right('00000000000000000000'+CAST (REPLACE(ISNULL(TA.Total,0.0),'.','')AS Varchar(20)),12) AS TOTAL,

    --==================== RPR SECTION =====================
    LEFT(MTED.RP1Code+ISNULL(MTED.RP2Code,''),2) AS RPCode,
	RIGHT(REPLICATE(N' ', 6) + RIGHT(N'0000' + ISNULL(LTRIM(RTRIM(CAST(MMR.Code AS NVARCHAR(50)))), N''), 4), 6) AS CompCode,


    CASE WHEN CONVERT(INT,SUBSTRING(CAST(Qty as Varchar(5)),CHARINDEX('.',CAST(Qty AS Varchar(5)))+1,Len(CAST(Qty AS VARCHAR(5)))))>0
        THEN
            CASE WHEN LEN(REPLACE(Qty,SUBSTRING(CAST(Qty AS varchar(5)),CHARINDEX('.',CAST(Qty AS Varchar(5))),Len(CAST(Qty AS VARCHAR(5)))),''))>=3
            THEN RIGHT('000000'+REPLACE(CAST(Qty AS VARCHAR(5)),SUBSTRING(CAST(Qty AS VARCHAR(5)),CHARINDEX('.',CAST(Qty AS VARCHAR(5))),Len(CAST(Qty AS VARCHAR(5)))),''),3)
            ELSE LEFT(REPLACE(RIGHT('00000000'+CAST (Qty AS varchar(10)),5),'.',''),3) END
        ELSE
            RIGHT('00000'+REPLACE(CAST(Qty AS VARCHAR(5)),SUBSTRING(CAST(Qty AS VARCHAR(5)),CHARINDEX('.',CAST(Qty AS VARCHAR(5))),LEN(CAST(Qty AS VARCHAR(5)))),''),3)
    END AS QTY,
    Right('00000000000000000000'+CAST (Replace(ISNULL(MTED.MaterialRate,0.0),'.','')AS varchar(10)),12) AS TotalValue, -- Renamed from rowTotal in C#
    Right('00000000000000000000'+CAST (REPLACE(Convert(Decimal(18,2),ISNULL(MTED.LabourHours ,0.0)),'.','')AS Varchar(20)),4) AS LHrs, -- Renamed from rowLhrs in C#
    MTED.Responsible AS RESP,

    --==================== PRT SECTION =====================
    CASE WHEN (Substring (Cast(ISNULL(MTED.PQt1,0.0) AS Varchar(10)),((CharIndex('.',Cast(ISNULL(MTED.PQt1,0.0) As Varchar(10))))+1),len(Cast(ISNULL(MTED.PQt1,0.0) As Varchar(10)))))='00'
        THEN ISNULL(Left('0'+Replace(Cast (ISNULL(MTED.PQt1,0.0)AS varchar(10)),'.',''),3),000)
        WHEN lEN(sUBSTRING(CAST (MTED.PQt1 aS vARCHAR(10)),0,CharIndex('.',Cast(MTED.PQt1 As Varchar(10)))))>=2
        THEN ISNULL(Left(Replace(Cast (ISNULL(MTED.PQt1,0.0)AS varchar(10)),'.',''),3),000)
        WHEN lEN(sUBSTRING(CAST (MTED.PQt1 aS vARCHAR(10)),0,CharIndex('.',Cast(MTED.PQt1 As Varchar(10)))))<2
        THEN ISNULL(left('0'+Replace(Cast (ISNULL(MTED.PQt1,0.0)AS varchar(10)),'.',''),3),000)
    END AS PQt1,
    ISNULL(MTED.PartNo,'') AS PartNo,
    --==================== PRT2 SECTION =====================
    CASE WHEN (Substring (Cast(ISNULL(MTED.PQt2,0.0) AS Varchar(10)),((CharIndex('.',Cast(ISNULL(MTED.PQt2,0.0) As Varchar(10))))+1),len(Cast(ISNULL(MTED.PQt2,0.0) As Varchar(10)))))='00'
        THEN Left('0'+Replace(Cast (ISNULL(MTED.PQt2,0.0)AS varchar(10)),'.',''),3)
        WHEN lEN(sUBSTRING(CAST (MTED.PQt2 aS vARCHAR(10)),0,CharIndex('.',Cast(MTED.PQt2 As Varchar(10)))))>=2
        THEN Left(Replace(Cast (ISNULL(MTED.PQt2,0.0)AS varchar(10)),'.',''),3)
        WHEN lEN(sUBSTRING(CAST (MTED.PQt2 aS vARCHAR(10)),0,CharIndex('.',Cast(MTED.PQt2 As Varchar(10)))))<2
        THEN left('0'+Replace(Cast (ISNULL(MTED.PQt2,0.0)AS varchar(10)),'.',''),3)
    END AS PQt2,
    ISNULL(MTED.PartNo2,'') AS PartNo2,
    --==================== PRT3 SECTION =====================
    CASE WHEN (Substring (Cast(ISNULL(MTED.PQt3,0.0) AS Varchar(10)),((CharIndex('.',Cast(ISNULL(MTED.PQt3,0.0) As Varchar(10))))+1),len(Cast(ISNULL(MTED.PQt3,0.0) As Varchar(10)))))='00'
        THEN Left('0'+Replace(Cast (ISNULL(MTED.PQt3,0.0)AS varchar(10)),'.',''),3)
        WHEN lEN(sUBSTRING(CAST (MTED.PQt3 aS vARCHAR(10)),0,CharIndex('.',Cast(MTED.PQt3 As Varchar(10)))))>=2
        THEN Left(Replace(Cast (ISNULL(MTED.PQt3,0.0)AS varchar(10)),'.',''),3)
        WHEN lEN(sUBSTRING(CAST (MTED.PQt3 aS vARCHAR(10)),0,CharIndex('.',Cast(MTED.PQt3 As Varchar(10)))))<2
        THEN left('0'+Replace(Cast (ISNULL(MTED.PQt3,0.0)AS varchar(10)),'.',''),3)
    END AS PQt3,
    ISNULL(MTED.PartNo3,'') AS PartNo3,
    --==================== PRT4 SECTION =====================
    CASE WHEN (Substring (Cast(ISNULL(MTED.PQt4,0.0) AS Varchar(10)),((CharIndex('.',Cast(ISNULL(MTED.PQt4,0.0) As Varchar(10))))+1),len(Cast(ISNULL(MTED.PQt4,0.0) As Varchar(10)))))='00'
        THEN Left('0'+Replace(Cast (ISNULL(MTED.PQt4,0.0)AS varchar(10)),'.',''),3)
        WHEN lEN(sUBSTRING(CAST (MTED.PQt4 aS vARCHAR(10)),0,CharIndex('.',Cast(MTED.PQt4 As Varchar(10)))))>=2
        THEN Left(Replace(Cast (ISNULL(MTED.PQt4,0.0)AS varchar(10)),'.',''),3)
        WHEN lEN(sUBSTRING(CAST (MTED.PQt4 aS vARCHAR(10)),0,CharIndex('.',Cast(MTED.PQt4 As Varchar(10)))))<2
        THEN left('0'+Replace(Cast (ISNULL(MTED.PQt4,0.0)AS varchar(10)),'.',''),3)
    END AS PQt4,
    ISNULL(MTED.PartNo4,'') AS PartNo4,
    --==================== PRT5 SECTION =====================
    CASE WHEN (Substring (Cast(ISNULL(MTED.PQt5,0.0) AS Varchar(10)),((CharIndex('.',Cast(ISNULL(MTED.PQt5,0.0) As Varchar(10))))+1),len(Cast(ISNULL(MTED.PQt5,0.0) As Varchar(10)))))='00'
        THEN Left('0'+Replace(Cast (ISNULL(MTED.PQt5,0.0)AS varchar(10)),'.',''),3)
        WHEN lEN(sUBSTRING(CAST (MTED.PQt5 aS vARCHAR(10)),0,CharIndex('.',Cast(MTED.PQt5 As Varchar(10)))))>=2
        THEN Left(Replace(Cast (ISNULL(MTED.PQt5,0.0)AS varchar(10)),'.',''),3)
        WHEN lEN(sUBSTRING(CAST (MTED.PQt5 aS vARCHAR(10)),0,CharIndex('.',Cast(MTED.PQt5 As Varchar(10)))))<2
        THEN left('0'+Replace(Cast (ISNULL(MTED.PQt5,0.0)AS varchar(10)),'.',''),3)
    END AS PQt5,
    ISNULL(MTED.PartNo5,'') AS PartNo5,
    MNLD.EstimateId,
    MTED.ID,
    ISNULL(MTE.Comments,'') AS Comments --[Naren 26Aug2015]
FROM mnrtEDILog MNL
INNER JOIN mnrtEDIDetailLog MNLD ON MNLD.EDILogId = MNL.Id
INNER JOIN mnrmEDISetup MNE ON MNE.ID = MNL.EDIId
INNER JOIN comiCustomer CC ON CC.ID = MNE.OperatorID
INNER JOIN mnrtEstimate MTE ON MTE.ID = MNLD.EstimateId AND MTE.IsActive=1
INNER JOIN mnrtEstimateDetail MTED ON MTED.EstimateID = MTE.ID AND (MTED.IsActive = 1 OR MTED.JobOrderStatus = 'C')
INNER JOIN mnrmRepairSetItem MMR ON MMR.ID = MTED.RepairSetItemId
INNER JOIN Total_Amt_CTE TA ON TA.EstimateID = MTE.ID AND TA.Mode = MMR.Mode -- Joining the CTE
WHERE MNL.ID = @EdiLogId AND MNL.Status = 'P';
