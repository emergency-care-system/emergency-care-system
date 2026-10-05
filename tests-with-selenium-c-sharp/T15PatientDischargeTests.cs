// Selenium WebDriver + NUnit test for
// tests-with-given-when-then-features/15-patient-discharge.feature
// (equivalent to tests-with-selenium-javascript/15-patient-discharge.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.SeleniumTests;

[TestFixture]
[NonParallelizable]
public class T15PatientDischargeTests
{
    private IWebDriver driver = null!;

    [OneTimeSetUp]
    public void SetUpClass()
    {
        driver = DriverFactory.Build();
    }

    [OneTimeTearDown]
    public void TearDownClass()
    {
        driver.Quit();
    }

    [SetUp]
    public void SetUp()
    {
        // Background:
        //   Given the emergency care system is operational
        //   And I am logged in as "Dr. Johnson"
        //   And the discharge module is active
        //   And billing integration is enabled
        //   And bed management system is connected
        VerifySystemIsOperational(driver);
        Login(driver, "Dr. Johnson");
        // The discharge module, billing integration, and the bed management
        // system connection are assumed to be active backend configuration
        // already in place for this environment.

        var patientDischargeNavLink = WaitForTestId(driver, "Nav Patient Discharge");
        patientDischargeNavLink.Click();
        WaitForTestId(driver, "Patient Discharge Panel");
    }

    [Test, Order(1)]
    [Description("Complete standard patient discharge with instructions")]
    public void CompleteStandardPatientDischargeWithInstructions()
    {
        // Given a patient "Jennifer Martinez" is in bed "ED-8"
        // And the patient has completed treatment for "urinary tract infection"
        // And all diagnostic tests and treatments are finished
        // And the patient is medically stable for discharge
        // (assumed pre-seeded test data)
        // When I enter discharge orders and instructions:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Discharge Status"), ("Value", "Home with medications")),
            Row(("Field", "Primary Diagnosis"), ("Value", "Urinary tract infection (N39.0)")),
            Row(("Field", "Medications"), ("Value", "Trimethoprim-Sulfamethoxazole 800mg BID x7d")),
            Row(("Field", "Follow-up Care"), ("Value", "Primary care physician in 3-5 days")),
            Row(("Field", "Activity Level"), ("Value", "Regular activities as tolerated")),
            Row(("Field", "Diet"), ("Value", "Regular diet, increase fluid intake")),
            Row(("Field", "Return Precautions"), ("Value", "Fever >101°F, worsening symptoms, blood in urine")),
        });
        // And I submit the discharge orders
        driver.FindElement(By.CssSelector("[data-testid=\"submit-discharge-form\"]")).Click();

        // Then the system generates comprehensive discharge paperwork:
        WaitForTestId(driver, "Discharge Summary");
        var dischargePaperwork = new List<Dictionary<string, string>>
        {
            Row(("Document", "Discharge Summary"), ("Content", "Treatment summary, diagnosis, medications")),
            Row(("Document", "Medication List"), ("Content", "Prescriptions with dosing instructions")),
            Row(("Document", "Follow-up Instructions"), ("Content", "PCP appointment scheduling information")),
            Row(("Document", "Return Precautions"), ("Content", "When to seek emergency care")),
            Row(("Document", "Patient Education"), ("Content", "UTI prevention and care instructions")),
        };
        foreach (var rowData in dischargePaperwork)
        {
            var document = rowData["Document"];
            var content = rowData["Content"];
            Assert.That(GetText(driver, document), Is.EqualTo(content));
        }

        // And the system updates bed availability:
        var bedAvailabilityUpdates = new List<Dictionary<string, string>>
        {
            Row(("Change", "Previous Status"), ("Details", "Occupied by Jennifer Martinez")),
            Row(("Change", "New Status"), ("Details", "Needs cleaning")),
            Row(("Change", "Availability"), ("Details", "Removed from available bed count")),
            Row(("Change", "Housekeeping Alert"), ("Details", "Cleaning notification sent")),
        };
        foreach (var rowData in bedAvailabilityUpdates)
        {
            var change = rowData["Change"];
            var details = rowData["Details"];
            Assert.That(GetText(driver, change), Is.EqualTo(details));
        }

        // And billing processes are automatically triggered:
        var billingActions = new List<Dictionary<string, string>>
        {
            Row(("Action", "Final Charges"), ("Details", "All services and procedures captured")),
            Row(("Action", "Insurance Billing"), ("Details", "Claims prepared for submission")),
            Row(("Action", "Patient Statement"), ("Details", "Financial responsibility calculated")),
            Row(("Action", "Coding Review"), ("Details", "ICD-10 and CPT codes validated")),
        };
        foreach (var rowData in billingActions)
        {
            var action = rowData["Action"];
            var details = rowData["Details"];
            Assert.That(GetText(driver, action), Is.EqualTo(details));
        }
    }

    [Test, Order(2)]
    [Description("Discharge patient with prescription medications")]
    public void DischargePatientWithPrescriptionMedications()
    {
        // Given a patient "Robert Chen" is ready for discharge
        // And treatment required multiple medications
        // (assumed pre-seeded test data)
        // When I enter discharge orders including prescriptions:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Amoxicillin Dose"), ("Value", "500mg")),
            Row(("Field", "Amoxicillin Frequency"), ("Value", "TID")),
            Row(("Field", "Amoxicillin Duration"), ("Value", "10 days")),
            Row(("Field", "Amoxicillin Special Instructions"), ("Value", "Take with food")),
            Row(("Field", "Ibuprofen Dose"), ("Value", "600mg")),
            Row(("Field", "Ibuprofen Frequency"), ("Value", "Q6H PRN")),
            Row(("Field", "Ibuprofen Duration"), ("Value", "5 days")),
            Row(("Field", "Ibuprofen Special Instructions"), ("Value", "For pain only")),
            Row(("Field", "Omeprazole Dose"), ("Value", "20mg")),
            Row(("Field", "Omeprazole Frequency"), ("Value", "Daily")),
            Row(("Field", "Omeprazole Duration"), ("Value", "14 days")),
            Row(("Field", "Omeprazole Special Instructions"), ("Value", "Take before breakfast")),
        });
        // And I include medication education:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Drug Interactions"), ("Value", "Avoid alcohol with antibiotics")),
            Row(("Field", "Side Effects"), ("Value", "Watch for nausea, diarrhea, allergic reactions")),
            Row(("Field", "Compliance"), ("Value", "Complete full antibiotic course")),
        });
        // And I submit the discharge
        driver.FindElement(By.CssSelector("[data-testid=\"submit-discharge-form\"]")).Click();

        // Then the system generates medication-specific documentation:
        WaitForTestId(driver, "Prescription List");
        var medicationDocumentation = new List<Dictionary<string, string>>
        {
            Row(("Document", "Prescription List"), ("Content", "All medications with complete instructions")),
            Row(("Document", "Drug Information"), ("Content", "Side effects, interactions, precautions")),
            Row(("Document", "Pharmacy List"), ("Content", "Nearby pharmacies with hours")),
            Row(("Document", "Medication Calendar"), ("Content", "Dosing schedule for patient reference")),
        };
        foreach (var rowData in medicationDocumentation)
        {
            var document = rowData["Document"];
            var content = rowData["Content"];
            Assert.That(GetText(driver, document), Is.EqualTo(content));
        }

        // And prescriptions are electronically transmitted to patient's preferred pharmacy
        var prescriptionTransmissionStatus = GetText(driver, "Prescription Transmission Status");
        Assert.That(prescriptionTransmissionStatus, Does.Match(@"transmitted").IgnoreCase);

        // And medication allergy checking is performed one final time
        var allergyCheckStatus = GetText(driver, "Medication Allergy Check Status");
        Assert.That(allergyCheckStatus, Does.Match(@"checked|performed|cleared").IgnoreCase);

        // And patient receives medication counseling checklist
        var counselingChecklist = WaitForTestId(driver, "Medication Counseling Checklist");
        Assert.That(counselingChecklist.Displayed, Is.True);
    }

    [Test, Order(3)]
    [Description("Discharge patient requiring follow-up appointments")]
    public void DischargePatientRequiringFollowUpAppointments()
    {
        // Given a patient "Maria Santos" needs specialized follow-up care
        // And the treatment was for "complex laceration repair"
        // (assumed pre-seeded test data)
        // When I enter discharge orders with follow-up requirements:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Wound Check Timeframe"), ("Value", "2-3 days")),
            Row(("Field", "Wound Check Specialist Required"), ("Value", "Primary care")),
            Row(("Field", "Wound Check Special Instructions"), ("Value", "Remove sutures")),
            Row(("Field", "Specialist Consult Timeframe"), ("Value", "1 week")),
            Row(("Field", "Specialist Consult Specialist Required"), ("Value", "Plastic surgeon")),
            Row(("Field", "Specialist Consult Special Instructions"), ("Value", "Scar management")),
            Row(("Field", "Lab Follow-up Timeframe"), ("Value", "5 days")),
            Row(("Field", "Lab Follow-up Specialist Required"), ("Value", "Primary care")),
            Row(("Field", "Lab Follow-up Special Instructions"), ("Value", "Check CBC")),
        });
        // And I specify wound care instructions:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Dressing Changes"), ("Value", "Change daily, keep dry for 48 hours")),
            Row(("Field", "Cleaning Protocol"), ("Value", "Gentle soap and water after 48 hours")),
            Row(("Field", "Activity Restrictions"), ("Value", "No heavy lifting >10 lbs for 2 weeks")),
            Row(("Field", "Signs of Infection"), ("Value", "Redness, swelling, pus, fever")),
        });

        // Then the system schedules and documents follow-up care:
        WaitForTestId(driver, "Appointment Booking");
        var followUpScheduling = new List<Dictionary<string, string>>
        {
            Row(("Action", "Appointment Booking"), ("Details", "Attempts to schedule with preferred providers")),
            Row(("Action", "Referral Generation"), ("Details", "Electronic referrals to specialists")),
            Row(("Action", "Reminder Setup"), ("Details", "Patient reminders for appointments")),
        };
        foreach (var rowData in followUpScheduling)
        {
            var action = rowData["Action"];
            var details = rowData["Details"];
            Assert.That(GetText(driver, action), Is.EqualTo(details));
        }

        // And comprehensive wound care instructions are provided
        var woundCareInstructions = WaitForTestId(driver, "Wound Care Instructions");
        Assert.That(woundCareInstructions.Displayed, Is.True);

        // And follow-up appointment confirmations are sent to patient
        var appointmentConfirmationStatus = GetText(driver, "Appointment Confirmation Status");
        Assert.That(appointmentConfirmationStatus, Does.Match(@"sent|confirmed").IgnoreCase);

        // And referring physician receives notification of specialist referral
        var specialistReferralNotificationStatus = GetText(driver, "Specialist Referral Notification Status");
        Assert.That(specialistReferralNotificationStatus, Does.Match(@"sent|notified").IgnoreCase);
    }

    [Test, Order(4)]
    [Description("Handle discharge with insurance authorization requirements")]
    public void HandleDischargeWithInsuranceAuthorizationRequirements()
    {
        // Given a patient "David Kim" requires expensive follow-up imaging
        // And the patient's insurance requires prior authorization
        // (assumed pre-seeded test data)
        // When I enter discharge orders including:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Imaging Study"), ("Value", "MRI lumbar spine within 2 weeks")),
            Row(("Field", "Estimated Cost"), ("Value", "$2,400")),
            Row(("Field", "Medical Necessity"), ("Value", "Rule out disc herniation")),
        });
        // And I submit the discharge orders
        driver.FindElement(By.CssSelector("[data-testid=\"submit-discharge-form\"]")).Click();

        // Then the system handles insurance requirements:
        WaitForTestId(driver, "Authorization Check");
        var insuranceRequirements = new List<Dictionary<string, string>>
        {
            Row(("Process", "Authorization Check"), ("Action", "Prior auth required for MRI")),
            Row(("Process", "Documentation Prep"), ("Action", "Clinical justification prepared")),
            Row(("Process", "Patient Notification"), ("Action", "Informed of authorization process")),
            Row(("Process", "Alternative Options"), ("Action", "Suggest urgent care MRI if auth denied")),
        };
        foreach (var rowData in insuranceRequirements)
        {
            var process = rowData["Process"];
            var action = rowData["Action"];
            Assert.That(GetText(driver, process), Is.EqualTo(action));
        }

        // And the patient receives information about:
        var patientInformation = new List<Dictionary<string, string>>
        {
            Row(("Type", "Authorization Process"), ("Content", "Timeline and requirements explained")),
            Row(("Type", "Financial Options"), ("Content", "Self-pay rates and payment plans")),
            Row(("Type", "Alternative Providers"), ("Content", "Facilities that may not require pre-auth")),
        };
        foreach (var rowData in patientInformation)
        {
            var type = rowData["Type"];
            var content = rowData["Content"];
            Assert.That(GetText(driver, type), Is.EqualTo(content));
        }

        // And insurance pre-authorization request is automatically submitted
        var preAuthorizationStatus = GetText(driver, "Insurance Pre-Authorization Status");
        Assert.That(preAuthorizationStatus, Does.Match(@"submitted").IgnoreCase);
    }

    [Test, Order(5)]
    [Description("Discharge pediatric patient with parent/guardian instructions")]
    public void DischargePediatricPatientWithParentGuardianInstructions()
    {
        // Given a pediatric patient "Emma Foster" (age 6) is ready for discharge
        // And the parent "Sarah Foster" is present
        // And treatment was for "febrile seizure"
        // (assumed pre-seeded test data)
        // When I enter pediatric discharge orders:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Weight-based Medications"), ("Value", "Acetaminophen 10mg/kg Q6H PRN fever")),
            Row(("Field", "Parent Education"), ("Value", "Fever management, seizure precautions")),
            Row(("Field", "Activity Restrictions"), ("Value", "No swimming for 24 hours")),
            Row(("Field", "School Return"), ("Value", "May return tomorrow if fever-free")),
        });
        // And I provide seizure-specific education:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Seizure Precautions"), ("Value", "Keep child safe during future episodes")),
            Row(("Field", "When to Call 911"), ("Value", "Seizure >5 minutes, difficulty breathing")),
            Row(("Field", "Temperature Control"), ("Value", "Aggressive fever reduction strategies")),
        });

        // Then the system generates pediatric-specific discharge materials:
        WaitForTestId(driver, "Parent Instructions");
        var pediatricMaterials = new List<Dictionary<string, string>>
        {
            Row(("Document", "Parent Instructions"), ("Content", "Age-appropriate medication dosing")),
            Row(("Document", "Emergency Signs"), ("Content", "When to bring child back to ED")),
            Row(("Document", "School Note"), ("Content", "Medical excuse and return instructions")),
            Row(("Document", "Developmental Info"), ("Content", "Normal vs concerning behaviors post-seizure")),
        };
        foreach (var rowData in pediatricMaterials)
        {
            var document = rowData["Document"];
            var content = rowData["Content"];
            Assert.That(GetText(driver, document), Is.EqualTo(content));
        }

        // And parent acknowledgment is electronically captured
        var parentAcknowledgmentStatus = GetText(driver, "Parent Acknowledgment Status");
        Assert.That(parentAcknowledgmentStatus, Does.Match(@"captured|recorded").IgnoreCase);

        // And pediatric follow-up with primary care pediatrician is scheduled
        var pediatricFollowUpStatus = GetText(driver, "Pediatric Follow-up Status");
        Assert.That(pediatricFollowUpStatus, Does.Match(@"scheduled").IgnoreCase);

        // And school nurse receives medical summary if parent consents
        var schoolNurseNotificationStatus = GetText(driver, "School Nurse Notification Status");
        Assert.That(schoolNurseNotificationStatus, Does.Match(@"sent|notified").IgnoreCase);
    }

    [Test, Order(6)]
    [Description("Handle discharge during shift change")]
    public void HandleDischargeDuringShiftChange()
    {
        // Given a patient "Lisa Brown" is ready for discharge at 18:45
        // And shift change occurs at 19:00
        // And "Dr. Day" (day shift) is discharging the patient
        // And "Dr. Night" (evening shift) is incoming
        // (assumed pre-seeded test data)
        // When "Dr. Day" enters the discharge orders
        // And the discharge process extends past shift change
        // (assumed to have already occurred / triggered by the system)

        // Then the system manages the transition seamlessly:
        WaitForTestId(driver, "Discharge Ownership");
        var transitionManagement = new List<Dictionary<string, string>>
        {
            Row(("Item", "Discharge Ownership"), ("Action", "Dr. Day completes discharge process")),
            Row(("Item", "Documentation"), ("Action", "All discharge notes under Dr. Day's name")),
            Row(("Item", "Follow-up Responsibility"), ("Action", "Any issues route to Dr. Night")),
            Row(("Item", "Billing Attribution"), ("Action", "Dr. Day receives credit for discharge")),
        };
        foreach (var rowData in transitionManagement)
        {
            var item = rowData["Item"];
            var action = rowData["Action"];
            Assert.That(GetText(driver, item), Is.EqualTo(action));
        }

        // And both physicians receive handoff notification:
        var handoffNotifications = new List<Dictionary<string, string>>
        {
            Row(("Physician", "Dr. Day"), ("Content", "Discharge completed for Lisa Brown")),
            Row(("Physician", "Dr. Night"), ("Content", "Lisa Brown discharged - available for questions")),
        };
        foreach (var rowData in handoffNotifications)
        {
            var physician = rowData["Physician"];
            var content = rowData["Content"];
            Assert.That(GetText(driver, $"{physician} Notification"), Is.EqualTo(content));
        }

        // And the bed becomes available for evening shift patient flow
        var bedAvailabilityStatus = GetText(driver, "Bed Availability Status");
        Assert.That(bedAvailabilityStatus, Does.Match(@"available").IgnoreCase);
    }

    [Test, Order(7)]
    [Description("Discharge patient against medical advice (AMA)")]
    public void DischargePatientAgainstMedicalAdviceAMA()
    {
        // Given a patient "Michael Davis" wants to leave against medical advice
        // And the patient has been informed of risks
        // And the patient has decision-making capacity
        // (assumed pre-seeded test data)
        // When I process an AMA discharge:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Risk Explanation"), ("Value", "Documented that risks were explained")),
            Row(("Field", "Patient Understanding"), ("Value", "Patient verbalized understanding of risks")),
            Row(("Field", "Capacity Assessment"), ("Value", "Patient has decision-making capacity")),
            Row(("Field", "Witness Required"), ("Value", "Nurse witness to AMA conversation")),
        });
        // And I enter minimal safe discharge instructions:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Return Immediately"), ("Value", "If symptoms worsen or new symptoms develop")),
            Row(("Field", "Follow-up Care"), ("Value", "Strong recommendation for PCP visit")),
            Row(("Field", "Medication Safety"), ("Value", "Critical medications must be continued")),
        });

        // Then the system generates AMA-specific documentation:
        WaitForTestId(driver, "AMA Form");
        var amaDocumentation = new List<Dictionary<string, string>>
        {
            Row(("Document", "AMA Form"), ("Content", "Legal documentation of patient choice")),
            Row(("Document", "Risk Documentation"), ("Content", "Medical risks of leaving explained")),
            Row(("Document", "Witness Signatures"), ("Content", "Patient, physician, and nurse signatures")),
            Row(("Document", "Limited Liability"), ("Content", "Hospital liability limitations documented")),
        };
        foreach (var rowData in amaDocumentation)
        {
            var document = rowData["Document"];
            var content = rowData["Content"];
            Assert.That(GetText(driver, document), Is.EqualTo(content));
        }

        // And billing processes reflect AMA status
        var billingAmaStatus = GetText(driver, "Billing AMA Status");
        Assert.That(billingAmaStatus, Does.Match(@"AMA").IgnoreCase);

        // And legal risk management is notified of AMA discharge
        var legalRiskNotificationStatus = GetText(driver, "Legal Risk Management Notification Status");
        Assert.That(legalRiskNotificationStatus, Does.Match(@"notified").IgnoreCase);

        // And patient still receives basic safety instructions
        var basicSafetyInstructions = WaitForTestId(driver, "Basic Safety Instructions");
        Assert.That(basicSafetyInstructions.Displayed, Is.True);
    }

    [Test, Order(8)]
    [Description("Batch discharge processing during high volume")]
    public void BatchDischargeProcessingDuringHighVolume()
    {
        // Given multiple patients are ready for simultaneous discharge:
        //   | Patient Name    | Bed    | Diagnosis        | Discharge Type     |
        //   | Patient A       | ED-3   | Minor injury     | Home              |
        //   | Patient B       | ED-7   | Gastroenteritis  | Home with meds    |
        //   | Patient C       | ED-11  | Anxiety          | Home with referral |
        // (assumed pre-seeded test data)
        // When I process multiple discharges efficiently
        // (assumed to have already occurred / triggered by the system)

        // Then the system handles batch processing:
        WaitForTestId(driver, "Template Usage");
        var batchProcessing = new List<Dictionary<string, string>>
        {
            Row(("Feature", "Template Usage"), ("Functionality", "Common discharge templates applied")),
            Row(("Feature", "Automated Documentation"), ("Functionality", "Standard instructions auto-populated")),
            Row(("Feature", "Concurrent Processing"), ("Functionality", "Multiple discharges processed simultaneously")),
        };
        foreach (var rowData in batchProcessing)
        {
            var feature = rowData["Feature"];
            var functionality = rowData["Functionality"];
            Assert.That(GetText(driver, feature), Is.EqualTo(functionality));
        }

        // And all bed updates occur simultaneously:
        var bedUpdates = new List<Dictionary<string, string>>
        {
            Row(("Management", "Status Updates"), ("Action", "All beds marked \"needs cleaning\"")),
            Row(("Management", "Housekeeping Batch"), ("Action", "Single notification for multiple rooms")),
            Row(("Management", "Availability Count"), ("Action", "Bed count updated after all discharges")),
        };
        foreach (var rowData in bedUpdates)
        {
            var management = rowData["Management"];
            var action = rowData["Action"];
            Assert.That(GetText(driver, management), Is.EqualTo(action));
        }

        // And billing processes are optimized for batch handling
        var batchBillingStatus = GetText(driver, "Batch Billing Status");
        Assert.That(batchBillingStatus, Does.Match(@"optimized").IgnoreCase);
    }

    [Test, Order(9)]
    [Description("Track discharge metrics and quality indicators")]
    public void TrackDischargeMetricsAndQualityIndicators()
    {
        // Given patient discharges are being processed
        // (assumed pre-seeded test data)
        // When discharge orders are completed
        // (assumed to have already occurred / triggered by the system)

        // Then the system tracks key performance indicators:
        WaitForTestId(driver, "Discharge Time");
        var performanceIndicators = new List<Dictionary<string, string>>
        {
            Row(("Metric", "Discharge Time"), ("Measurement", "Order entry to patient departure")),
            Row(("Metric", "Readmission Rate"), ("Measurement", "72-hour return rate tracking")),
            Row(("Metric", "Instruction Quality"), ("Measurement", "Patient understanding verification")),
            Row(("Metric", "Follow-up Compliance"), ("Measurement", "Scheduled appointment attendance")),
        };
        foreach (var rowData in performanceIndicators)
        {
            var metric = rowData["Metric"];
            var measurement = rowData["Measurement"];
            Assert.That(GetText(driver, metric), Is.EqualTo(measurement));
        }

        // And generates quality reports:
        var qualityReports = new List<Dictionary<string, string>>
        {
            Row(("Report", "Provider Performance"), ("Content", "Discharge efficiency by physician")),
            Row(("Report", "Patient Satisfaction"), ("Content", "Discharge process satisfaction scores")),
            Row(("Report", "Readmission Analysis"), ("Content", "Patterns in early returns")),
        };
        foreach (var rowData in qualityReports)
        {
            var report = rowData["Report"];
            var content = rowData["Content"];
            Assert.That(GetText(driver, report), Is.EqualTo(content));
        }

        // And identifies improvement opportunities:
        var improvementOpportunities = new List<Dictionary<string, string>>
        {
            Row(("Area", "Process Efficiency"), ("Recommendation", "Streamline documentation workflows")),
            Row(("Area", "Patient Education"), ("Recommendation", "Enhance instruction clarity")),
            Row(("Area", "Follow-up Coordination"), ("Recommendation", "Improve appointment scheduling system")),
        };
        foreach (var rowData in improvementOpportunities)
        {
            var area = rowData["Area"];
            var recommendation = rowData["Recommendation"];
            Assert.That(GetText(driver, area), Is.EqualTo(recommendation));
        }
    }
}
