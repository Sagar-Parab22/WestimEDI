using MNRService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace MNRService
{
    public partial class MNREDIService : ServiceBase
    {
        private System.Timers.Timer MNREDITimer = new System.Timers.Timer();
        private ClsMNREDI _ediLogic;
        public MNREDIService()
        {
            this.ServiceName = "MNREDIService";
        }
        public void DebugOnStart()
        {
            OnStart(null);
        }
        public void DebugOnStop()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            MNREDITimer.AutoReset = false;
            try
            {
                // Initialize our logic class
                _ediLogic = new ClsMNREDI();

                Writefile("Service starting...");

                // Configure the timer
                MNREDITimer.Elapsed += new ElapsedEventHandler(MNREDITimer_Elapsed);

                // Get interval from App.config, default to 5 minutes (300000ms)
                int interval;
                if (!int.TryParse(ConfigurationManager.AppSettings["TimerInterval"], out interval))
                {
                    interval = 300000;
                }
                MNREDITimer.Interval = interval;

                Writefile($"Timer interval set to {interval}ms.");

                // Start the timer
                MNREDITimer.Enabled = true;
                MNREDITimer.AutoReset = false; // We set this to false to prevent re-entry

                // Run the first check immediately on start
                Writefile("Triggering initial run...");
                MNREDITimer_Elapsed(null, null);
            }
            catch (Exception ex)
            {
                Writefile("FATAL ERROR OnStart: " + ex.Message);
            }
        }

        protected override void OnStop()
        {
            try
            {
                Writefile("Service stopping...");
                MNREDITimer.Enabled = false;
                MNREDITimer.Stop();
            }
            catch (Exception ex)
            {
                Writefile("ERROR OnStop: " + ex.Message);
            }
        }

        private void MNREDITimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                MNREDITimer.Stop();
                Writefile("Step 1: MNREDITimer_Elapsed Started.");

                // --- 1. Process/Queue EDI Jobs ---
                Writefile("Step 2: Calling ProcessEDI (ManagingEDIDetails.sql)...");
                _ediLogic.ProcessEDI();
                Writefile("ProcessEDI complete.");

                // --- 2. Get Pending Jobs ---
                Writefile("Step 3: Calling GetEDILogId (GetEDILogId.sql)...");
                var listEDIMsgDtls = _ediLogic.GetEDILogId();
                Writefile($"Found {listEDIMsgDtls.Count} jobs to process.");

                // --- 3. Process Each Job ---
                foreach (var ediJob in listEDIMsgDtls)
                {
                    // Declare variables needed within the loop scope
                    string demageImage = ""; // Reset for each job
                    string[] splitDemageImageArray;
                    string[] fileDetails = null; // Initialize to null
                    bool fileGenerated = false; // Flag to track success

                    try
                    {
                        Writefile($"--- Processing EDILogId: {ediJob.EDILogId} (EDITypeId: {ediJob.EDITypeId}) ---");

                        // *** REMOVED: The check for EDILogId == 25 is GONE ***

                        string ediStoragePath = ConfigurationManager.AppSettings["MNREdiStorage"];

                        // *** CORE LOGIC: Check EDITypeId ***
                        if (ediJob.EDITypeId == 2)
                        {
                            // --- 4a. Process MSK (Fixed-Width) Format ---
                            Writefile("Type 2 (MSK) detected. Getting MSK details...");
                            var listMSKDetails = _ediLogic.GetMSKEDIActivityDetails(ediJob.EDILogId);
                            Writefile($"Found {listMSKDetails.Count} MSK detail lines.");

                            // --- Fetch Damage Images ONLY for MSK ---
                            var listDemageImages = _ediLogic.GetDemageImageForEDI(ediJob.EDILogId);
                            Writefile($"Found {listDemageImages.Count} damage image records.");
                            if (listDemageImages.Count > 0)
                            {
                                foreach (var imgRecord in listDemageImages)
                                {
                                    if (!string.IsNullOrEmpty(imgRecord.BPhotes))
                                    {
                                        splitDemageImageArray = imgRecord.BPhotes.Split('~');
                                        foreach (string s in splitDemageImageArray)
                                        {
                                            if (!string.IsNullOrEmpty(s))
                                            {
                                                demageImage += $"<img src='{s}' height='100px' width='100px'>";
                                            }
                                        }
                                    }
                                }
                                Writefile($"Generated {demageImage.Length} chars of image HTML.");
                            }
                            // --- End Damage Image Logic ---

                            if (listMSKDetails.Count > 0)
                            {
                                string fileName = _ediLogic.GenerateMSKEDI(listMSKDetails, ediStoragePath, ediJob);
                                fileDetails = new string[] { "MSK_EDI_DATA", fileName, Path.Combine(ediStoragePath, fileName) };
                                Writefile($"MSK file generated: {fileName}");
                                fileGenerated = true;
                            }
                            else
                            {
                                Writefile("No MSK details found, skipping file generation for this job.");
                                // No 'continue' here, let it proceed to queueing if needed,
                                // or add 'continue' if you want to skip queuing entirely.
                            }
                        }
                        else if (ediJob.EDITypeId == 3 || ediJob.EDITypeId == 1) // Type 3 = New Fixed-Width Format
                        {
                            // --- 4b. Process New Fixed-Width Format ---
                            Writefile($"Type 3 (New Format) detected. Getting new format details...");
                            var listNewFormatDetails = _ediLogic.GetNewEDIFormatDetails(ediJob.EDILogId);
                            Writefile($"Found {listNewFormatDetails.Count} new format detail lines.");

                            if (listNewFormatDetails.Count > 0)
                            {
                                // --- MODIFIED: This method now returns a LIST of file paths ---
                                List<string> generatedFilePaths = _ediLogic.GenerateNewFileFormat(listNewFormatDetails, ediStoragePath, ediJob);

                                if (generatedFilePaths.Count > 0)
                                {
                                    Writefile($"New format generated {generatedFilePaths.Count} file(s).");
                                    fileGenerated = true;
                                    // --- MODIFIED: Loop through each generated file and queue it ---
                                    int partNum = 1;
                                    foreach (string fullPath in generatedFilePaths)
                                    {
                                        DateTime geoDate = DateTime.UtcNow;
                                        string subject = $"MNR EDI for {ediJob.GeoCode} as on {geoDate:dd MMM yyyy} (Part {partNum})";

                                        GTEmailLog objMailLog = new GTEmailLog
                                        {
                                            ReferenceId = ediJob.EDILogId,
                                            AttachedFile = fullPath, // Use the full path for this specific file
                                            Body = "", // This format doesn't use damage images
                                            EmailSent = geoDate,
                                            EmailStatus = 'P',
                                            Recipients = ediJob.Email,
                                            Sender = ConfigurationManager.AppSettings["MNRUserName"],
                                            SenderPassword = ConfigurationManager.AppSettings["MNRPassword"],
                                            Subject = subject,
                                            RequestDate = geoDate
                                        };

                                        _ediLogic.CreateEmailStructure(ediJob, objMailLog);
                                        Writefile($"Successfully queued job {ediJob.EDILogId} (Part {partNum}) for delivery: {Path.GetFileName(fullPath)}");
                                        partNum++;
                                    }
                                }
                                else
                                {
                                    Writefile("File generation ran but produced 0 files.");
                                }
                            }
                            else
                            {
                                Writefile("No new format details found, skipping file generation for this job.");
                            }
                        }


                        else // --- If EDITypeId is NOT 2 ---
                        {
                            // --- 4b. Process Standard (EDIFACT) Format ---
                            Writefile($"Type {ediJob.EDITypeId} (Standard) detected. Getting EDI details...");
                            var listEDIDetails = _ediLogic.GetEDIActivityDetails(ediJob.EDILogId);
                            Writefile($"Found {listEDIDetails.Count} EDI detail lines.");

                            // Note: Damage images are NOT fetched or used for Standard format

                            if (listEDIDetails.Count > 0)
                            {
                                string fileName = _ediLogic.GenerateNewEDI(listEDIDetails, ediStoragePath, ediJob);
                                fileDetails = new string[] { "EDIFACT_DATA", fileName, Path.Combine(ediStoragePath, fileName) };
                                Writefile($"EDIFACT file generated: {fileName}");
                                fileGenerated = true;
                            }
                            else
                            {
                                Writefile("No EDI details found, skipping file generation for this job.");
                                // No 'continue' here, let it proceed to queueing if needed,
                                // or add 'continue' if you want to skip queuing entirely.
                            }
                        }

                        // --- 5. Queue for Delivery (Only if a file was successfully generated) ---
                        if (fileGenerated && fileDetails != null)
                        {
                            Writefile("Step 5: Queuing file for delivery...");
                            DateTime geoDate = DateTime.UtcNow; // Using UtcNow as GetDate replacement

                            GTEmailLog objMailLog = new GTEmailLog
                            {
                                ReferenceId = ediJob.EDILogId,
                                AttachedFile = fileDetails[2], // Full file path
                                Body = demageImage, // Will be empty string "" if not MSK
                                EmailSent = geoDate,
                                EmailStatus = 'P',
                                Recipients = ediJob.Email,
                                Sender = ConfigurationManager.AppSettings["MNRUserName"],
                                SenderPassword = ConfigurationManager.AppSettings["MNRPassword"],
                                Subject = $"MNR EDI for {ediJob.GeoCode} as on {geoDate:dd MMM yyyy}",
                                RequestDate = geoDate
                            };

                            _ediLogic.CreateEmailStructure(ediJob, objMailLog);
                            Writefile($"Successfully queued job {ediJob.EDILogId} for delivery.");
                        }
                        else if (!fileGenerated)
                        {
                            Writefile($"Skipping queuing for job {ediJob.EDILogId} as no file was generated.");
                            // Optionally, update the status to something like 'N' (No Data) or 'E' (Error) here
                            // _ediLogic.ManageEDILogStatus(ediJob.EDILogId, "N", "NoDetailsFound");
                        }

                    }
                    catch (Exception exJob)
                    {
                        Writefile($"!!! ERROR processing job {ediJob.EDILogId}: {exJob.Message} \nStackTrace: {exJob.StackTrace}");
                        // Update status to 'F' (Failed) when an error occurs during processing
                        try
                        {
                            _ediLogic.ManageEDILogStatus(ediJob.EDILogId, "F", "ProcessingError");
                        }
                        catch (Exception exStatus)
                        {
                            Writefile($"!!! FAILED TO UPDATE STATUS TO FAILED for job {ediJob.EDILogId}: {exStatus.Message}");
                        }
                    }
                } // End foreach loop

                Writefile("Step 6: All jobs processed.");
            }
            catch (Exception ex)
            {
                Writefile($"!!! FATAL ERROR in Timer_Elapsed main loop: {ex.Message} \nStackTrace: {ex.StackTrace}");
            }
            finally
            {
                // Restart the timer for the next run
                if (!MNREDITimer.Enabled)
                {
                    MNREDITimer.Start(); // Only restart AFTER all work is done
                    Writefile("Timer restarted for next interval.");
                }
            }
        }
        public static void Writefile(string msg)
        {
            try
            {
                string filePath = ConfigurationManager.AppSettings["MNRERRORLOGFILE"];
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = "C:\\Temp\\"; // Fallback directory
                }

                // Ensure the directory exists
                Directory.CreateDirectory(filePath);

                string fileName = "MNREDILog" + DateTime.Now.ToString("dd-MM-yy") + ".txt";
                string fullPath = Path.Combine(filePath, fileName);

                // Prepend timestamp to the message
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} :: {msg}{Environment.NewLine}";

                File.AppendAllText(fullPath, logEntry);
            }
            catch (Exception)
            {
                // We can't log an error about logging...
            }
        }
    }
}
