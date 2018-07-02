module Types
    open FSharp.Data.Sql
    let [<Literal>] ConnectionString = "Data Source=localhost;Initial Catalog=NavcareDB_interface2;Integrated Security=True;"
    type Sql = SqlDataProvider<ConnectionString = ConnectionString, DatabaseVendor = Common.DatabaseProviderTypes.MSSQLSERVER, UseOptionTypes = true>    
    let [<Literal>] sampleProvider = "sampleData.xml"
    type CCD = FSharp.Data.XmlProvider<sampleProvider, SampleIsList=true>
    // Question: Always the home address\city\state\zip?
    //           Or like the other fields like address instead of homeAddress

    type StatusType = Active | Inactive

    type CCDType = ICD9 of string | ICD10 of string

    type Allergy =
        { name : string
        ; reaction : string option
        }

    type Problem = // <title>PROBLEMS</title>
        { code : string 
        ; ccdType : CCDType
        ; date : Result<System.DateTime,string>
        ; performer : string
        ; status : StatusType
        }

    type Medication =
        { name : string
        ; code : string
        ; codeSystem : string
        ; strength : string
        ; directions : string
        ; startDate : Result<System.DateTime,string>
        ; status : string
        ; prescriber : string
        }

    type Vital = //<title>VITAL SIGNS</title>
        { name : string
        ; value : string
        ; effectiveDate : Result<System.DateTime,string>
        }

    type Encounter = // <title>ENCOUNTER DIAGNOSIS</title>
        { code : string 
        ; ccdType : CCDType
        ; date : Result<System.DateTime,string>
        ; performer : string
        ; status : StatusType
        }

    type Immunization = // <title>IMMUNIZATIONS</title>
        { vaccine : string
        ; code : string 
        ; ccdType : CCDType
        ; date : Result<System.DateTime,string>
        ; status : StatusType
        }

    type CCDRecord = 
        { ``File Name`` : string
        ; ``Last Four of Social Security Number`` : Result<string,string>   // [Enrollment].[SSNNumber]
        ; ``First Name`` : string option                                    // [Enrollment].[FirstName]
        ; ``Middle Initial`` : string option                                // [Enrollment].[MiddleName]
        ; ``Last Name`` : string option                                     // [Enrollment].[LastName]
        ; ``Facility Name`` : string option                                 // [Enrollment].[FacilityID]
        ; ``8 digit Date of Birth`` : Result<System.DateTime,string>        // [Enrollment].[DoB]
        ; ``Address`` : string option                                       // [Enrollment].[HomeAddress]
        ; ``City`` : string option                                          // [Enrollment].[HomeCity]
        ; ``State`` : string option                                         // [Enrollment].[HomeState]
        ; ``Zip Code`` : Result<string, string>                             // [Enrollment].[HomeZip]
        ; ``Medical Record Number`` : Result<string,string>                 // [Enrollment].[MedicalRecordNumber]
        ; ``Home Phone`` : string option                                    // [Enrollment].[HomePhone]
        ; ``Work Phone`` : string option                                    // [Enrollment].[WorkPhone]
        ; ``Cell Phone`` : string option                                    // [Enrollment].[CellPhone]
        ; ``Preferred Phone Type Id`` : int option                          // [Enrollment].[PrimaryPhoneNumberTypeId]
        ; ``Marital Status`` : string option                                // [Enrollment].[MaritalStatus]
        ; ``Smoking Status``: Result<string, string>                        // [Enrollment].?
        //-?-alcoholStatus : string                                         // [Enrollment].?
        ; ``Primary Insurance`` : Result<string,string>                     // [Enrollment].[PrimaryInsurance]
        ; ``Secondary Insurance`` : Result<string,string>                   // [Enrollment].[SecondaryInsurance]
        // CLS table section
        ; ``Allergies`` : Result<Allergy array,string>                      // [cls].[Allergies]
        //; ``Diagnoses & Active Problem List`` : ?                 
        //; ``Active Medications`` : ? active medications
        //; ``Vitals`` : ?
        //; ``Encounter Notes`` : ?
        //; ``Immunizations/Screenings`` : ?
        
        
        //; ``Past Medical History`` : ?                        [ptn].[PastMedicalHistories]
        
        
        // Additional fields
        ; ``Gender`` : string option                                        // [Enrollment].[Gender]
        ; ``Preferred Language``: string option                             // [Enrollment].[PreferredLanguage]
        ; ``Last Encounter Date``: Result<System.DateTime,string>           // [Enrollment].
        ; ``Race``: string option                                           // [Enrollment].[Ethnicity]
        }
