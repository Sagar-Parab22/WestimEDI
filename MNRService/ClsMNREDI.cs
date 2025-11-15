using MNRService.Helpers;
using MNRService.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNRService
{
    public class ClsMNREDI
    {

        #region Database Read Methods

        // Fetches the list of pending EDI jobs
        public List<MNREDI> GetEDILogId()
        {
            var listEDIMsgDtls = new List<MNREDI>();

            using (SqlDataReader rdr = SqlHelper.ExecuteReaderText("GetEDILogId.sql"))
            {
                while (rdr.Read())
                {
                    var objEDIMsgDtls = new MNREDI
                    {
                        EDILogId = rdr.GetInt32(0),
                        Email = rdr.GetString(1),
                        GeoId = rdr.GetInt32(2),
                        GeoCode = rdr.GetString(3),
                        EDITypeId = rdr.GetInt32(4),
                        Operator = rdr.GetString(5),
                        CommunicationID = rdr.GetInt32(6),
                        FTPPath = rdr.GetString(7)
                    };

                    // SFTP value parsing from your old code
                    string[] SFTPValue = objEDIMsgDtls.FTPPath.Split('~');
                    if (SFTPValue.Length > 1)
                    {
                        objEDIMsgDtls.FTPPath = SFTPValue[0];
                        objEDIMsgDtls.SFTPKeyPath = SFTPValue[1];
                        objEDIMsgDtls.SFTPHostKey = SFTPValue[2];
                    }
                    listEDIMsgDtls.Add(objEDIMsgDtls);
                }
            }
            return listEDIMsgDtls;
        }

        // Fetches details for standard EDIFACT files
        public List<MNREDI> GetEDIActivityDetails(long EDILogId)
        {
            var ListEDIContainerDtls = new List<MNREDI>();
            var parameters = new[] { new SqlParameter("@EDILogId", EDILogId) };

            using (SqlDataReader dr = SqlHelper.ExecuteReaderText("GetEDIActivityDetails.sql", parameters))
            {
                while (dr.Read())
                {
                    var ObjEDIContainerDtls = new MNREDI
                    {
                        EDIId = dr.GetInt32(0),
                        UNB = dr.GetString(1),
                        A1 = dr.GetString(2),
                        UNH = dr.GetString(3),
                        DTM = dr.GetString(4),
                        RFF1 = dr.GetString(5),
                        RFF = dr.GetString(6),
                        ACA = dr.GetString(7),
                        LBR = dr.GetString(8),
                        G1NAD = dr.GetString(9),
                        G2NAD = dr.GetString(10),
                        EQF = dr.GetString(11),
                        ECI = dr.GetString(12),
                        CUI = dr.IsDBNull(13) ? string.Empty : dr.GetString(13),
                        DAM = dr.GetString(14),
                        CTO = dr.GetString(15),
                        TMA = dr.GetString(16),
                        UNT = dr.GetString(17),
                        UNZ = dr.GetString(18),
                        Operator = dr.IsDBNull(19) ? string.Empty : dr.GetString(19)
                    };
                    ListEDIContainerDtls.Add(ObjEDIContainerDtls);
                }
            }
            return ListEDIContainerDtls;
        }

        #region New EDI Format
        // Fetches details for the new fixed-width format
        public List<NewEDIFormatLine> GetNewEDIFormatDetails(long EDILogId)
        {
            var listNewFormatDetails = new List<NewEDIFormatLine>();
            var parameters = new[] { new SqlParameter("@EDILogId", EDILogId) };

            using (SqlDataReader dr = SqlHelper.ExecuteReaderText("GetNewEDIFormatDetails.sql", parameters))
            {
                while (dr.Read())
                {
                    var detailLine = new NewEDIFormatLine
                    {
                        // Make sure to handle potential NULLs from the database
                        Date1 = dr.IsDBNull(0) ? DateTime.MinValue : dr.GetDateTime(0),
                        WorkOrderNum = dr.IsDBNull(1) ? string.Empty : dr.GetString(1),
                        Date2 = dr.IsDBNull(2) ? DateTime.MinValue : dr.GetDateTime(2),
                        ContainerNum = dr.IsDBNull(3) ? string.Empty : dr.GetString(3),
                        LabourHours = dr.IsDBNull(4) ? 0m : dr.GetDecimal(4),
                        LineItemNum = dr.IsDBNull(5) ? 0 : dr.GetInt64(5),
                        RepairCode = dr.IsDBNull(6) ? string.Empty : dr.GetString(6),
                        LocationCode = dr.IsDBNull(7) ? string.Empty : dr.GetString(7),
                        MaterialCost = dr.IsDBNull(8) ? 0m : dr.GetDecimal(8),
                        MaterialValue = dr.IsDBNull(9) ? 0m : dr.GetDecimal(9),
                        ResponsibleParty = dr.IsDBNull(10) ? string.Empty : dr.GetString(10),
                        Qty = dr.IsDBNull(11) ? 0m : dr.GetDecimal(11)
                    };
                    listNewFormatDetails.Add(detailLine);
                }
            }
            return listNewFormatDetails;
        }
        private string PadDecimalWithZeros(decimal value, int length)
        {
            string formatted = value.ToString("F2"); // Format as "0.00"
            // Pad with '0' on the left
            return formatted.PadLeft(length, '0');
        }
        // Generates the full file for the new fixed-width format
        public List<string> GenerateNewFileFormat(List<NewEDIFormatLine> listData, string storagePath, MNREDI ediJob)
        {
            var generatedFilePaths = new List<string>();
            if (listData.Count == 0)
            {
                return generatedFilePaths; // Return empty list if no data
            }

            var sbFile = new StringBuilder();
            string baseFileName = Path.GetFileNameWithoutExtension(GetFileName(ediJob));
            string fileExtension = Path.GetExtension(GetFileName(ediJob));
            int filePart = 1;
            string currentContainer = listData[0].ContainerNum; // Initialize with the first container

            foreach (var lineData in listData)
            {
                // RULE 2: Check if LineItemNum is over 99 (e.g., 100, 199, etc.)

                if (lineData.LineItemNum > 1 && (lineData.LineItemNum - 1) % 99 == 0 && sbFile.Length > 0)
                {
                    // --- Save the current file ---
                    string partFileName = $"{baseFileName}-Part{filePart}{fileExtension}";
                    string fullPath = Path.Combine(storagePath, partFileName);
                    WriteFile(fullPath, sbFile.ToString());
                    generatedFilePaths.Add(fullPath);

                    // --- Start a new file ---
                    sbFile.Clear();
                    filePart++;
                }

                // RULE 1: Check if the container number has changed
                //if (lineData.ContainerNum != currentContainer && sbFile.Length > 0)
                //{
                //    sbFile.AppendLine(); // Add a blank line (line gap)
                //    currentContainer = lineData.ContainerNum;
                //}

                // Build and append the fixed-width line for the current record
                string fixedWidthLine = BuildFixedWitdthLine(lineData);
                if (!string.IsNullOrEmpty(fixedWidthLine))
                {
                    sbFile.AppendLine(fixedWidthLine); // AppendLine adds the record + a newline
                }
            }

            // After the loop, save any remaining data in the last file
            if (sbFile.Length > 0)
            {
                string partFileName = $"{baseFileName}-Part{filePart}{fileExtension}";
                string fullPath = Path.Combine(storagePath, partFileName);
                WriteFile(fullPath, sbFile.ToString());
                generatedFilePaths.Add(fullPath);
            }

            return generatedFilePaths;
        }

        /// <summary>
        /// Builds a single fixed-width line for the new EDI format.
        /// </summary>
        private string BuildFixedWitdthLine(NewEDIFormatLine data)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.Append("F");                                             // Pos 1, Len 1
                sb.Append(new string(' ', 9));                              // Pos 2, Len 9
                sb.Append(data.Date1.ToString("yyyyMMdd"));                 // Pos 11, Len 8
                sb.Append(PadString(data.WorkOrderNum, 12));                // Pos 19, Len 12
                sb.Append(new string(' ', 2));                              // Pos 31, Len 2
                sb.Append("0");                                             // Pos 33, Len 1
                sb.Append(data.Date2.ToString("yyyyMMdd"));                 // Pos 34, Len 8
                sb.Append(PadString(data.ContainerNum, 11));                // Pos 42, Len 11
                sb.Append(new string(' ', 59));                             // Pos 53, Len 60

                string formattedHours = data.LabourHours.ToString("00.00");
                if (formattedHours.Length > 5)
                {
                    formattedHours = "99.99"; // Cap at 5 chars
                }
                sb.Append(formattedHours);

                sb.Append(data.LineItemNum.ToString().PadLeft(2, '0'));     // Pos 117, Len 2
                sb.Append(PadString(data.RepairCode, 2));                   // Pos 119, Len 2
                sb.Append(new string(' ', 2));                              // Pos 121, Len 3
                string qtyString = ((int)data.Qty).ToString(); // Get integer part
                sb.Append(qtyString.Substring(qtyString.Length - 1, 1));    // Pos 123, Len 1 (Last digit of int)
                sb.Append(PadString(data.LocationCode, 4));                // Pos 124, Len 11
                sb.Append(PadDecimalWithZeros(data.MaterialCost, 6));                // Pos 179, Len 6
                sb.Append(new string(' ', 1));

                sb.Append(new string(' ', 4));                              // Pos 135, Len 4
                sb.Append(PadDecimal(0.00m, 4));                            // Pos 139, Len 4 (Hardcoded)
                sb.Append(new string(' ', 4));                              // Pos 143, Len 4
                sb.Append(PadDecimal(0.00m, 4));                            // Pos 147, Len 4 (Hardcoded)
                sb.Append(new string(' ', 8));                              // Pos 151, Len 8
                sb.Append(PadString("CMT", 3));                             // Pos 159, Len 3 (Hardcoded)
                sb.Append(new string(' ', 2));                              // Pos 162, Len 2
                sb.Append(PadDecimal(0.00m, 4));                            // Pos 164, Len 4 (Hardcoded)
                sb.Append(new string(' ', 11));                             // Pos 168, Len 11
                sb.Append(PadDecimalWithZeros(data.MaterialValue, 6));                // Pos 179, Len 6
                sb.Append(PadString(data.ResponsibleParty, 1));             // Pos 185, Len 1
                sb.AppendLine();
                return sb.ToString();
            }
            catch (Exception ex)
            {
                MNREDIService.Writefile($"Error building fixed-width line: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Helper to pad strings to the right with spaces.
        /// </summary>
        private string PadString(string value, int length)
        {
            if (value == null) value = "";
            // Use Substring to guarantee exact length in case 'value' is too long
            return value.PadRight(length).Substring(0, length);
        }

        /// <summary>
        /// Helper to format and pad decimals to the left with spaces.
        /// </summary>
        private string PadDecimal(decimal value, int length)
        {
            string formatted = value.ToString("F2"); // Format as "0.00"
            return formatted.PadLeft(length, ' ');
        }

        #endregion

        // Fetches details for MSK (Type 2) files
        public List<MNRMSKEDI> GetMSKEDIActivityDetails(long EDILogId)
        {
            var ListEDIContainerDtls = new List<MNRMSKEDI>();
            var parameters = new[] { new SqlParameter("@EDILogId", EDILogId) };

            using (SqlDataReader dr = SqlHelper.ExecuteReaderText("GetMSKEDIActivityDetails.sql", parameters))
            {
                while (dr.Read())
                {
                    var ObjEDIContainerDtls = new MNRMSKEDI
                    {
                        EDIId = dr.GetInt32(0),
                        GeoCodeTranslation = dr.GetString(1),
                        EstimateNo = dr.GetString(2),
                        EstimateDt = dr.GetString(3),
                        ContainerNo = dr.GetString(4),
                        Mode = dr.GetString(5),
                        DmgReason = dr.GetString(6),
                        LocationCode = dr.GetString(7),

                        // --- THIS IS THE FIX ---
                        // Read both string and decimal versions, as defined in your model
                        TotalLHrs = dr.GetString(8),
                        
                        // --- END FIX ---

                        OTHrs = dr.GetDecimal(9),
                        DTHrs = dr.GetDecimal(10),
                        MISCtHrs = dr.GetDecimal(11),
                        LabourAmount = dr.GetDecimal(12),
                        TotalValue = dr.GetString(13),
                        RpCode = dr.GetString(14),
                        CompCode = dr.GetString(15),
                        qty = dr.GetString(16),
                        rowTotal = dr.GetString(17),
                        rowLhrs = dr.GetString(18),
                        Responsible = dr.GetString(19),
                        PQt1 = dr.GetString(20),
                        PartNo = dr.GetString(21),
                        PQt2 = dr.GetString(22),
                        PartNo2 = dr.GetString(23),
                        PQt3 = dr.GetString(24),
                        PartNo3 = dr.GetString(25),
                        PQt4 = dr.GetString(26),
                        PartNo4 = dr.GetString(27),
                        PQt5 = dr.GetString(28),
                        PartNo5 = dr.GetString(29),
                        EstimateId = dr.GetInt32(30),
                        EstimateDTLId = dr.GetInt32(31),
                        Comments = dr.GetString(32)
                    };
                    // Re-populating decimal LabourHrs from the string TotalLHrs for consistency

                    decimal.TryParse(ObjEDIContainerDtls.TotalLHrs, out decimal lhrs);

                    ObjEDIContainerDtls.LabourHrs = lhrs;
                    ListEDIContainerDtls.Add(ObjEDIContainerDtls);
                }
            }
            return ListEDIContainerDtls;
        }

        // Fetches damage image details
        public List<MNRMSKEDI> GetDemageImageForEDI(long EDILogId)
        {
            var ListEDIContainerDtls = new List<MNRMSKEDI>();
            var parameters = new[] { new SqlParameter("@EDILogId", EDILogId) };

            using (SqlDataReader dr = SqlHelper.ExecuteReaderText("GetDemageImageForEDI.sql", parameters))
            {
                while (dr.Read())
                {
                    var ObjEDIContainerDtls = new MNRMSKEDI
                    {
                        EstimateId = dr.GetInt32(0),
                        BPhotes = dr.GetString(1),
                        ContainerNo = dr.GetString(2),
                        EDILogId = dr.GetInt32(3)
                    };
                    ListEDIContainerDtls.Add(ObjEDIContainerDtls);
                }
            }
            return ListEDIContainerDtls;
        }

        #endregion

        #region Database Write Methods

        // Kicks off the process to queue up EDI jobs
        public int ProcessEDI()
        {
            try
            {
               return SqlHelper.ExecuteNonQueryText("ManagingEDIDetails.sql");
            }
            catch (Exception ex)
            {
                MNREDIService.Writefile("Error in ProcessEDI: " + ex.Message);
                throw;
            }
        }

        // Updates the job status after queuing for delivery
        public void ManageEDILogStatus(long EDILogId, string Status, string fileName)
        {
            var parameters = new[]
            {
                new SqlParameter("@EDILogId", EDILogId),
                new SqlParameter("@Status", Status),
                new SqlParameter("@FileName", fileName)
            };
            SqlHelper.ExecuteNonQueryText("ManageEDIStatus.sql", parameters);
        }

        // Saves the delivery job to the dputEmailStructure table
        public void CreateEmailStructure(MNREDI ediJob, GTEmailLog emailLog)
        {
            
            int FTPID = 1740;   
            int SFTPID = 1744;  
            int EmailID = 1741; 

            if (!string.IsNullOrEmpty(ediJob.Email))
            {
                SaveEmailStruct(
                    sender: emailLog.Sender,
                    receiver: ediJob.Email,
                    subject: emailLog.Subject,
                    body: emailLog.Body,
                    filePath: emailLog.AttachedFile,
                    sendDate: emailLog.RequestDate,
                    password: emailLog.SenderPassword,
                    commTypeID: EmailID
                );
            }

            if (!string.IsNullOrEmpty(ediJob.FTPPath))
            {
                // This logic from your old code assumes FTP/SFTP paths are space-separated
                string[] ftpsftpList = ediJob.FTPPath.Split(' ');
                string[] sftpKeyList = (ediJob.SFTPKeyPath ?? "").Split('~');
                string[] sftpKeyFingerPrintList = (ediJob.SFTPHostKey ?? "").Split('~');

                int sftpIndex = 0;
                foreach (string path in ftpsftpList)
                {
                    if (string.IsNullOrEmpty(path)) continue;

                    string sftpurl = path.ToUpper();
                    if (sftpurl.StartsWith("FTP"))
                    {
                        // Save FTP job
                        SaveEmailStruct(
                            sender: emailLog.Sender,
                            receiver: path,
                            subject: "FTP",
                            body: emailLog.Body,
                            filePath: emailLog.AttachedFile,
                            sendDate: emailLog.RequestDate,
                            password: emailLog.SenderPassword,
                            commTypeID: FTPID
                        );
                    }
                    else
                    {
                        // Save SFTP job
                        SaveEmailStruct(
                            sender: emailLog.Sender,
                            receiver: path,
                            subject: $"SFTP - MNR EDI ID: {ediJob.EDIId}",
                            body: emailLog.Body,
                            filePath: emailLog.AttachedFile,
                            sendDate: emailLog.RequestDate,
                            password: emailLog.SenderPassword,
                            commTypeID: SFTPID,
                            keyPath: sftpIndex < sftpKeyList.Length ? sftpKeyList[sftpIndex] : "",
                            hostKey: sftpIndex < sftpKeyFingerPrintList.Length ? sftpKeyFingerPrintList[sftpIndex] : ""
                        );
                        sftpIndex++;
                    }
                }
            }

            // Finally, mark the original job as Processed
            ManageEDILogStatus(emailLog.ReferenceId, "S", Path.GetFileName(emailLog.AttachedFile));
        }

        // Private helper to call the SP to insert into dputEmailStructure
        private void SaveEmailStruct(string sender, string receiver, string subject, string body,
            string filePath, DateTime sendDate, string password, int commTypeID,
            string keyPath = "", string hostKey = "")
        {
            var parameters = new[]
            {
                new SqlParameter("@Sender", sender ?? ""),
                new SqlParameter("@Receiver", receiver ?? ""),
                new SqlParameter("@Subject", subject ?? ""),
                new SqlParameter("@Body", body ?? ""),
                new SqlParameter("@FilePath", filePath ?? ""),
                new SqlParameter("@SendDate", sendDate),
                new SqlParameter("@SenderPassword", password ?? ""),
                new SqlParameter("@CommunicationTypeID", commTypeID),
                new SqlParameter("@KeyPath", keyPath ?? ""),
                new SqlParameter("@HostKeyFingerprint", hostKey ?? ""),
                new SqlParameter("@ErrorMsg", SqlDbType.VarChar, 7) { Direction = ParameterDirection.Output }
            };

            SqlHelper.ExecuteNonQueryOutputResultText("InsertEmailStructure.sql", "@ErrorMsg", parameters);
        }

        #endregion

        #region File Generation Logic

        // Generates the filename for both file types
        private string GetFileName(MNREDI ediJob)
        {
            // This logic is copied from your old MNREDIService.cs
            // TODO: Your old code uses LogstarCache.GetDate(). We are replacing
            // it with DateTime.UtcNow. You must verify if this is correct.
            DateTime dateTime = DateTime.UtcNow; // Was LogstarCache.GetDate(EDILogId.GeoId);

            string strDate = dateTime.Year.ToString("0000") + dateTime.Month.ToString("00") + dateTime.Day.ToString("00");
            string strTime = dateTime.Hour.ToString("00") + dateTime.Minute.ToString("00") + dateTime.Second.ToString("00");

            string fileName = ediJob.Operator + ediJob.GeoCode + strDate.Substring(2, strDate.Length - 2) + strTime + "-" + ediJob.EDILogId.ToString();
            // The old code had this, but it seems redundant as it's never used.
            // string LenFileName = FileName.Remove(0, 7);

            return fileName + ".edi";
        }

        // Generates the standard EDIFACT file
        public string GenerateNewEDI(List<MNREDI> ListEDIContainerDtls, string storagePath, MNREDI ediJob)
        {
            string fileData = string.Empty;
            string fileName = GetFileName(ediJob);
            string filePath = Path.Combine(storagePath, fileName);

            fileData += ListEDIContainerDtls[0].UNB;
            fileData += ListEDIContainerDtls[0].A1;
            for (int i = 0; i < ListEDIContainerDtls.Count; i++)
            {
                fileData +=
                          ListEDIContainerDtls[i].UNH
                         + ListEDIContainerDtls[i].DTM
                         + ListEDIContainerDtls[i].RFF1
                         + ListEDIContainerDtls[i].RFF
                         + ListEDIContainerDtls[i].ACA
                         + ListEDIContainerDtls[i].LBR
                         + ListEDIContainerDtls[i].G1NAD
                         + ListEDIContainerDtls[i].G2NAD
                         + ListEDIContainerDtls[i].EQF
                         + ListEDIContainerDtls[i].ECI
                         + ListEDIContainerDtls[i].CUI
                         + ListEDIContainerDtls[i].DAM
                         + ListEDIContainerDtls[i].CTO
                         + ListEDIContainerDtls[i].TMA
                         + ListEDIContainerDtls[i].UNT;
            }
            fileData += ListEDIContainerDtls[0].UNZ;

            WriteFile(filePath, fileData);
            return fileName;
        }

        // Generates the MSK (Type 2) fixed-width file
        public string GenerateMSKEDI(List<MNRMSKEDI> ListEDIContainerDtls, string storagePath, MNREDI ediJob)
        {
            // This is a direct copy of the logic from your old MNREDIService.cs
            StringBuilder FileData = new StringBuilder();
            string fileName = GetFileName(ediJob);
            string filePath = Path.Combine(storagePath, fileName);

            string Header = string.Empty;
            string HD1TEXT = string.Empty;
            string HD2TEXT = string.Empty;
            string RPRTEXT = string.Empty;
            string PARTTEXT = string.Empty;
            string RMKTEXT = string.Empty;

            int SpaceCount = 0;
            int h1 = 0;
            int i = 0;

            var ContainerList = ListEDIContainerDtls.Select(e => new { e.EstimateId, e.GeoCodeTranslation })
                                 .Distinct().ToList();

            foreach (var Objcontainer in ContainerList)
            {
                var LstHD = (from a in ListEDIContainerDtls
                             where a.EstimateId == Objcontainer.EstimateId
                             // We group by the fields the HD class needs
                             group a by new { a.EstimateNo, a.DmgReason, a.Mode, a.ContainerNo, a.EstimateDt, a.LocationCode, a.TotalLHrs, a.TotalValue, a.Comments } into b
                             select new HD()
                             {
                                 ContainerNo = b.Key.ContainerNo,
                                 EstimateNo = b.Key.EstimateNo,
                                 DmgReason = b.Key.DmgReason,
                                 EstimateDt = b.Key.EstimateDt,
                                 LocationCode = b.Key.LocationCode,
                                 Mode = b.Key.Mode,
                                 TotalLHrs = b.Key.TotalLHrs, // Use the string TotalLHrs from the key
                                 Total = b.Key.TotalValue,
                                 LabourAmount = b.Sum(X => X.LabourAmount),
                                 LabourHrs = b.Sum(X => X.LabourHrs), // This will now work
                                 MISCtHrs = b.Sum(X => X.MISCtHrs),
                                 OTHrs = b.Sum(X => X.OTHrs),
                                 Comments = b.Key.Comments
                             }).Distinct().ToList();

                var LstRPR = (from a in ListEDIContainerDtls
                              where a.EstimateId == Objcontainer.EstimateId
                              select new RPR()
                              {
                                  CompCode = a.CompCode,
                                  Mode = a.Mode,
                                  LbrHour = a.LabourAmount, // This seems to be LabourAmount based on your old code
                                  MaterialAmount = a.MISCtHrs, // This seems to be MISCtHrs
                                  MiscTimeHrs = a.MISCtHrs,
                                  qty = a.qty,
                                  rowTotal = a.rowTotal,
                                  rowLhrs = a.rowLhrs,
                                  Responsible = a.Responsible,
                                  RpCode = a.RpCode,
                                  LocationCode = a.LocationCode,
                                  EstimateDtlId = a.EstimateDTLId
                              }).ToList();

                var LstPART = (from a in ListEDIContainerDtls
                               where a.EstimateId == Objcontainer.EstimateId && a.PartNo != ""
                               select new PART() { PartNo = a.PartNo, qty = a.PQt1, EstimateDtlId = a.EstimateDTLId }).ToList();
                var LstPART2 = (from a in ListEDIContainerDtls
                                where a.EstimateId == Objcontainer.EstimateId && a.PartNo2 != ""
                                select new PART() { PartNo = a.PartNo2, qty = a.PQt2, EstimateDtlId = a.EstimateDTLId }).ToList();
                var LstPART3 = (from a in ListEDIContainerDtls
                                where a.EstimateId == Objcontainer.EstimateId && a.PartNo3 != ""
                                select new PART() { PartNo = a.PartNo3, qty = a.PQt3, EstimateDtlId = a.EstimateDTLId }).ToList();
                var LstPART4 = (from a in ListEDIContainerDtls
                                where a.EstimateId == Objcontainer.EstimateId && a.PartNo4 != ""
                                select new PART() { PartNo = a.PartNo4, qty = a.PQt4, EstimateDtlId = a.EstimateDTLId }).ToList();
                var LstPART5 = (from a in ListEDIContainerDtls
                                where a.EstimateId == Objcontainer.EstimateId && a.PartNo5 != ""
                                select new PART() { PartNo = a.PartNo5, qty = a.PQt5, EstimateDtlId = a.EstimateDTLId }).ToList();

                Header = string.Empty;

                if (h1 == 0)
                {
                    string headerText = "CTL" + Objcontainer.GeoCodeTranslation + fileName.Substring(fileName.Length - 5, 5);
                    SpaceCount = 81 - headerText.Length;
                    Header = headerText + "".PadRight(SpaceCount, ' ');
                    FileData.Append(Header + Environment.NewLine);
                    h1++;
                }

                string LastMode = string.Empty;
                int j = 0;
                int InnerCount = 0, InnerCount2 = 0, InnerCount3 = 0, InnerCount4 = 0, InnerCount5 = 0;

                foreach (var HD in LstHD)
                {
                    if (HD.Mode == LastMode)
                    {
                        LastMode = HD.Mode;
                        continue;
                    }
                    LastMode = HD.Mode;

                    HD1TEXT = String.Empty;
                    HD2TEXT = String.Empty;
                    RMKTEXT = string.Empty;

                    string hd1Text = "HD1" + "MAER" + Objcontainer.GeoCodeTranslation + HD.EstimateDt + HD.ContainerNo + HD.Mode + HD.DmgReason + Convert.ToString("   ") + "W";
                    SpaceCount = 80 - hd1Text.Length;
                    HD1TEXT = hd1Text + "".PadRight(SpaceCount, ' ') + Environment.NewLine;

                    i = i + 1;
                    string sEstimateNo = HD.EstimateNo.Length > 27 ? HD.EstimateNo.Substring(0, 26) + i + HD.EstimateNo.Substring(27) : HD.EstimateNo;
                    string hd2Text = "HD2" + sEstimateNo + HD.TotalLHrs + "000000000000" + HD.Total;
                    SpaceCount = 80 - hd2Text.Length;
                    HD2TEXT = hd2Text + "".PadRight(SpaceCount, ' ');
                    FileData.Append(HD1TEXT + HD2TEXT + Environment.NewLine);

                    if (i == 1)
                    {
                        string rmkText = "RMK" + "    " + HD.Comments;
                        SpaceCount = 80 - rmkText.Length;
                        RMKTEXT = rmkText + "".PadRight(SpaceCount, ' ');
                        FileData.Append(RMKTEXT + Environment.NewLine);
                    }

                    var data = LstRPR.Where(a => a.Mode.Equals(HD.Mode));
                    foreach (var RPR in data)
                    {
                        RPRTEXT = string.Empty;
                        string rprText = "RPR" + RPR.RpCode + RPR.CompCode + RPR.LocationCode + RPR.qty + RPR.rowTotal + RPR.rowLhrs + RPR.Responsible;
                        SpaceCount = 80 - rprText.Length;
                        RPRTEXT = rprText + "".PadRight(SpaceCount, ' ');
                        FileData.Append(RPRTEXT + Environment.NewLine);

                        // Refactored PART logic to be cleaner
                        AppendPartData(LstPART, RPR.EstimateDtlId, ref InnerCount, FileData);
                        AppendPartData(LstPART2, RPR.EstimateDtlId, ref InnerCount2, FileData);
                        AppendPartData(LstPART3, RPR.EstimateDtlId, ref InnerCount3, FileData);
                        AppendPartData(LstPART4, RPR.EstimateDtlId, ref InnerCount4, FileData);
                        AppendPartData(LstPART5, RPR.EstimateDtlId, ref InnerCount5, FileData);

                        if ((LstHD.Count - 1) > j)
                        {
                            if (LstHD[j].Mode != LstHD[j + 1].Mode)
                            {
                                j++;
                                break;
                            }
                        }
                    }
                }
            }

            WriteFile(filePath, FileData.ToString());
            return fileName;
        }

        // Helper for MSK PART generation
        private void AppendPartData(List<PART> partList, long estimateDtlId, ref int innerCount, StringBuilder fileData)
        {
            var data = partList.Where(a => a.EstimateDtlId.Equals(estimateDtlId));
            foreach (var PART in data)
            {
                if (partList.Count > innerCount && partList.ElementAt(innerCount).Equals(PART))
                {
                    string partText = "PRT" + PART.qty + "    " + PART.PartNo;
                    int spaceCount = 80 - partText.Length;
                    string PARTTEXT = partText + "".PadRight(spaceCount, ' ');
                    fileData.Append(PARTTEXT + Environment.NewLine);
                    innerCount++;
                    break;
                }
            }
        }

        // Helper to write the file to disk
        private void WriteFile(string FilePath, string FileData)
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
                File.AppendAllText(FilePath, FileData);
                MNREDIService.Writefile($"Successfully wrote file: {FilePath}");
            }
            catch (Exception ex)
            {
                MNREDIService.Writefile($"Error writing file {FilePath}. Error: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}
