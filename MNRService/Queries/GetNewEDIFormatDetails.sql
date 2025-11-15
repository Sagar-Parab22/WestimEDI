SELECT
    -- Fields for NewEDIFormatLine model
    MTE.Date AS Date1,                           -- [0] (Date1)
    MTE.Number AS WorkOrderNum,                  -- [1] (WorkOrderNum)
    MTE.InPhysicalTime AS Date2,                 -- [2] (Date2)
    MTE.ContainerNo AS ContainerNum,             -- [3] (ContainerNum)
    MTED.LabourHours AS LabourHours,             -- [4] (LabourHours)
    
    -- Field 6: (Line Item No - NEW - Generates '1', '2', '3'...)
    ROW_NUMBER() OVER(PARTITION BY MNL.ID ORDER BY MTED.ID ASC) AS LineItemNum, -- [5] (LineItemNum)
    
    -- Field 7: (Repair Code - User Confirmed)
    LEFT(MTED.RP1Code + ISNULL(MTED.RP2Code,''), 2) AS RepairCode, -- [6] (RepairCode)
    
    MTED.LocationCode AS LocationCode,           -- [7] (LocationCode)
    
    -- Field 9: (Material Cost - User Confirmed as 'TotalValue' which maps to MaterialRate)
    ISNULL(MTED.MaterialRate, 0.0) AS MaterialCost, -- [8] (MaterialCost)
	(ISNULL(MTED.MaterialRate, 0.0) * ISNULL(MTED.Qty, 0.0)) + ISNULL(MTED.LabourValue, 0.0) AS MaterialValue, -- [9] (MaterialValue)
     --   ISNULL(MTED.MaterialValue, 0.0) AS MaterialValue, -- [9] (MaterialValue)

    -- *** SWAPPED THESE TWO COLUMNS ***
    MTED.Responsible AS ResponsibleParty,      -- [10] (ResponsibleParty)
    MTED.Qty AS Qty                            -- [11] (Qty)
    -- *** END SWAP ***

FROM mnrtEDILog MNL
INNER JOIN mnrtEDIDetailLog MNLD ON MNLD.EDILogId = MNL.Id
INNER JOIN mnrmEDISetup MNE ON MNE.ID = MNL.EDIId
INNER JOIN mnrtEstimate MTE ON MTE.ID = MNLD.EstimateId AND MTE.IsActive = 1
INNER JOIN mnrtEstimateDetail MTED ON MTED.EstimateID = MTE.ID AND (MTED.IsActive = 1 OR MTED.JobOrderStatus = 'C')
WHERE MNL.ID = @EdiLogId AND MNL.Status = 'P'
ORDER BY MTED.ID ASC; -- Ensure lines are in the correct order
