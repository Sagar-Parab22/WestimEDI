
-- Temporary table and table variable declarations (needed for the query)
DECLARE @TotalCount int=0;
DECLARE @TotalCol int=0;
SELECT @TotalCount=COUNT (DISTINCT ContainerNo) from mnrtEDIDetailLog WHERE EDILogId=@EDILogId;
Drop table if exists #DamTmp 
CREATE Table #DamTmp (EstimateID INT, EdiDtls Varchar(max), LabourRate money);
TRUNCATE TABLE #DamTmp;

Insert Into #DamTmp
SELECT MTE.ID,
 'DAM+'+RIGHT(('0'+CAST(ROW_NUMBER() OVER(PARTITION BY MTED.ESTIMATEID ORDER BY mted.ID ASC) AS VARCHAR(20))),2) +'+'+MTED.LocationCode+'+'+MTED.CompCode+'+'+ISNULL(MTED.RP1Code,'')+'+'+ISNULL(MTED.RP2Code,'')+''''
+'WOR+'+CASE WHEN MTE.OperatorId=63 THEN MRSI.Code+'+' ELSE 'AV+' END+'MMT'+':'+CAST(ISNULL(MTED.Length,0.0) AS VARCHAR(15))+':'+CAST(ISNULL(MTED.Width,0.0) AS VARCHAR(15))+':'+CAST(ISNULL(MTED.Height,0.0) AS VARCHAR(15))+'+'+CAST(ISNULL(CAST(MTED.Qty AS INT),0) AS VARCHAR(15)) +''''
+'COS+'+'00+'+CAST(Convert(decimal(18,2),MTED.LabourHours) AS VARCHAR(15))+'+'+CAST(MTED.MaterialValue AS VARCHAR(15))+'+'+(CASE When MTED.Responsible='O' THEN 'O' Else 'U' END)+'+'+CAST(MMHD.LabourRate AS VARCHAR(15))+'+'+'N'''
,CAST(MMHD.LabourRate AS VARCHAR(15)) LabourRate
FROM mnrtEDIDetailLog EDI
INNER JOIN mnrtEstimate MTE ON MTE.ID=EDI.EstimateId
INNER JOIN mnrtEstimateDetail MTED ON MTED.EstimateID=MTE.ID --AND (MTED.IsActive='Y' OR MTED.JobOrdStatus='C')
INNER JOIN mnrmRepairSetItem MRSI ON MRSI.ID=MTED.RepairSetItemId -- Added on 13-05-17 by Amit for Repair Mode
INNER JOIN mnrmHourContract MMH ON MMH.OperatorId=MTE.OperatorId AND MMH.SiteID=MTE.SiteID
AND dbo.Fn_GetDate(MTE.SiteID) BETWEEN MMH.ValidFrom AND MMH.ValidUntil
INNER JOIN mnrmHourContractDetail MMHD ON MMH.ID=MMHD.HourContractID AND MMHD.IsoSzTypeId=MTE.SizeTypeID
WHERE EDI.EDILogId=@EDILogId;

DECLARE @CTE_DAM_COS AS TABLE ( EstimateID INT, CountE INT, EdiLineDtls Varchar(5000), LabourRate money );
INsert Into @CTE_DAM_COS
SELECT DISTINCT EstimateID, Count(EstimateID)*3 CountE,
 STUFF( (SELECT '' + est.EdiDtls
         FROM #DamTmp AS est
         WHERE est.EstimateID = dt.EstimateID
         FOR XML PATH('')), 1, 0, '') AS EdiLineDtls, LabourRate
FROM #DamTmp dt
GROUP BY EstimateID,LabourRate;

Update CT
SET CT.CountE=(Select SUM(CTE.CountE) FROM @CTE_DAM_COS CTE)
FROM @CTE_DAM_COS CT;

DECLARE @CTE_CT AS TABLE ( EstimateID INT, CTO_Details Varchar(5000) );
Insert Into @CTE_CT
SELECT MTED.EstimateID,'CTO+'+(CASE When MTED.Responsible='O' THEN 'O' Else 'U' END)+'+'
 +CAST(SUM(MTED.LabourValue) AS VARCHAR(15) )+'+'
 +CAST(SUM(MTED.MaterialValue) AS VARCHAR(15))
 +'+0++'+CAST(SUM(TotalValue) AS VARCHAR(15))+'''' CTO_Details
FROM mnrtEDIDetailLog EDI
 INNER JOIN mnrtEstimate MTE ON MTE.ID=EDI.EstimateId
 INNER JOIN mnrtEstimateDetail MTED ON MTED.EstimateID=MTE.ID --AND (MTED.IsActive='Y' OR MTED.JobOrdStatus='C')
 WHERE EDI.EDILogId=@EDILogId
 GROUP BY MTED.EstimateID,MTED.Responsible;

DECLARE @CTE_CTO AS TABLE ( EstimateID INT, CountCTO INT, CTO_Details Varchar(5000) );
Insert Into @CTE_CTO
SELECT DISTINCT EstimateID, Count(EstimateId)CountCTO,
 STUFF( (SELECT '' + est.CTO_Details
         FROM @CTE_CT AS EST
         WHERE EST.EstimateID = CT.EstimateID
         FOR XML PATH('')), 1, 0, '') AS EdiLineDtls
FROM @CTE_CT CT
GROUP BY EstimateID;

DECLARE @CTE_TMA AS TABLE ( EstimateID INT, TMA_Details Varchar(5000) );
INSERT INTO @CTE_TMA
SELECT EDI.EstimateID,SUM(MTED.TotalValue) TMA_Details
FROM mnrtEDIDetailLog EDI
 INNER JOIN mnrtEstimate MTE ON MTE.ID=EDI.EstimateId
 INNER JOIN mnrtEstimateDetail MTED ON MTED.EstimateID=MTE.ID --AND (MTED.IsActive='Y' OR MTED.JobOrdStatus='C')
 WHERE EDI.EDILogId=@EDILogId
 GROUP BY EDI.EstimateID;

DECLARE @TmpDetails AS Table ( EdiId Int, UNB VARCHAR(50), UNBDETAILS VARCHAR(500), UNHDETAILS VARCHAR(500), DTMDETAILS VARCHAR(500), RFF1DETAILS VARCHAR(500), RFFDETAILS VARCHAR(500), ACADETAILS VARCHAR(500), LBRDETAILS VARCHAR(500), NAD1DETAILS VARCHAR(500), NAD2DETAILS VARCHAR(500), EQFDetails VARCHAR(500), ECIDETAILS VARCHAR(500), CUIDETAILS Varchar(500), DAMDetails VARCHAR(500), CTODetails VARCHAR(500), TMADetails VARCHAR(500), UNTDetails VARCHAR(500), UNZDetails Varchar(500), Operator VARCHAR(50), ColumnCount INT, CountE INT );

INSERT INTO @TmpDetails(EdiId,UNB,UNBDETAILS,UNHDETAILS,DTMDETAILS, RFF1DETAILS,RFFDETAILS ,ACADETAILS ,LBRDETAILS ,NAD1DETAILS ,NAD2DETAILS ,EQFDetails,ECIDETAILS ,CUIDETAILS,DAMDetails ,CTODetails ,TMADetails ,UNTDetails,UNZDetails ,Operator ,ColumnCount,CountE)
SELECT DISTINCT
 EDI.Id
 ,'UNB+' UNB
 ,ISNULL(MAp.A1,'')+':'+ISNULL(MAp.A2,'') +'+'+ISNULL(MAp.A3,'') + '+'+ISNULL(MAp.A4,'') +'+'+
 CONVERT(VARCHAR(6),dbo.Fn_GetDate(MTE.SiteID),12)+':'+ Replace ((CONVERT(VARCHAR(5),dbo.Fn_GetDate(MTE.SiteID), 108)),':','')+'+'+CAST(EDTLS.ID AS varchar(15)) +'''' UNBDETAILS
 ,'UNH+'+Right(CASE
    WHEN CHARINDEX('.', ISNULL(MTE.EIRNo,'')) > 0 -- Check if '.' exists
    THEN Substring(ISNULL(MTE.EIRNo,''), 1, CHARINDEX('.', ISNULL(MTE.EIRNo,'')) - 1) -- Original logic if '.' exists
    ELSE ISNULL(MTE.EIRNo,'') -- Otherwise, take the whole string (or empty if NULL)
END,14)+'+'+'WESTIM'+':'+'0'+'+'+Case When (MTE.Type='X') THEN 'C' Else Case When (MTE.Type='U') THEN 'M' Else ' ' END END +'+'+Case When Number
 LIKE '%.%' Then Right(Number,1) Else '0' END+'''' UNHDETAILS
 ,'DTM+'+ISNULL(MAp.B1,'')+'+'+RIGHT(CONVERT(VARCHAR(8),MTE.Date,112),6)+'''' DTMDETAILS
 ,'RFF+'+ISNULL(MAp.Z1,'')+ISNULL('+'+MTE.GridCode,'')+'''' RFF1DETAILS
 ,'RFF+'+ISNULL(MAp.C1,'')+'+'+Right(CASE
    WHEN CHARINDEX('.', ISNULL(MTE.EIRNo,'')) > 0 -- Check if '.' exists
    THEN Substring(ISNULL(MTE.EIRNo,''), 1, CHARINDEX('.', ISNULL(MTE.EIRNo,'')) - 1) -- Original logic if '.' exists
    ELSE ISNULL(MTE.EIRNo,'') -- Otherwise, take the whole string (or empty if NULL)
END,14)--ISNULL(MTE.EIRNo,'')
 +'+'+RIGHT(CONVERT(VARCHAR(8),MTE.Date ,112),6)+'''' RFFDETAILS
 ,'ACA+'+'ZAR'+'+'+'STD'+':'+'0'+'''' ACADETAILS
 ,'LBR+'+ CONVERT(VARCHAR,CDC.LabourRate)+'''' LBRDETAILS
 ,'NAD+'+ISNULL(MAp.D1,'')+'+'+ISNULL(MAp.D2,'')+'''' NAD1DETAILS
 ,'NAD+'+ISNULL(MAp.E1,'')+'+'+ISNULL(MAp.E2,'')+'''' NAD2DETAILS
 ,'EQF+'+Case When (GMO.IsRefCargo='N') Then 'CON+' Else Case When (GMO.IsRefCargo='Y') Then 'REF+' END END+LEFT(MTE.ContainerNo,4)+':'+ RIGHT(MTE.ContainerNo,7)
 +'+'+EDTLS.IsoSzType+':'+DOM.OperatorSizeType+'+MGW:'
 +CAST(Convert(Decimal(18,0),ISNULL(EDTLS.GrossWt,0)) AS VARCHAR(15))+':KGM''' EQFDetails
 ,'ECI+D'+'''' ECIDETAILS
 ,Case When Map.CUI='Y' THEN 'CUI++'+ISNULL(Map.Z2,'')+':'+CONVERT(VARCHAR(6),MTE.InPhysicalTime,12)+'''' Else'' END CUIDETAILS
 ,CDC.EdiLineDtls DAMDetails
 ,CTO.CTO_Details CTODetails
 ,'TMA+'+CAST(TMA.TMA_Details AS VARCHAR(15))+'+++++0'+'''' TMADetails
 ,'UNT+#'+'+'+Right(CASE
    WHEN CHARINDEX('.', ISNULL(MTE.EIRNo,'')) > 0 -- Check if '.' exists
    THEN Substring(ISNULL(MTE.EIRNo,''), 1, CHARINDEX('.', ISNULL(MTE.EIRNo,'')) - 1) -- Original logic if '.' exists
    ELSE ISNULL(MTE.EIRNo,'') -- Otherwise, take the whole string (or empty if NULL)
END,14)+'''' UNTDetails
 ,'UNZ+'+CAST(ISNULL(ELog.TotalContainer,0)AS VARCHAR(10))+ISNULL('+'+ CAST(EDTLS.ID AS varchar(15)) ,'')+'''' UNZDetails
 ,ISNULL(EDTLS.Operator,'')Operator
  ,ISNULL(CDC.CountE,0)+ISNull(CTO.CountCTO,0) ColumnCount
 ,CDC.CountE
 FROM mnrtEDILog ELOG
 INNER JOIN mnrtEDIDetailLog EDTLS ON ELOG.ID = EDTLS.EDILogId
 INNER JOIN mnrmEDISetup EDI ON EDI.ID = ELOG.EDIId
 INNER JOIN mnrtEDIMessageMapping MAP ON MAP.EDIId = EDI.ID
 INNER JOIN ComiCustomer GM ON GM.ID = EDI.OperatorId
 INNER JOIN ComiSite GEO ON GEO.ID = EDI.SiteID
 INNER JOIN mnrtEstimate MTE ON MTE.ID=EDTLS.EstimateID
 INNER JOIN mnrmOperatorSizeType DOM ON DOM.ID=MTE.SizeTypeID AND DOM.SiteID=MTE.SiteID --AND DOM.OperatorId=MTE.OperatorId
 INNER JOIN sysmOperatorFinanceSizeType GMO ON GMO.ID=DOM.SizeTypeID
 INNER JOIN mnrtEstimateDetail MTED ON MTE.ID=MTED.EstimateID --AND (MTED.IsActive='Y' OR MTED.JobOrdStatus='C')
 INNER JOIN @CTE_DAM_COS CDC ON CDC.EstimateID=EDTLS.EstimateId
 INNER JOIN @CTE_CTO CTO ON CTO.EstimateID=EDTLS.EstimateId
 INNER JOIN @CTE_TMA TMA ON TMA.EstimateId=EDTLS.EstimateId
 WHERE EDTLS.EDILogId = @EDILogId; --AND (MTE.IsActive='Y' OR MTE.EstimateCategory='C')

-- Final SELECT statement
SELECT
 EdiId
 ,UNB
 ,UNBDETAILS
 ,UNHDETAILS
 ,DTMDETAILS
 ,RFF1DETAILS
 ,RFFDETAILS
 ,ACADETAILS
 ,LBRDETAILS
 ,NAD1DETAILS
 ,NAD2DETAILS
 ,EQFDetails
 ,ECIDETAILS
 ,CUIDETAILS
 ,DAMDetails
 ,CTODetails
 ,TMADetails
 ,REPLACE(UNTDetails,'#',CONVERT(VARCHAR(20),((Case When CUIDETAILS=''Then 15+(ColumnCount-3) Else 15+(ColumnCount-1) End)))) UNTDetails
 ,UNZDetails
 ,Operator
FROM @TmpDetails;

-- Cleanup temporary table
DROP TABLE #DamTmp;
