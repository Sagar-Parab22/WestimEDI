using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNRService.Models
{
    #region Models from ClsTransactions.cs

    public class rsResultSet
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Session { get; set; }
    }
    public class MNRTransactions
    {
        public List<PendingForAppWL> PendingForAppWL { get; set; }
        public List<PendingWorkOrderCompletion> PendingWorkOrderCompletion { get; set; }
        public List<PendingJobOrderCompletion> PendingJobOrderCompletion { get; set; }
        public List<MNRTransactions> MNRTransactions1 { get; set; } // ADDED BY SANKET 15JAN2018
        public List<PendingBatchEstimation> PendingBatchEstimation { get; set; }
        public List<PendingBatchEstimation> PendingBtEstimation { get; set; }
        public List<PendingEstimationCompletion> PendingEstimationCompletion { get; set; }
        public List<MNRSparePartList> mnrSparePartList { get; set; }
        public string ArtStartDateTime { get; set; }
        public string ArtEndDateTime { get; set; }
        public int GeoId { get; set; }
        public DataTable EstimateData { get; set; }
        public string AutoApproveContainers { get; set; }
        public Int32 EstimateId { get; set; }
        public Int32 EstimateDtlID { get; set; }
        public string EstimateIdEncode { get; set; }
        [Display(Name = "Container Number")]
        [Required(ErrorMessage = "Please Select ContainerNo")]
        public string ContainerNo { get; set; }
        public string OPR { get; set; }
        [Display(Name = "Container ISO Size/Type")]
        [Required(ErrorMessage = "Please Select Iso Size Type")]
        public string ISOSizeType { get; set; }
        public int ISOSizeTypeId { get; set; }
        [Required(ErrorMessage = "Please Select Condition Code")]
        public int DamageId { get; set; }
        [Display(Name = "Depot OPS Size/Type")]
        public string OPSSizeType { get; set; }
        public string IsRefCargo { get; set; }
        [Display(Name = "Depot Size/Type")]
        public string DepotSzTy { get; set; }
        [Display(Name = "Work Order Number")]
        public string WONo { get; set; }
        [Display(Name = "Work Order Status")]
        public string WOStatus { get; set; }
        [Display(Name = "Work Order Date")]
        public string WOOrderDate { get; set; }
        [Display(Name = "Estimate Number")]
        public string EstimateNo { get; set; }
        [Display(Name = "Estimate Type")]
        [Required(ErrorMessage = "Please Select Estimate Type")]
        public string EstimateType { get; set; }
        [Display(Name = "Estimate Currency(as required by Operator)")]
        public string EstimateCurrency { get; set; }
        [Display(Name = "Re-Estimate")]
        public string ReEstimate { get; set; }
        [Display(Name = "Estimate Done By")]
        public int EstimateDoneBy { get; set; }
        [Display(Name = "Estimated Completed On")]
        public string EstimateCompletedOn { get; set; }
        [Display(Name = "Operator Code/Name")]
        public string OperatorCodeName { get; set; }
        [Display(Name = "Material Charges(Operator Account)")]
        public decimal OPRMaterialCharges { get; set; }
        [Display(Name = "Material Charges(3rd Party Account)")]
        public decimal PartyMaterialCharges { get; set; }
        [Display(Name = "Labour Charges(Operator Account)")]
        public decimal OPRLabourCharges { get; set; }
        [Display(Name = "Labour Charges(3rd Party Account)")]
        public decimal PartyLabourCharges { get; set; }
        [Display(Name = "Material + Labour Charges(Total)")]
        public decimal MaterialLabourCharges { get; set; }
        [Display(Name = "JobOrder No.")]
        public string JobOrdNo { get; set; }  //SANKET 15JAn2018
        [Display(Name = "JobOrder Number")]
        public string JONo { get; set; }
        [Display(Name = "Job Order Number/Work Order Number")]
        public string JobWorkOrderNo { get; set; }
        [Display(Name = "Job Order Status")]
        public string JOStatus { get; set; }
        public Int32 AssignedTo { get; set; }
        [Display(Name = "Job Order Presently Assigned to")]
        public string JOBAssignedTo { get; set; }
        [Display(Name = "Job Order Assigned By")]
        public string JOBAssignedBy { get; set; }
        [Display(Name = "Job Order Assigned On")]
        public string JOBAssignedOn { get; set; }
        [Display(Name = "Job Order to be Re-Assigned to")]
        public string ReAssignedTo { get; set; }
        [Display(Name = "Assigned On")]
        public string AssignedOn { get; set; }
        [Display(Name = "Assigned By")]
        public Int32 AssignedBy { get; set; }
        public string ModeCode { get; set; }
        [Display(Name = "Repair Mode+Code/Description")]
        public string ModeCodeDesc { get; set; }
        public string Responsible { get; set; }
        public int UOMID { get; set; }
        public string UOMName { get; set; }
        public int Part1UOMID { get; set; }
        public string Part1UOMName { get; set; }
        public int Part2UOMID { get; set; }
        public string Part2UOMName { get; set; }
        public int Part3UOMID { get; set; }
        public string Part3UOMName { get; set; }
        public int Part4UOMID { get; set; }
        public string Part4UOMName { get; set; }
        public int Part5UOMID { get; set; }
        public string Part5UOMName { get; set; }
        public string PartUOMs { get; set; }
        public string IsRefrigerants { get; set; }
        public int PartUOMID { get; set; }
        public string PartUOMName { get; set; }
        [Display(Name = "Labour Hours")]
        public decimal? LabourHrs { get; set; }
        [Display(Name = "Currency")]
        public string Currency { get; set; }
        [Display(Name = "Total Charges for Job Order")]
        public decimal TotalCharges { get; set; }
        [Display(Name = "Location Code")]
        public string LocationCode { get; set; }
        [Display(Name = "Component Code")]
        public string ComponentCode { get; set; }
        public Int32 CompCodeID { get; set; }
        public Int32 LocationCodeID { get; set; }
        public Int32 DemageTypeID { get; set; }
        public Int32 RepairTypeID { get; set; }
        public string RP1 { get; set; }
        public string RP2 { get; set; }
        public string PartContractID { get; set; }
        public decimal SelectedPartQty { get; set; }
        public int SelectedPartUOM { get; set; }
        public int PartId1 { get; set; }
        public int PartId2 { get; set; }
        public int PartId3 { get; set; }
        public int PartId4 { get; set; }
        public int PartId5 { get; set; }
        [Display(Name = "Part Number1")]
        public string PartNo { get; set; }
        [Display(Name = "Part Number2")]
        public string PartNo2 { get; set; }
        [Display(Name = "Part Number3")]
        public string PartNo3 { get; set; }
        [Display(Name = "Part Number4")]
        public string PartNo4 { get; set; }
        [Display(Name = "Part Number5")]
        public string PartNo5 { get; set; }
        public string PartQty { get; set; }
        public decimal Quantity { get; set; }//Change int to decimal by kirti on 23 jan 2015
        [Display(Name = "Material Rate")]
        public decimal MaterialRate { get; set; }
        [Display(Name = "Material Charges for Job Order")]
        public decimal MaterialCharges { get; set; }
        [Display(Name = "Labour Charges for Job Order")]
        public decimal LabourCharges { get; set; }
        [Display(Name = "Condition Code")]
        public string ConditionCode { get; set; }
        [Display(Name = "Artisan Code/Name")]
        public string ArtisanCodeName { get; set; }
        [Display(Name = "Artisan's Start Date & Time")]
        public string ArtisanStartDate { get; set; }
        [Display(Name = "Artisan's End Date & Time")]
        public string ArtisanEndDate { get; set; }
        public string TimeSpent { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        [Display(Name = "Estimate Status")]
        public string EstimateStatus { get; set; }
        [Display(Name = "Quality Code")]
        public string QualityCode { get; set; }
        [Display(Name = "Physical Inflow Date & Time")]
        public string PhysicalInFlw { get; set; }
        [Display(Name = "CSC Plate Number")]
        [RegularExpression(@"^[0-9a-zA-Z]+$", ErrorMessage = "Please enter Valid  CSC Plate Number")]
        public string CSCPlateNo { get; set; }
        [Display(Name = "Year of Manufacture")]
        public string ManfMonthYr { get; set; }
        [Display(Name = "Container Payload (Kgs)")]
        public decimal? ContPayLoad { get; set; }
        [Display(Name = "Container Tare Weight (kgs)")]
        public decimal? ContTareWt { get; set; }
        [Display(Name = "ACEP Number")]
        [RegularExpression(@"^[0-9a-zA-Z]+$", ErrorMessage = "Please enter Valid ACEP Number")]
        public string ACEPNo { get; set; }
        [Display(Name = "EIR Number")]
        [StringLength(21, ErrorMessage = "The {0} must be minimum 18 and maximum 21 characters long.", MinimumLength = 18)]//Ranjeet 25-AUG -2014
        public string EIRNo { get; set; }
        [Display(Name = "Sent On")]
        public string SentOn { get; set; }
        [Display(Name = "Sent By")]
        public string SentBy { get; set; }
        [Display(Name = "Updated On")]
        public string CrtDate { get; set; }
        [Display(Name = "Updated By")]
        public string CrtBy { get; set; }
        public string CrtIp { get; set; }
        public bool BoolIsActive { get; set; }
        public string IsActive { get; set; }
        public int DepotEIRNo { get; set; }
        [Required(ErrorMessage = "Please Select Opeartor")]
        [Display(Name = "Operator Code/Name")]
        public int OperatorId { get; set; }
        [Display(Name = "Repair Mode+Code /Description ")]
        public string RepairModeCode { get; set; }
        public string RepairModeCodedesc { get; set; }
        [Display(Name = "Comp Code")]
        public string CompCode { get; set; }
        [Display(Name = "Qty")]
        public decimal Qty { get; set; }          //Int 32? change to decimal by kirti on 23 jan 2014
        public string Comments { get; set; }
        public string IsLiveReefer { get; set; }
        public int QualityId { get; set; }
        public string EstimateCategory { get; set; }
        [Display(Name = "Approval Updated By")]
        public string ApprovedBy { get; set; }
        [Display(Name = "Approval Updated On")]
        public string ApprovedOn { get; set; }
        public string StaffCode { get; set; }
        public int StaffId { get; set; }
        public string DamageCode { get; set; }
        public string DMGCode { get; set; }
        public int DMDamageId { get; set; }
        public int RepairSetItemId { get; set; }
        public int RepairSetId { get; set; }
        public int Max { get; set; }
        public string Flag { get; set; }
        public Decimal? LabourRate { get; set; }
        public DataTable Xml_value { get; set; }
        public string CompleteUpdate { get; set; }
        public string ContractStatus { get; set; }
        public string ContractStatus1 { get; set; }
        public string ContractStatus2 { get; set; }
        public string ContractStatus3 { get; set; }
        public string ContractStatus4 { get; set; }
        public string ContractStatus5 { get; set; }
        public decimal PartPrice { get; set; }//Added By Akash
        public decimal PartPrice1 { get; set; }//Added By Akash
        public decimal PartPrice2 { get; set; }//Added By Akash
        public decimal PartPrice3 { get; set; }//Added By Akash
        public decimal PartPrice4 { get; set; }//Added By Akash
        public decimal PartPrice5 { get; set; }//Added By Akash
        public string PartNoDB { get; set; }//Added By Akash
        [Display(Name = "Batch Number")]
        public string BatchNumber { get; set; }
        public string BatchStatus { get; set; }
        public int BatchId { get; set; }
        public string BatchIdEncode { get; set; }
        public string BatchStatusFull { get; set; }
        public string EstDoneBy { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime SysDate { get; set; }
        public DateTime WOAssignedDate { get; set; }
        public Int32 RequestId { get; set; } //[Akash] 07 -July -2014
        public int DepotDamageId { get; set; } //[Akash] 07 -July -2014
        public string ESTSTATUSTYPE { get; set; }
        public int DMCurrencyId { get; set; }
        [Display(Name = "Truck No")]
        public string TruckNo { get; set; }   //Ranjeet 19-AUG -2014
        [Display(Name = "Driver Name")]
        public string DriverName { get; set; }//Ranjeet 19-AUG -2014
        [Display(Name = "Transporter Name")]
        public string TranspName { get; set; }//Ranjeet 19-AUG -2014
        [Display(Name = "Driver LSC.No")]
        public string DriverLSCNo { get; set; }//Ranjeet 19-AUG -2014
        [Display(Name = "Container ISO Size/Type")]
        public Int32 SzTypeId { get; set; }   //Ranjeet 19-AUG -2014
        [Display(Name = "L(Length)")]
        public string Length { get; set; }
        [Display(Name = "W(Width)")]
        public string Width { get; set; }
        [Display(Name = "H(Height)")]
        public string Height { get; set; }
        [Display(Name = "Manufacturing Date")]
        public DateTime? MfgDate { get; set; }
        [Display(Name = "Manufacturer")]
        public string MFgCode { get; set; }
        public string MFgName { get; set; }
        public Nullable<int> MFgId { get; set; }
        [Display(Name = "Grade")]
        public string GrdCode { get; set; }
        public string GrdName { get; set; }
        public int ORD { get; set; }
        [Display(Name = "Qty")]
        public decimal PQt1 { get; set; }
        [Display(Name = "Qty")]
        public decimal PQt2 { get; set; }
        [Display(Name = "Qty")]
        public decimal PQt3 { get; set; }
        [Display(Name = "Qty")]
        public decimal PQt4 { get; set; }
        [Display(Name = "Qty")]
        public decimal PQt5 { get; set; }
        public decimal PartMaxQty { get; set; }
        public decimal PartQt1 { get; set; }
        public decimal PartQt2 { get; set; }
        public decimal PartQt3 { get; set; }
        public decimal PartQt4 { get; set; }
        public decimal PartQt5 { get; set; }
        public String GeoCode { get; set; }
        [Display(Name = "Container Damage Photos")]
        public String BPhotos { get; set; }
        public string PDFFile { get; set; }
        public string DriverNo { get; set; }
        public string ShipmentNo { get; set; }
        public string SealNo { get; set; }
        public string cirCycleType { get; set; }
        public String CIRRequestNo { get; set; }
        public string CIRLstDate { get; set; }
        public string Quantity1 { get; set; }
        public String Email { get; set; }
        [Display(Name = "Estimate Date")]
        public string EstimateDt { get; set; }
        public string PartCode { get; set; }
        public string MaxQty { get; set; }
        public string ActivityType { get; set; }
        public string Code { get; set; }
    }

    public class PendingForAppWL
    {
        public string ContainerNo { get; set; }
        public Int32 EstimateId { get; set; }
        public string OPR { get; set; }
        public string ISOSizeType { get; set; }
        public string EstimateNo { get; set; }
        public string EstimateType { get; set; }
        public string EstimateStatus { get; set; }
        public string ConditionCode { get; set; }
        public string QualityCode { get; set; }
        public string DepotSzTy { get; set; }
        public string EIRNo { get; set; }
        public string CrtDate { get; set; }
        public string CrtBy { get; set; }
    }

    public class PendingWorkOrderCompletion //Kirti
    {
        public Int32 EstimateID { get; set; }
        public int BatchId { get; set; }
        public string WONo { get; set; }
        public int StaffId { get; set; }
        public string EstimateStatus { get; set; }
        public string WOStatus { get; set; }
        public string AssignedTo { get; set; }
        public string AssignedOn { get; set; }
        public decimal LabourHrs { get; set; }
        public string ISOSizeType { get; set; }
        public string ContainerNo { get; set; }
        public string ArtisanCodeName { get; set; }
        public string ArtisanStartDate { get; set; }
        public string ArtisanEndDate { get; set; }
        public string CrtBy { get; set; }
        public string CrtDate { get; set; }
        public string TimeSpent { get; set; }
    }

    public class PendingJobOrderCompletion //RANJEET
    {
        public Int32 EstimateDtlID { get; set; }
        public string JONo { get; set; }
        public string JOStatus { get; set; }
        public string AssignedTo { get; set; }
        public int StaffId { get; set; }
        public string AssignedOn { get; set; }
        public string ModeCode { get; set; }
        public string Responsible { get; set; }
        public decimal LabourHrs { get; set; }
        public string Currency { get; set; }
        public decimal MaterialCharges { get; set; }
        public decimal LabourCharges { get; set; }
        public decimal TotalCharges { get; set; }
        public string ArtisanCodeName { get; set; }
        public string ArtisanStartDate { get; set; }
        public string ArtisanEndDate { get; set; }
        public string CrtBy { get; set; }
        public string CrtDate { get; set; }
        public string TimeSpent { get; set; }
        public decimal Qty { get; set; }
    }

    public class PendingEstimationCompletion
    {
        public Int32 EstimateId { get; set; }
        public Int32 EstimateDtlID { get; set; }
        public Int32 RepairSetItemId { get; set; }
        public string RepairModeCode { get; set; }
        public string RepairModeCodedesc { get; set; }
        public string LocationCode { get; set; }
        public string CompCode { get; set; }
        public string RP1 { get; set; }
        public string RP2 { get; set; }
        public decimal Qty { get; set; } //INt32 ? change to decimal by kirti on 23 jan 2015
        public string Max { get; set; }
        public string Responsible { get; set; }
        public string PartNo { get; set; }
        public string Comments { get; set; }
        public decimal? MaterialRate { get; set; }
        public decimal? LabourHrs { get; set; }
        public decimal MaterialValue { get; set; }
        public decimal LabourValue { get; set; }
        public decimal TotalValue { get; set; }
        public decimal LabourRate { get; set; }
        public decimal PartPrice { get; set; }
        public string PartNo2 { get; set; } //[AKASH] 29-10-2014
        public string PartNo3 { get; set; } //[AKASH] 29-10-2014
        public string PartNo4 { get; set; } //[AKASH] 29-10-2014
        public string PartNo5 { get; set; } //[AKASH] 29-10-2014
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal PQt1 { get; set; }
        public decimal PQt2 { get; set; }
        public decimal PQt3 { get; set; }
        public decimal PQt4 { get; set; }
        public decimal PQt5 { get; set; }
        public int UOMID { get; set; }
        public int Part1UOMID { get; set; }
        public int Part2UOMID { get; set; }
        public int Part3UOMID { get; set; }
        public int Part4UOMID { get; set; }
        public int Part5UOMID { get; set; }
    }

    public class PendingBatchEstimation
    {
        public Int32 EstimateId { get; set; }
        public string ContainerNo { get; set; }
        public string ISOSizeType { get; set; }
        public int ISOSizeTypeId { get; set; }
        public string LocationCode { get; set; }
        public string CompCode { get; set; }
        public string RP1 { get; set; }
        public string RP2 { get; set; }
        public decimal Qty { get; set; }//change to decimal by kirti on 23 jan 2015
        public Int32 Max { get; set; }
        public string Responsible { get; set; }
        public string PartNo { get; set; }
        public string Comments { get; set; }
        public decimal MaterialRate { get; set; }
        public decimal LabourHrs { get; set; }
        public decimal MaterialValue { get; set; }
        public decimal LabourValue { get; set; }
        public decimal TotalValue { get; set; }
        public decimal LabourRate { get; set; }
        public decimal PartPrice { get; set; }
        public string ModeCode { get; set; }
        public string Description { get; set; }
        public Int32 EstimateDtlID { get; set; } // ADDED BY SANKET JAN2018
        public int RepairSetItemId { get; set; }
    }

    public class PendingWorkOrderList
    {
        public int GeoId { get; set; }
        public Int32 EstimateId { get; set; }
        public Int32 EstimateDtlID { get; set; }
        public string JobNO { get; set; }
        public string CrtBy { get; set; }
        public string CrtIp { get; set; }
        public int AssignedTo { get; set; }
        public string Remark { get; set; }
    }

    public class MTBatchEstimateTmp
    {
        public string ContainerNo { get; set; }
        public string SzTypeID { get; set; }
        public string CompCode { get; set; }
        public string RP1Code { get; set; }
        public string RP2CODE { get; set; }
        public string PartNo { get; set; }
        public decimal Qty { get; set; } //change to decimal by kirti on 23 jan 2015
        public string Responsible { get; set; }
        public string Mode { get; set; }
        public string Code { get; set; }
        public string Location { get; set; }
        public string Comments { get; set; }
        public string Remarks { get; set; }
    }

    public class MNREDI
    {
        public int EDIId { get; set; }
        public int GeoId { get; set; }
        public int OperatorId { get; set; }
        public string Isactive { get; set; }
        public int EDITypeId { get; set; }
        public string CrtIp { get; set; }
        [Display(Name = "EDI Rule Active?")]
        public bool IsactiveNX { get; set; }
        [Required(ErrorMessage = "Please Enter To Address")]
        [RegularExpression(@"^((\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)*([,])*)*$", ErrorMessage = "Email Address Not Valid")]
        [Display(Name = "To Address")]
        public string ToAddress { get; set; }
        [Display(Name = "Frequency")]
        public string FrequencyDesc { get; set; }
        [Display(Name = "EDI Type")]
        public string EDIType { get; set; }
        [Required(ErrorMessage = "Please Enter Operator Geo Code Translation")]
        [Display(Name = "Operator Geo Code Translation")]
        public string GeoCodeTranslation { get; set; }
        [Display(Name = "Operator Code/Name")]
        public string OperatorName { get; set; }
        [Display(Name = "Operator Type")]
        public string OperatorType { get; set; }
        [Display(Name = "Empty Pool")]
        public string OperatorPool { get; set; }
        [Display(Name = "Updated By")]
        public string CrtBy { get; set; }
        [Display(Name = "Updated On")]
        public string CrtDate { get; set; }
        public int CommunicationID { get; set; }
        public string FTPPath { get; set; }
        public string SFTPKeyPath { get; set; }
        public string SFTPHostKey { get; set; }
        [Required(ErrorMessage = "Please Select Frequency")]
        public string FrequencyCode { get; set; }
        public string UNB { get; set; }
        public string A1 { get; set; }
        public string A2 { get; set; }
        public string A3 { get; set; }
        public string A4 { get; set; }
        public string A5 { get; set; }
        public string UNH { get; set; }
        public string DTM { get; set; }
        public string B1 { get; set; }
        public string RFF { get; set; }
        public string C1 { get; set; }
        public string ACA { get; set; }
        public string LBR { get; set; }
        public string G1NAD { get; set; }
        public string D1 { get; set; }
        public string D2 { get; set; }
        public string G2NAD { get; set; }
        public string E1 { get; set; }
        public string E2 { get; set; }
        public string ECI { get; set; }
        public string EQF { get; set; }
        public string DAM { get; set; }
        public string COS { get; set; }
        public string CTO { get; set; }
        public string TMA { get; set; }
        public string UNT { get; set; }
        public bool BUNZ { get; set; }//Kirti on 28/4/2015
        public bool BUNT { get; set; }
        public bool BUNB { get; set; }
        public bool BECI { get; set; }
        public bool BEQF { get; set; }
        public bool BDAM { get; set; }
        public bool BCOS { get; set; }
        public bool BCTO { get; set; }
        public bool BTMA { get; set; }
        public bool BG2NAD { get; set; }
        public bool BG1NAD { get; set; }
        public bool BACA { get; set; }
        public bool BLBR { get; set; }
        public bool BRFF { get; set; }
        public bool BRFF1 { get; set; }//Kirti on 8/4/2015
        public string RFF1 { get; set; }//Kirti on 8/4/2015
        public string Z1 { get; set; }//Kirti on 8/4/2015
        public bool BCUI { get; set; }//Kirti on 28/4/2015
        public string CUI { get; set; }//Kirti on 28/4/2015
        public string Z2 { get; set; }//Kirti on 28/4/2015
        public string UNZ { get; set; }//Kirti on 28/4/2015
        public bool BUNH { get; set; }
        public bool BDTM { get; set; }
        public Int32 EDILogId { get; set; }
        public string Email { get; set; }
        public string GeoCode { get; set; }
        public string Operator { get; set; }
    }

    public class MNREDISTRUCTURE
    {
        public int EDIId { get; set; }
        public int MsgId { get; set; }
        public string Isactive { get; set; }
        public string A1 { get; set; }
        public string A2 { get; set; }
        public string A3 { get; set; }
        public string A4 { get; set; }
        public string A5 { get; set; }
        public string UNH { get; set; }
        public string DTM { get; set; }
        public string B1 { get; set; }
        public string RFF { get; set; }
        public string C1 { get; set; }
        public string ACA { get; set; }
        public string LBR { get; set; }
        public string G1NAD { get; set; }
        public string D1 { get; set; }
        public string D2 { get; set; }
        public string G2NAD { get; set; }
        public string E1 { get; set; }
        public string E2 { get; set; }
        public string ECI { get; set; }
        public string EQF { get; set; }
        public string DAM { get; set; }
        public string COS { get; set; }
        public string CTO { get; set; }
        public string TMA { get; set; }
        public string UNT { get; set; }
        public string IsActive { get; set; }
        public string UNB { get; set; }
        public bool BUNT { get; set; }
        public bool BUNB { get; set; }
        public bool BECI { get; set; }
        public bool BEQF { get; set; }
        public bool BDAM { get; set; }
        public bool BCOS { get; set; }
        public bool BCTO { get; set; }
        public bool BTMA { get; set; }
        public bool BG2NAD { get; set; }
        public bool BG1NAD { get; set; }
        public bool BACA { get; set; }
        public bool BLBR { get; set; }
        public bool BRFF { get; set; }
        public bool BUNH { get; set; }
        public bool BDTM { get; set; }
    }

    public class MNRMSKEDI
    {
        public int EDIId { get; set; }
        public string GeoCodeTranslation { get; set; }
        public string EstimateNo { get; set; }
        public string EstimateDt { get; set; }
        public string Mode { get; set; }
        public string ContainerNo { get; set; }
        public string DmgReason { get; set; }
        public string LocationCode { get; set; }
        public decimal LabourHrs { get; set; }
        public decimal OTHrs { get; set; }
        public decimal DTHrs { get; set; }
        public decimal MISCtHrs { get; set; }
        public decimal LabourAmount { get; set; }
        public string RpCode { get; set; }
        public string CompCode { get; set; }
        public string qty { get; set; }
        public decimal MiscTimeHrs { get; set; }
        public decimal MaterialAmount { get; set; }
        public decimal LbrHour { get; set; }
        public string Responsible { get; set; }
        public string PartNo { get; set; }
        public Int32 EstimateId { get; set; }
        public string PQt1 { get; set; }
        public string PQt2 { get; set; }
        public string PQt3 { get; set; }
        public string PQt4 { get; set; }
        public string PQt5 { get; set; }
        public string PartNo2 { get; set; }
        public string PartNo3 { get; set; }
        public string PartNo4 { get; set; }
        public string PartNo5 { get; set; }
        public string TotalValue { get; set; }
        public Int32 EstimateDTLId { get; set; }
        public string TotalLHrs { get; set; }
        public decimal Total { get; set; }
        public string rowTotal { get; set; }
        public string rowLhrs { get; set; }
        public string Comments { get; set; }
        public string BPhotes { get; set; }
        public Int32 EDILogId { get; set; }
    }

    public class HD
    {
        public string EstimateNo { get; set; }
        public string EstimateDt { get; set; }
        public string Mode { get; set; }
        public string ContainerNo { get; set; }
        public string DmgReason { get; set; }
        public string LocationCode { get; set; }
        public decimal LabourHrs { get; set; }
        public decimal OTHrs { get; set; }
        public decimal DTHrs { get; set; }
        public decimal MISCtHrs { get; set; }
        public decimal LabourAmount { get; set; }
        public string TotalLHrs { get; set; }
        public string Total { get; set; }
        public string rowTotal { get; set; }
        public string rowLhrs { get; set; }
        public string Comments { get; set; }
    }
    public class HD1
    {
        public string EstimateNo { get; set; }
        public string EstimateDt { get; set; }
        public string Mode { get; set; }
        public string ContainerNo { get; set; }
        public string DmgReason { get; set; }
        public string LocationCode { get; set; }
    }

    public class HD2
    {
        public string DmgReason { get; set; }
        public string EstimateNo { get; set; }
        public decimal LabourHrs { get; set; }
        public decimal OTHrs { get; set; }
        public decimal DTHrs { get; set; }
        public decimal MISCtHrs { get; set; }
        public decimal LabourAmount { get; set; }
    }

    public class RPR
    {
        public string RpCode { get; set; }
        public string CompCode { get; set; }
        public string qty { get; set; }
        public decimal MiscTimeHrs { get; set; }
        public decimal MaterialAmount { get; set; }
        public decimal LbrHour { get; set; }
        public string Responsible { get; set; }
        public string LocationCode { get; set; }
        public Int32 EstimateDtlId { get; set; }
        public string rowTotal { get; set; }
        public string rowLhrs { get; set; }
        public string Mode { get; set; }
    }
    public class PART
    {
        public string qty { get; set; }
        public string PartNo { get; set; }
        public Int32 EstimateDtlId { get; set; }
    }

    public class QualityDamageUpate
    {
        public int QualityId { get; set; }
        public int DamageId { get; set; }
    }

    public class MNRCIROutput
    {
        public String CIRRequestNo { get; set; }
        public Int32 EstimateId { get; set; }
        public String Info { get; set; }
    }

    public class MNRCIRRequestData
    {
        public string CIRLstDate { get; set; }
        public string GeoCode { get; set; }
        public string OperatorCode { get; set; }
        public string ContainerNo { get; set; }
        public string ISOSizeType { get; set; }
        public string IsLiveReefer { get; set; }
        public string cirCycleType { get; set; }
        public string TruckNo { get; set; }
        public string DriverNo { get; set; }
        public string ShipmentNo { get; set; }
        public string SealNo { get; set; }
        public int OperatorId { get; set; }
        public int QualityId { get; set; }
        public string QualityCode { get; set; }
    }

    public class MNRSparePartList
    {
        public string Mode { get; set; }
        public string Code { get; set; }
        public string RepairDescription { get; set; }
        public string PartDescription { get; set; }
        public string PartCode { get; set; }
        public string Currency { get; set; }
        public string MfgCode { get; set; }
        public decimal MaxQty { get; set; }
        public int ID { get; set; }
        public decimal IssuanceQuantity { get; set; }
        public decimal ConsumedQuantity { get; set; }
        public decimal UnblockQuantity { get; set; }
        public bool IsRefrigerant { get; set; }
        public string IssuanceRemark { get; set; }
        public string ConsumedRemark { get; set; }
    }

    #endregion

    #region Missing GTEmailLog Model

    // This model was missing from ClsTransactions.cs but is required by the service logic
    public class GTEmailLog
    {
        public long ReferenceId { get; set; }
        public string Recipients { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string AttachedFile { get; set; }
        public char EmailStatus { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime EmailSent { get; set; }
        public string SenderPassword { get; set; }
    }

    #endregion

    public class NewEDIFormatLine
    {
        public DateTime Date1 { get; set; }
        public string WorkOrderNum { get; set; }
        public DateTime Date2 { get; set; }
        public string ContainerNum { get; set; }
        public decimal LabourHours { get; set; }
        public long LineItemNum { get; set; } // From ROW_NUMBER()
        public string RepairCode { get; set; }
        public string LocationCode { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal MaterialValue { get; set; }
        public string ResponsibleParty { get; set; }
        public decimal Qty { get; set; }
    }
}
