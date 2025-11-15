SELECT
    MTE.ID AS EstimateId,
    MTE.BPhoto,
    MTE.ContainerNo,
    MEDL.ID AS EDILogId
FROM
    mnrtEDILog MEDL
INNER JOIN
    mnrtEDIDetailLog MEDLD ON MEDLD.EDILogId = MEDL.ID
INNER JOIN
    mnrtEstimate MTE ON MTE.ID = MEDLD.EstimateId AND MTE.ContainerNo = MEDLD.ContainerNo
WHERE
    MEDL.ID = @EdiLogId
    AND MEDL.Status = 'P' 
    AND MTE.BPhoto IS NOT NULL 
    AND MTE.Isactive = 1; 