using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace NppPluginNET
{
    class InformationExtractor
    {

        private char SegmentSeparator = Convert.ToChar(30);//Dec (30) | Hex (1E)
        private char DataElementSeparator = Convert.ToChar(29);//Dec(29) | Hex (1D)
        private char ComponentDataElementSeparator = Convert.ToChar(28);//Dec (28) | Hex (1C)
        private char RepetitionSeparator = Convert.ToChar(31);//Dec (31) | Hex (1F)

        private char DecimalNotation = Convert.ToChar(46);//Dec (46) | Char (.) | Hex (2E)
        private char ReleaseIndicator = Convert.ToChar(32);//Dec (32) | Hex (20)

        private Dictionary<string, string[][]> descriptions = new Dictionary<string, string[][]>();
        private Dictionary<string, string[][]> seqDescriptions = new Dictionary<string, string[][]>();

        private string messageToProcess;

        public static InformationExtractor instance(string message, bool decode = false)
        {
            InformationExtractor extractor = new InformationExtractor();
            extractor.initDescriptions();
            extractor.initDescriptionsSequel();
            if (decode)
            {
                extractor.messageToProcess = new ASCIIEncoding().GetString(Convert.FromBase64String(message));
            }
            else
            {
                extractor.messageToProcess = message;
            }
            extractor.processMessage();

            return extractor;
        }

        private void initDescriptions()
        {
            descriptions.Add("UNA",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Component Data Element Separator", "Data Element Separator", "Decimal Notation", 
                                           "Release Indicator", "Repetition Separator", "Segment Separator"}));
            descriptions.Add("UIB",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Syntax Identifier", "Syntax Version Number", "Service Code Directory Version Number", 
                                           "Service Code Directory Controlling Agency"},
                            new string[] { "Dialogue Reference" },
                            new string[] { "Transaction Control Reference[MessageID]", "Initiator Reference Identifier[RelatesToMessageID]", 
                                           "Controlling Agency, Coded[TertiaryIdentifier]"},
                            new string[] { "Scenario Identifier" },
                            new string[] { "Dialogue Identifier" },
                            new string[] { "Sender ID – Level One[From]", "Level One ID Code Qualifier[From-Qualifier]", 
                                           "Sender ID – Level Two", "Sender ID – Level Three"},
                            new string[] { "Recipient ID – Level One[To]", "Level One ID Code Qualifier[To-Qualifier]", 
                                           "Recipient ID – Level Two", "Recipient ID – Level Three"},
                            new string[] { "Date[SentTime]", "Event Time[SentTime]" },
                            new string[] { "Duplicator Indicator" },
                            new string[] { "Test Indicator[TestMessage]" }
                            ));
            descriptions.Add("UIH",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Message Type", "Message Version Number", "Message Release Number", "Message Function",
                                           "Controlling Agency","Association Assigned Code[HighestVersionSupported]"},
                            new string[] { "RxReferenceNumber" },
                            new string[] { "Initiator Control Reference[PrescriberOrderNumber]", "Initiator Reference ID[DeliveredID]", 
                                           "Controlling Agency, Coded[AdditionalTraceNumber]","Responder Control Reference[AcknowledgementID]"},
                            new string[] { "Status of Transfer Interactive" },
                            new string[] { "Date[SentTime]", "Event Time[SentTime]", "Time Offset" },
                            new string[] { "Test Indicator" }
                            ));
            descriptions.Add("REQ",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Message Function[ChangeRequestType]" },
                            new string[] { "Code List Qualifier[ReturnReceipt]" },
                            new string[] { "Reference Number[RequestReferenceNumber]" },
                            new string[] { "Sender Identification[Level 2]" },
                            new string[] { "Sender Identification[Level 2]" },
                            new string[] { "Change of Prescription Status Flag[ChangeofPrescriptionStatusFlag]" },
                            new string[] { "Date/time Period Qualifier" },
                            new string[] { "Date or Quantity[CensusEffectiveDate]" },
                            new string[] { "Date/time Period Format Qualifier" }
                            ));
            descriptions.Add("RES",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Response Type" },
                            new string[] { "Code List Qualifier" },
                            new string[] { "Reference Number[ReferenceNumber]" },
                            new string[] { "Free Text[Note]" }
                            ));
            descriptions.Add("STS",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Status Type Code[Code]" },
                            new string[] { "Code List Qualifier[DescriptionCode]" },
                            new string[] { "Free Text[Description]" }
                            ));
            descriptions.Add("PVD-PROVIDER",
                valueToInsert(
                            new string[] { "Segment Code[Prescriber|Supervisor]" },
                            new string[] { "Provider Code[Prescriber|Supervisor]" },
                            new string[] { "Reference Number[Identification]", "Reference Qualifier" },
                            new string[] { "Healthcare Service Location" },
                            new string[] { "Agency Qualifier", "Provider Specialty[SpecialtyCode]", "Provider Specialty Code[Specialty]" },
                            new string[] { "Party Name[LastName]", "First Name[FirstName]", "Middle Name[MiddleName]", "Suffix[Suffix]", "Prefix[Prefix]" },
                            new string[] { "Postcode Identification" },
                            new string[] { "Party Name[ClinicName]" },
                            new string[] { "Street & No/PO Box[AddressLine1]", "City Name[City]", "State[State]", "Postal Code[ZipCode]",
                                           "Place/Location Qualifier[PlaceLocationQualifier]","Place Location[AddressLine2]"},
                            new string[] { "Communication Number[Number]", "Code List Qualifier[Qualifier]" },
                            new string[] { "Party Name[LastName]", "First Name[FirstName]", "Middle Name[MiddleName]", "Suffix[Suffix]", "Prefix[Prefix]" }
                            ));
            descriptions.Add("PVD-FACILITY",
                valueToInsert(
                            new string[] { "Segment Code[Facility]" },
                            new string[] { "Provider Code[Facility]" },
                            new string[] { "Reference Number[Identification]", "Reference Qualifier" },
                            new string[] { "Healthcare Service Location" },
                            new string[] { "Provider Specialty" },
                            new string[] { "Name" },
                            new string[] { "Postcode Identification" },
                            new string[] { "Party Name[FacilityName]" },
                            new string[] { "Street & No/PO Box[AddressLine1]", "City Name[City]", "State[State]", "Postal Code[ZipCode]",
                                           "Place/Location Qualifier[PlaceLocationQualifier]","Place Location[AddressLine2]"},
                            new string[] { "Communication Number[Number]", "Code List Qualifier[Qualifier]" },
                            new string[] { "Name" }
                            ));
            descriptions.Add("PVD-PHARMACY",
                valueToInsert(
                            new string[] { "Segment Code[Pharmacy]" },
                            new string[] { "Provider Code[Pharmacy]" },
                            new string[] { "Reference Number[Identification]", "Reference Qualifier" },
                            new string[] { "Healthcare Service Location" },
                            new string[] { "Agency Qualifier", "Provider Specialty[SpecialtyCode]", "Provider Specialty Code[Specialty]" },
                            new string[] { "Name[Pharmacist]", "Party Name[LastName]", "First Name[FirstName]", "Middle Name[MiddleName]", "Suffix[Suffix]", "Prefix[Prefix]" },
                            new string[] { "Postcode Identification" },
                            new string[] { "Party Name[StoreName]" },
                            new string[] { "Street & No/PO Box[AddressLine1]", "City Name[City]", "State[State]", "Postal Code[ZipCode]",
                                           "Place/Location Qualifier[PlaceLocationQualifier]","Place Location[AddressLine2]"},
                            new string[] { "Communication Number[Number]", "Code List Qualifier[Qualifier]" },
                            new string[] { "Name" }
                            ));
            descriptions.Add("PTT",
                valueToInsert(
                            new string[] { "Segment Code[Patient]" },
                            new string[] { "Relationship to Cardholder[PatientRelationship]" },
                            new string[] { "Date of Birth" },
                            new string[] { "Party Name[LastName]", "First Name[FirstName]", "Middle Name[MiddleName]", "Suffix[Suffix]", "Prefix[Prefix]" },
                            new string[] { "Gender" },
                            new string[] { "Reference Number[Identification]", "Reference Qualifier" },
                            new string[] { "Street & No/PO Box[AddressLine1]", "City Name[City]", "State[State]", "Postal Code[ZipCode]",
                                           "Place/Location Qualifier[PlaceLocationQualifier]","Place Location[AddressLine2]"},
                            new string[] { "Communication Number[Number]", "Code List Qualifier[Qualifier]" },
                            new string[] { "Facility Unit", "Room", "Bed" }
                            ));
            descriptions.Add("DRU",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Item Description Identification", "Item Description[DrugDescription]", "Item Number[DrugCoded.ProductCode]",
                                           "Code List Responsibility Agency[ProductCodeQualifier]","Code List Qualifier[DrugCoded.DosageForm]",
                                           "Free Text[DrugCoded.Strength]","Code List Qualifier[DrugCoded.StrengthUnits]","Reference Number[DrugCoded.DrugDBCode]",
                                           "Reference Qualifier[DrugCoded.DrugDBCodeQualifier]","Item Description[DrugDescription]","Item Description[DrugDescription]",
                                           "Item Description[DrugDescription]","Source Code List[FormSourceCode]","Item Form Code[FormCode]",
                                           "Source Code List[StrengthSourceCode]","Item Strength Code[StrengthCode]","DEA Schedule[DEASchedule]"},
                            new string[] { "Quantity Qualifier[Qualifier]","Quantity[Value]","Code List Qualifie[CodeListQualifier]","Source Code List[UnitSourceCode]",
                                           "Potency Unit Code[PotencyUnitCode]"},
                            new string[] { "Dosage ID", "Dosage[Directions]", "Dosage[Directions]" },
                            new string[] { "Date/time Period Qualifier", "Date or Quantity", "Date/time Period Format Qualifier" },
                            new string[] { "Product/Service Substitution" },
                            new string[] { "Quantity Qualifier[Qualifier]", "Quantity[Value]" },
                            new string[] { "Clinical Information Qualifier", "Clinical Information – Primary", "Code List Qualifier", "Clinical Information – Secondary", "Code List Qualifier" },
                            new string[] { "Reference Number[Value]", "Reference Qualifier[Qualifier]" },
                            new string[] { "Free Text[Note]" },
                            new string[] { "DUE Reason for Service Code","DUE Professional Service Code","DUE Result of Service Code","DUE Co-Agent ID","DUE Co-Agent ID Qualifier",
                                           "DUE Clinical Significance Code","DUE Acknowledgement Reason"},
                            new string[] { "Drug Coverage Status Code" },
                            new string[] { "Prior Authorization Status" },
                            new string[] { "Do Not Fill/Profile Flag" },
                            new string[] { "Date/time Period Qualifier", "Date or Quantity", "Date/time Period Format Qualifier" },
                            new string[] { "Time Zone Identifier", "Time Zone Difference Quantity" },
                            new string[] { "Needed No Later Than Reason" }
                            ));
            descriptions.Add("SIG",
                valueToInsert(
                            new string[] { "Segment Code[StructuredSIG]" },
                            new string[] { "Sig Sequence Position Number", "Multiple Sig Modifier" },
                            new string[] { "SNOMED Version", "FMT Version" },
                            new string[] { "Sig Free Text String Indicator", "Sig Free Text" },
                            new string[] { "Dose Composite Indicator", "Dose Delivery Method Text","Dose Delivery Method Code Qualifier","Dose Delivery Method Code",
                                           "Dose Delivery Method Modifier Text","Dose Delivery Method Modifier Code Qualifier","Dose Delivery Method Modifier Code",
                                           "Dose Quantity","Dose Form Text","Dose Form Code Qualifier","Dose Form Code","Dose Range Modifier"},
                            new string[] { "Dosing Basis Numeric Value","Dosing Basis Unit of Measure Text","Dosing Basis Unit of Measure Code Qualifier",
                                           "Dosing Basis Unit of Measure Code","Body Metric Qualifier","Body Metric Value","Calculated Dose Numeric",
                                           "Calculated Dose Unit of Measure Text","Calculated Dose Unit of Measure Code Qualifier","Calculated Dose Unit of Measure Code",
                                           "Dosing Basis Range Modifier"},
                            new string[] { "Vehicle Name","Vehicle Name Code Qualifier","Vehicle Name Code","Vehicle Quantity","Vehicle Unit Of Measure Text",
                                           "Vehicle Unit Of Measure Code Qualifier","Vehicle Unit Of Measure Code","Multiple Vehicle Modifier" },
                            new string[] { "Route of Administration Text", "Route of Administration Code Qualifier", "Route of Administration Code", "Multiple Route of Administration Modifier" },
                            new string[] { "Site of Administration Text", "Site of Administration Code Qualifier", "Site of Administration Code", "Multiple Site of Administration Timing Modifier" },
                            new string[] { "Administration Timing Text","Administration Timing Code Qualifier","Administration Timing Code","Multiple Administration Timing Modifier",
                                           "Rate of Administration","Rate Unit of Measure Text","Rate Unit of Measure Code Qualifier","Rate Unit of Measure Code",
                                           "Time Period Basis Text","Time Period Basis Code Qualifier","Time Period Basis Code","Frequency Numeric Value","Frequency Units Text",
                                           "Frequency Units Code Qualifier","Frequency Units Code","Variable Frequency Modifier","Interval Numeric Value","Interval Units Text",
                                           "Interval Units Code Qualifier","Interval Units Code","Variable Interval Modifier"},
                            new string[] { "Duration Numeric Value", "Duration Text", "Duration Text Code Qualifier", "Duration Text Code" },
                            new string[] { "Maximum Dose Restriction Numeric Value","Maximum Dose Restriction Units Text","Maximum Dose Restriction Code Qualifier",
                                           "Maximum Dose Restriction Units Code","Maximum Dose Restriction Variable Numeric Value", "Maximum Dose Restriction Variable Units Text",
                                           "Maximum Dose Restriction Variable Units Code Qualifier", "Maximum Dose Restriction Variable Units Code", "Maximum Dose Restriction Variable Duration Modifier"},
                            new string[] { "Indication Precursor Text" ,"Indication Precursor Code Qualifier","Indication Precursor Code","Indication Text","Indication Text Code Qualifier",
                                           "Indication Text Code","Indication Value Text","Indication Value Unit","Indication Value Unit of Measure Text",
                                           "Indication Value Unit of Measure Code Qualifier","Indication Value Unit of Measure Code","Indication Variable Modifier"},
                            new string[] { "Stop Indicator" }
                            ));
            descriptions.Add("OBS",
                valueToInsert(
                            new string[] { "Segment Code[Observation]" },
                            new string[] { "Measurement Dimension", "Measurement Value", "Measurement Unit Qualifier", "Date/Time/Period", "Date/Time/Period Format",
                                           "Measurement Data Qualifier","Source Code List","Measurement Unit Code"},
                            new string[] { "Free Text[ObservationNotes]" }
                            ));
            descriptions.Add("COO",
                valueToInsert(
                            new string[] { "Segment Code[BenefitsCoordination]" },
                            new string[] { "Reference Number[PayerIdentification]", "Reference Qualifier" },
                            new string[] { "Party Name[PayerName]" },
                            new string[] { "Service Type Code" },
                            new string[] { "Reference Number[CardholderID]", "Reference Qualifier" },
                            new string[] { "Party Name[LastName]", "First Name[FirstName]", "Middle Name[MiddleName]", "Suffix[Suffix]", "Prefix[Prefix]" },
                            new string[] { "Reference Number[GroupID]" },
                            new string[] { "Party Name[GroupName]" },
                            new string[] { "Street & No/PO Box[AddressLine1]", "City Name[City]", "State[State]", "Postal Code[ZipCode]",
                                           "Place/Location Qualifier[PlaceLocationQualifier]","Place Location[AddressLine2]"},
                            new string[] { "Date" },
                            new string[] { "Insurance Type Code" },
                            new string[] { "Address" },
                            new string[] { "Reference Number" },
                            new string[] { "Condition/Response Coded" },
                            new string[] { "Patient Identifier" },
                            new string[] { "Communication Number", "Code List Qualifier" }
                            ));
            descriptions.Add("UIT",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Message Reference Number" },
                            new string[] { "Number of Segments in Message" }
                            ));
            descriptions.Add("UIZ",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Dialogue Reference" },
                            new string[] { "Interchange Control Count" },
                            new string[] { "Duplicator Indicator" }
                            ));
        }

        private void initDescriptionsSequel()
        {
            seqDescriptions.Add("UNA",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Component Data Element Separator", "Data Element Separator", "Decimal Notation", 
                                           "Release Indicator", "Repetition Separator", "Segment Separator"}));
            seqDescriptions.Add("UIB",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Syntax Identifier", "Syntax Version Number", "Service Code Directory Version Number", 
                                           "Service Code Directory Controlling Agency"},
                            new string[] { "Dialogue Reference" },
                            new string[] { "MessageID[SEQUEL_HTTP_SEQ.NEXTVAL]", "Initiator Reference Identifier[RelatesToMessageID]", 
                                           "Controlling Agency, Coded[TertiaryIdentifier]"},
                            new string[] { "Scenario Identifier" },
                            new string[] { "Dialogue Identifier" },
                            new string[] { "STS_PROVIDER_INFO[SP1_NUM]", "Level One ID Code Qualifier[From-Qualifier]", 
                                           "Sender ID – Level Two", "Sender ID – Level Three"},
                            new string[] { "RX_OBS_MEDICATIONS_XML[NCPDP_ID]", "Level One ID Code Qualifier[To-Qualifier]", 
                                           "Recipient ID – Level Two", "Recipient ID – Level Three"},
                            new string[] { "Date[SentTime]", "Event Time[SentTime]" },
                            new string[] { "Duplicator Indicator" },
                            new string[] { "Test Indicator[TestMessage]" }
                            ));
            seqDescriptions.Add("UIH",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Message Type", "Message Version Number", "Message Release Number", "Message Function",
                                           "Controlling Agency","Association Assigned Code[HighestVersionSupported]"},
                            new string[] { "RX_OBS_MEDICATIONS_XML[RX_REFERENCE_NUM]" },
                            new string[] { "RX_OBS_MEDICATIONS_XML[EDI_CLIENT_IDcSEQ_NUM]", "Initiator Reference ID[DeliveredID]", 
                                           "Controlling Agency, Coded[AdditionalTraceNumber]","Responder Control Reference[AcknowledgementID]"},
                            new string[] { "Status of Transfer Interactive" },
                            new string[] { "Date[SentTime]", "Event Time[SentTime]", "Time Offset" },
                            new string[] { "Test Indicator" }
                            ));
            seqDescriptions.Add("REQ",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Message Function[ChangeRequestType]" },
                            new string[] { "Code List Qualifier[ReturnReceipt]" },
                            new string[] { "Reference Number[RequestReferenceNumber]" },
                            new string[] { "Sender Identification[Level 2]" },
                            new string[] { "Sender Identification[Level 2]" },
                            new string[] { "Change of Prescription Status Flag[ChangeofPrescriptionStatusFlag]" },
                            new string[] { "Date/time Period Qualifier" },
                            new string[] { "Date or Quantity[CensusEffectiveDate]" },
                            new string[] { "Date/time Period Format Qualifier" }
                            ));
            seqDescriptions.Add("RES",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Response Type" },
                            new string[] { "Code List Qualifier" },
                            new string[] { "Reference Number[ReferenceNumber]" },
                            new string[] { "Free Text[Note]" }
                            ));
            seqDescriptions.Add("STS",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Status Type Code[Code]" },
                            new string[] { "Code List Qualifier[DescriptionCode]" },
                            new string[] { "Free Text[Description]" }
                            ));
            seqDescriptions.Add("PVD-PROVIDER",
                valueToInsert(
                            new string[] { "Segment Code[Prescriber|Supervisor]" },
                            new string[] { "Provider Code[Prescriber|Supervisor]" },
                            new string[] { "(STS_PROVIDER_INFO[SP1_NUM]|RX_OBS_MEDICATIONS_XML[DEA_NUM|NPI])+", "Reference Qualifier" },
                            new string[] { "Healthcare Service Location" },
                            new string[] { "Agency Qualifier", "Provider Specialty[SpecialtyCode]", "Provider Specialty Code[Specialty]" },
                            new string[] { "RX_OBS_MEDICATIONS_XML[PROVIDER_LAST_NAME]", "RX_OBS_MEDICATIONS_XML[PROVIDER_FIRST_NAME]", "Middle Name[MiddleName]", "Suffix[Suffix]", "Prefix[Prefix]" },
                            new string[] { "Postcode Identification" },
                            new string[] { "Party Name[ClinicName]" },
                            new string[] { "RX_OBS_MEDICATIONS_XML[PROVIDER_ADDRESS1]", "RX_OBS_MEDICATIONS_XML[PROVIDER_CITY]", "RX_OBS_MEDICATIONS_XML[PROVIDER_STATE]", "RX_OBS_MEDICATIONS_XML[PROVIDER_ZIPCODE]",
                                           "Place/Location Qualifier[PlaceLocationQualifier]","Place Location[AddressLine2]"},
                            new string[] { "RX_OBS_MEDICATIONS_XML[CONTACT_NUM|PROVIDER_OFFICE_TEL]", "Code List Qualifier[Qualifier]" },
                            new string[] { "Party Name[LastName]", "First Name[FirstName]", "Middle Name[MiddleName]", "Suffix[Suffix]", "Prefix[Prefix]" }
                            ));
            seqDescriptions.Add("PVD-FACILITY",
                valueToInsert(
                            new string[] { "Segment Code[Facility]" },
                            new string[] { "Provider Code[Facility]" },
                            new string[] { "Reference Number[Identification]", "Reference Qualifier" },
                            new string[] { "Healthcare Service Location" },
                            new string[] { "Provider Specialty" },
                            new string[] { "Name" },
                            new string[] { "Postcode Identification" },
                            new string[] { "Party Name[FacilityName]" },
                            new string[] { "Street & No/PO Box[AddressLine1]", "City Name[City]", "State[State]", "Postal Code[ZipCode]",
                                           "Place/Location Qualifier[PlaceLocationQualifier]","Place Location[AddressLine2]"},
                            new string[] { "Communication Number[Number]", "Code List Qualifier[Qualifier]" },
                            new string[] { "Name" }
                            ));
            seqDescriptions.Add("PVD-PHARMACY",
                valueToInsert(
                            new string[] { "Segment Code[Pharmacy]" },
                            new string[] { "Provider Code[Pharmacy]" },
                            new string[] { "RX_OBS_MEDICATIONS_XML[NCPDP_ID]", "Reference Qualifier" },
                            new string[] { "Healthcare Service Location" },
                            new string[] { "Agency Qualifier", "Provider Specialty[SpecialtyCode]", "Provider Specialty Code[Specialty]" },
                            new string[] { "Name[Pharmacist]", "Party Name[LastName]", "First Name[FirstName]", "Middle Name[MiddleName]", "Suffix[Suffix]", "Prefix[Prefix]" },
                            new string[] { "Postcode Identification" },
                            new string[] { "RX_OBS_MEDICATIONS_XML[PHARMACY_NAME]" },
                            new string[] { "RX_OBS_MEDICATIONS_XML[PHARMACY_ADDRESS1]", "RX_OBS_MEDICATIONS_XML[PHARMACY_CITY]", "RX_OBS_MEDICATIONS_XML[PHARMACY_STATE]", "RX_OBS_MEDICATIONS_XML[PHARMACY_ZIPCODE]",
                                           "Place/Location Qualifier[PlaceLocationQualifier]","Place Location[AddressLine2]"},
                            new string[] { "RX_OBS_MEDICATIONS_XML[PHARMACY_CONTACT_NUM]", "Code List Qualifier[Qualifier]" },
                            new string[] { "Name" }
                            ));
            seqDescriptions.Add("PTT",
                valueToInsert(
                            new string[] { "Segment Code[Patient]" },
                            new string[] { "Relationship to Cardholder[PatientRelationship]" },
                            new string[] { "RX_OBS_MEDICATIONS_XML[DOB]" },
                            new string[] { "RX_OBS_MEDICATIONS_XML[LAST_NAME]", "RX_OBS_MEDICATIONS_XML[FIRST_NAME]", "Middle Name[MiddleName]", "Suffix[Suffix]", "Prefix[Prefix]" },
                            new string[] { "RX_OBS_MEDICATIONS_XML[SEX]" },
                            new string[] { "RX_OBS_MEDICATIONS_XML_DETAIL[MEMBER_PBM_ID]*|RX_OBS_MEDICATIONS_XML[SSN]", "Reference Qualifier" },
                            new string[] { "RX_OBS_MEDICATIONS_XML[ADDRESS1]", "RX_OBS_MEDICATIONS_XML[CITY]", "RX_OBS_MEDICATIONS_XML[STATE]", "RX_OBS_MEDICATIONS_XML[ZIPCODE]",
                                           "Place/Location Qualifier[PlaceLocationQualifier]","RX_OBS_MEDICATIONS_XML[ADDRESS2]"},
                            new string[] { "RX_OBS_MEDICATIONS_XML[TEL_NUM|WORK_TEL_NUM]", "Code List Qualifier[Qualifier]" },
                            new string[] { "Facility Unit", "Room", "Bed" }
                            ));
            seqDescriptions.Add("DRU",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Item Description Identification", "RX_OBS_MEDICATIONS_XML_DETAIL[MED_TRADE_NAME]", "RX_OBS_MEDICATIONS_XML_DETAIL[NDCID]",
                                           "Code List Responsibility Agency[ProductCodeQualifier]","Code List Qualifier[DrugCoded.DosageForm]",
                                           "RX_OBS_MEDICATIONS_XML_DETAIL[VOID_COMMENTS]","Code List Qualifier[DrugCoded.StrengthUnits]","Reference Number[DrugCoded.DrugDBCode]",
                                           "Reference Qualifier[DrugCoded.DrugDBCodeQualifier]","Item Description[DrugDescription]","Item Description[DrugDescription]",
                                           "Item Description[DrugDescription]","Source Code List[FormSourceCode]","Item Form Code[FormCode]",
                                           "Source Code List[StrengthSourceCode]","Item Strength Code[StrengthCode]","DEA Schedule[DEASchedule]"},
                            new string[] { "RX_OBS_MEDICATIONS_XML[SS_CODE]","RX_OBS_MEDICATIONS_XML_DETAIL[RX_QUANTITY]","Code List Qualifie[CodeListQualifier]","Source Code List[UnitSourceCode]",
                                           "Potency Unit Code[PotencyUnitCode]"},
                            new string[] { "Dosage ID", "RX_OBS_MEDICATIONS_XML_DETAIL[SIG_TEXT]", "Dosage[Directions]" },
                            new string[] { "Date/time Period Qualifier", "Now[yyyyMMdd]|RX_OBS_MEDICATIONS_XML_DETAIL[DAYS_SUPPLY]*", "Date/time Period Format Qualifier" },
                            new string[] { "RX_OBS_MEDICATIONS_XML_DETAIL[RX_DAW_FLAG]" },
                            new string[] { "Quantity Qualifier[Qualifier]", "RX_OBS_MEDICATIONS_XML_DETAIL[REFILLS_ALLOWED]" },
                            new string[] { "Clinical Information Qualifier", "Clinical Information – Primary", "Code List Qualifier", "Clinical Information – Secondary", "Code List Qualifier" },
                            new string[] { "Reference Number[Value]", "Reference Qualifier[Qualifier]" },
                            new string[] { "RX_OBS_MEDICATIONS_XML_DETAIL[PHARMACY_INSTRUCTIONS]" },
                            new string[] { "DUE Reason for Service Code","DUE Professional Service Code","DUE Result of Service Code","DUE Co-Agent ID","DUE Co-Agent ID Qualifier",
                                           "DUE Clinical Significance Code","DUE Acknowledgement Reason"},
                            new string[] { "Drug Coverage Status Code" },
                            new string[] { "Prior Authorization Status" },
                            new string[] { "Do Not Fill/Profile Flag" },
                            new string[] { "Date/time Period Qualifier", "Date or Quantity", "Date/time Period Format Qualifier" },
                            new string[] { "Time Zone Identifier", "Time Zone Difference Quantity" },
                            new string[] { "Needed No Later Than Reason" }
                            ));
            seqDescriptions.Add("SIG",
                valueToInsert(
                            new string[] { "Segment Code[StructuredSIG]" },
                            new string[] { "Sig Sequence Position Number", "Multiple Sig Modifier" },
                            new string[] { "SNOMED Version", "FMT Version" },
                            new string[] { "Sig Free Text String Indicator", "Sig Free Text" },
                            new string[] { "Dose Composite Indicator", "Dose Delivery Method Text","Dose Delivery Method Code Qualifier","Dose Delivery Method Code",
                                           "Dose Delivery Method Modifier Text","Dose Delivery Method Modifier Code Qualifier","Dose Delivery Method Modifier Code",
                                           "Dose Quantity","Dose Form Text","Dose Form Code Qualifier","Dose Form Code","Dose Range Modifier"},
                            new string[] { "Dosing Basis Numeric Value","Dosing Basis Unit of Measure Text","Dosing Basis Unit of Measure Code Qualifier",
                                           "Dosing Basis Unit of Measure Code","Body Metric Qualifier","Body Metric Value","Calculated Dose Numeric",
                                           "Calculated Dose Unit of Measure Text","Calculated Dose Unit of Measure Code Qualifier","Calculated Dose Unit of Measure Code",
                                           "Dosing Basis Range Modifier"},
                            new string[] { "Vehicle Name","Vehicle Name Code Qualifier","Vehicle Name Code","Vehicle Quantity","Vehicle Unit Of Measure Text",
                                           "Vehicle Unit Of Measure Code Qualifier","Vehicle Unit Of Measure Code","Multiple Vehicle Modifier" },
                            new string[] { "Route of Administration Text", "Route of Administration Code Qualifier", "Route of Administration Code", "Multiple Route of Administration Modifier" },
                            new string[] { "Site of Administration Text", "Site of Administration Code Qualifier", "Site of Administration Code", "Multiple Site of Administration Timing Modifier" },
                            new string[] { "Administration Timing Text","Administration Timing Code Qualifier","Administration Timing Code","Multiple Administration Timing Modifier",
                                           "Rate of Administration","Rate Unit of Measure Text","Rate Unit of Measure Code Qualifier","Rate Unit of Measure Code",
                                           "Time Period Basis Text","Time Period Basis Code Qualifier","Time Period Basis Code","Frequency Numeric Value","Frequency Units Text",
                                           "Frequency Units Code Qualifier","Frequency Units Code","Variable Frequency Modifier","Interval Numeric Value","Interval Units Text",
                                           "Interval Units Code Qualifier","Interval Units Code","Variable Interval Modifier"},
                            new string[] { "Duration Numeric Value", "Duration Text", "Duration Text Code Qualifier", "Duration Text Code" },
                            new string[] { "Maximum Dose Restriction Numeric Value","Maximum Dose Restriction Units Text","Maximum Dose Restriction Code Qualifier",
                                           "Maximum Dose Restriction Units Code","Maximum Dose Restriction Variable Numeric Value", "Maximum Dose Restriction Variable Units Text",
                                           "Maximum Dose Restriction Variable Units Code Qualifier", "Maximum Dose Restriction Variable Units Code", "Maximum Dose Restriction Variable Duration Modifier"},
                            new string[] { "Indication Precursor Text" ,"Indication Precursor Code Qualifier","Indication Precursor Code","Indication Text","Indication Text Code Qualifier",
                                           "Indication Text Code","Indication Value Text","Indication Value Unit","Indication Value Unit of Measure Text",
                                           "Indication Value Unit of Measure Code Qualifier","Indication Value Unit of Measure Code","Indication Variable Modifier"},
                            new string[] { "Stop Indicator" }
                            ));
            seqDescriptions.Add("OBS",
                valueToInsert(
                            new string[] { "Segment Code[Observation]" },
                            new string[] { "Measurement Dimension", "Measurement Value", "Measurement Unit Qualifier", "Date/Time/Period", "Date/Time/Period Format",
                                           "Measurement Data Qualifier","Source Code List","Measurement Unit Code"},
                            new string[] { "Free Text[ObservationNotes]" }
                            ));
            seqDescriptions.Add("COO",
                valueToInsert(
                            new string[] { "Segment Code[BenefitsCoordination]" },
                            new string[] { "Reference Number[PayerIdentification]", "Reference Qualifier" },
                            new string[] { "Party Name[PayerName]" },
                            new string[] { "Service Type Code" },
                            new string[] { "Reference Number[CardholderID]", "Reference Qualifier" },
                            new string[] { "Party Name[LastName]", "First Name[FirstName]", "Middle Name[MiddleName]", "Suffix[Suffix]", "Prefix[Prefix]" },
                            new string[] { "Reference Number[GroupID]" },
                            new string[] { "Party Name[GroupName]" },
                            new string[] { "Street & No/PO Box[AddressLine1]", "City Name[City]", "State[State]", "Postal Code[ZipCode]",
                                           "Place/Location Qualifier[PlaceLocationQualifier]","Place Location[AddressLine2]"},
                            new string[] { "Date" },
                            new string[] { "Insurance Type Code" },
                            new string[] { "Address" },
                            new string[] { "Reference Number" },
                            new string[] { "Condition/Response Coded" },
                            new string[] { "Patient Identifier" },
                            new string[] { "Communication Number", "Code List Qualifier" }
                            ));
            seqDescriptions.Add("UIT",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "RX_OBS_MEDICATIONS_XML[SEQ_NUM]" },
                            new string[] { "Number of Segments in Message" }
                            ));
            seqDescriptions.Add("UIZ",
                valueToInsert(
                            new string[] { "Segment Code" },
                            new string[] { "Dialogue Reference" },
                            new string[] { "Interchange Control Count" },
                            new string[] { "Duplicator Indicator" }
                            ));
        }

        private string[][] valueToInsert(params string[][] data)
        {
            string[][] array = new string[data.Length][];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = data[i];
            }
            return array;
        }

        private List<string> components = new List<string>();
        private Dictionary<string, List<string>> subComponents = new Dictionary<string, List<string>>();

        private string printableComponent(string segment, string value, int idxComponents)
        {
            if (segment.StartsWith("PVD"))
            {
                string component = segment.Split(DataElementSeparator)[1];
                if (component.StartsWith("P2"))
                {
                    return "PVDP2" + "-" + index(idxComponents) + ": " + value;
                }
                else if (component.StartsWith("PC"))
                {
                    return "PVDPC" + "-" + index(idxComponents) + ": " + value;
                }
            }
            return segment.Substring(0, 3) + "-" + index(idxComponents) + ": " + value;
        }


        private string putPrintableComponet(string segment, string value, int idxComponents)
        {
            string printableComp = printableComponent(segment, value, idxComponents);
            components.Add(printableComp);

            return printableComp;
        }

        private void putPrintableSubComponent(string component, string value, string segment, int idxComponents, int idxSubComponents = 0, int idxRepetitions = 0)
        {
            List<string> subComps = null;
            if (!subComponents.TryGetValue(component, out subComps))
            {
                subComps = new List<string>();
                subComponents.Add(component, subComps);
            }
            subComps.Add(printableSubComponent(segment, idxComponents, idxSubComponents, idxRepetitions) + ": " + value);
            subComponents[component] = subComps;
        }

        private string printableSubComponent(string segment, int idxComponents, int idxSubComponents = 0, int idxRepetitions = 0)
        {
            string subComponent = "";
            string subComponentSequel = "";
            if (segment.StartsWith("PVD"))
            {
                string component = segment.Split(DataElementSeparator)[1];
                if (component.StartsWith("P2"))
                {
                    subComponent = descriptions["PVD-PHARMACY"][idxComponents][idxSubComponents];
                    subComponentSequel = seqDescriptions["PVD-PHARMACY"][idxComponents][idxSubComponents];
                }
                else if (component.StartsWith("PC"))
                {
                    subComponent = descriptions["PVD-PROVIDER"][idxComponents][idxSubComponents];
                    subComponentSequel = seqDescriptions["PVD-PROVIDER"][idxComponents][idxSubComponents];
                }
            }
            else if (descriptions.ContainsKey(segment.Substring(0, 3)))
            {
                subComponent = descriptions[segment.Substring(0, 3)][idxComponents][idxSubComponents];
                subComponentSequel = seqDescriptions[segment.Substring(0, 3)][idxComponents][idxSubComponents];
            }
            if (!subComponent.Equals(subComponentSequel))
            {
                return index(idxSubComponents) + ":" + subComponentSequel;
            }
            else
            {
                return index(idxSubComponents) + ":" + subComponent;
            }
        }

        private string index(int idx)
        {
            if (idx < 9)
            {
                return "0" + idx + "0";
            }
            else if (idx < 99)
            {
                return "0" + idx;
            }

            return Convert.ToString(idx);
        }

        private void processMessage()
        {
            var unaSegment = messageToProcess.Substring(0, 9);
            ComponentDataElementSeparator = unaSegment[3];
            DataElementSeparator = unaSegment[4];
            DecimalNotation = unaSegment[5];
            ReleaseIndicator = unaSegment[6];
            RepetitionSeparator = unaSegment[7];
            SegmentSeparator = unaSegment[8];

            var segments = messageToProcess.Split(SegmentSeparator);
            if (segments.Length > 0)
            {
                for (int idxSegments = 0; idxSegments < segments.Length; idxSegments++)
                {
                    string segment = segments[idxSegments];
                    if (!string.IsNullOrEmpty(segment))
                    {
                        var components = segment.Split(DataElementSeparator);
                        string printableComponenet = "";
                        if (components.Length > 1)
                        {
                            for (int idxComponents = 0; idxComponents < components.Length; idxComponents++)
                            {
                                string component = components[idxComponents];
                                if (!string.IsNullOrEmpty(component))
                                {
                                    printableComponenet = putPrintableComponet(segment, component, idxComponents);
                                    var componentRepetitions = component.Split(RepetitionSeparator);
                                    for (int idxComponentRepetitions = 0; idxComponentRepetitions < componentRepetitions.Length; idxComponentRepetitions++)
                                    {
                                        var componentRepetition = componentRepetitions[idxComponentRepetitions];
                                        if (!string.IsNullOrEmpty(componentRepetition))
                                        {
                                            var subComponents = componentRepetition.Split(ComponentDataElementSeparator);
                                            if (subComponents.Length > 1)
                                            {
                                                for (int idxSubComponents = 0; idxSubComponents < subComponents.Length; idxSubComponents++)
                                                {
                                                    string subComponent = subComponents[idxSubComponents];
                                                    if (!string.IsNullOrEmpty(subComponent))
                                                    {
                                                        var repetitions = subComponent.Split(RepetitionSeparator);
                                                        if (repetitions.Length > 1)
                                                        {
                                                            for (int idxRepetitions = 0; idxRepetitions < repetitions.Length; idxRepetitions++)
                                                            {
                                                                string repetition = repetitions[idxRepetitions];
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (!string.IsNullOrEmpty(printableComponenet))
                                                            {
                                                                putPrintableSubComponent(printableComponenet, subComponent, segment, idxComponents, idxSubComponents);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(printableComponenet))
                                                {
                                                    putPrintableSubComponent(printableComponenet, component, segment, idxComponents);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(printableComponenet))
                            {
                                putPrintableSubComponent(printableComponenet, segment, segment, 0);
                            }
                        }
                    }
                }
            }
        }

        public void printInformation()
        {
            using (StreamWriter writer = new StreamWriter(File.Open(Path.GetTempPath() + "Message.txt",FileMode.Create)))
            {
                components.ForEach(component =>
                {
                    writer.WriteLine(component);
                    Console.WriteLine(component);
                    subComponents[component].ForEach(value => {
                        writer.WriteLine(value);
                        Console.WriteLine(value); 
                    });
                    writer.WriteLine();
                    Console.WriteLine();
                });
                Console.ReadKey();
            }
            Process.Start(@Path.GetTempPath() + "Message.txt");
        }

        public string informationToPrint()
        {
            StringBuilder sb = new StringBuilder();
            var segments = messageToProcess.Split(SegmentSeparator);
            foreach (string segment in segments)
            {
                if (!string.IsNullOrEmpty(segment))
                {
                    string dashes = generateDashes(segment);
                    sb.AppendLine(dashes);
                    sb.AppendLine(segment);
                    sb.AppendLine(dashes);
                    components.ForEach(component =>
                    {
                        if (segment.StartsWith("PVD"))
                        {
                            string componentPVD = segment.Split(DataElementSeparator)[1];
                            if (componentPVD.StartsWith("P2") && component.StartsWith("PVDP2"))
                            {
                                sb.AppendLine(component);
                                subComponents[component].ForEach(value =>
                                {
                                    sb.AppendLine(value);
                                });
                                sb.AppendLine();
                            }
                            else if (componentPVD.StartsWith("PC") && component.StartsWith("PVDPC"))
                            {
                                sb.AppendLine(component);
                                subComponents[component].ForEach(value =>
                                {
                                    sb.AppendLine(value);
                                });
                                sb.AppendLine();
                            }
                        }
                        else
                        {
                            if (segment.StartsWith(component.Substring(0, 3)))
                            {
                                sb.AppendLine(component);
                                subComponents[component].ForEach(value =>
                                {
                                    sb.AppendLine(value);
                                });
                                sb.AppendLine();
                            }
                        }
                    });
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        private string generateDashes(string segment)
        {
            string dash = "";
            foreach (char character in segment)
            {
                dash += "-";
            }
            return dash;
        }
    }
}
