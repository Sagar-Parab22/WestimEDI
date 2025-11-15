SELECT       
 ELOG.ID,Email,EDI.SiteID,GEO.Code ,EDI.FileTypeID,GMO.Code ,CommunicationID,  
isnull(FTPPath,'') FTPPath,Frequency  
FROM mnrtEDILog ELOG      
INNER JOIN mnrmEDISetup EDI ON EDI.ID=ELOG.EDIId AND ELOG.IsActive=1      
INNER JOIN comiSite GEO ON GEO.ID=EDI.SiteID AND GEO.IsActive=1  
INNER JOIN comiCustomer GMO ON GMO.ID =EDI.OperatorId AND GMO.IsActive=1  
WHERE Status = 'P'