module Types
    open FSharp.Data.Sql
    let [<Literal>] ConnectionString = "Data Source=localhost;Initial Catalog=NavcareDB_interface2;Integrated Security=True;"
    //let [<Literal>] ConnectionString = "Data Source=10.8.100.4;Initial Catalog=NavcareDBTest;user id=USCareNet;password=BHI_US_2022_2022;"
    type Sql = SqlDataProvider<ConnectionString = ConnectionString, DatabaseVendor = Common.DatabaseProviderTypes.MSSQLSERVER, UseOptionTypes = true>    
    let [<Literal>] sampleProvider = "sampleData.xml"
    type CCD = FSharp.Data.XmlProvider<sampleProvider, SampleIsList=true>
    // Question: Always the home address\city\state\zip?
    //           Or like the other fields like address instead of homeAddress
    
    type Problem =
        { name : string option
        ; code : int64 option
        ; codeSystem : string option
        ; date : Result<System.DateTime,string>
        ; status : string option
        }

    type Medication =
        { name : string option
        ; code : string option
        ; codeSystem : string option
        ; strength : string option
        ; directions : string option
        ; startDate : Result<System.DateTime,string>
        ; status : string option
        ; prescriber : string option
        }

    type MedicalHistory =
        { code : string option
        ; codeSystem : string option
        ; description : string option
        ; date : Result<System.DateTime,string>
        ; performer : string option
        ; status : string option
        }

    //type Encounter = // is it <encompassingEncounter>
    //                 // or this? <title>ENCOUNTER DIAGNOSIS</title>
    //    { code : string 
    //    ; codeSystem : string 
    //    ; date : Result<System.DateTime,string>
    //    ; performer : string
    //    ; status : string
    //    }

    type Immunization = // <title>IMMUNIZATIONS</title>
        { vaccine : string option
        ; code : string option
        ; codeSystem : string option
        ; date : Result<System.DateTime,string>
        ; status : string option
        }

    type Allergy =
        { name : string
        ; reaction : string option
        }

    type Vital = //<title>VITAL SIGNS</title>
        { name : string option
        ; value : string option
        ; effectiveDate : Result<System.DateTime,string>
        }

    type CCDRecord = 
        { ``File Name`` : string
        ; ``Last Four of Social Security Number`` : Result<string,string>    // [Enrollment].[SSNNumber]
        ; ``First Name`` : string option                                     // [Enrollment].[FirstName]
        ; ``Middle Initial`` : string option                                 // [Enrollment].[MiddleName]
        ; ``Last Name`` : string option                                      // [Enrollment].[LastName]
        ; ``Facility Name`` : string option                                  // [Enrollment].[FacilityID]
        ; ``8 digit Date of Birth`` : Result<System.DateTime,string>         // [Enrollment].[DoB]
        ; ``Address`` : string option                                        // [Enrollment].[HomeAddress]
        ; ``City`` : string option                                           // [Enrollment].[HomeCity]
        ; ``State`` : string option                                          // [Enrollment].[HomeState]
        ; ``Zip Code`` : Result<string, string>                              // [Enrollment].[HomeZip]
        ; ``Medical Record Number`` : Result<string,string>                  // [Enrollment].[MedicalRecordNumber]
        ; ``Home Phone`` : string option                                     // [Enrollment].[HomePhone]
        ; ``Work Phone`` : string option                                     // [Enrollment].[WorkPhone]
        ; ``Cell Phone`` : string option                                     // [Enrollment].[CellPhone]
        ; ``Preferred Phone Type Id`` : int option                           // [Enrollment].[PrimaryPhoneNumberTypeId]
        ; ``Marital Status`` : string option                                 // [Enrollment].[MaritalStatus]
        ; ``Smoking Status``: Result<string, string>                         // [Enrollment].?
        //-?-alcoholStatus : string                                          // [Enrollment].?
        ; ``Primary Insurance`` : Result<string,string>                      // [Enrollment].[PrimaryInsurance]
        ; ``Secondary Insurance`` : Result<string,string>                    // [Enrollment].[SecondaryInsurance]
        // CLS table section
        ; ``Diagnoses & Active Problem List`` : Result<Problem array,string> // [dbo].[PatientProblems]
        ; ``Active Medications`` : Result<Medication array,string>           // [dbo].[Medications]
        ; ``Past Medical History`` : Result<MedicalHistory array,string>     // [ptn].[PastMedicalHistories]
        //; ``Encounter Notes`` : Result<Encounter array,string>               // ?
        ; ``Immunizations/Screenings`` : Result<Immunization array,string>   // [cls].[Vaccinations]
        ; ``Allergies`` : Result<Allergy array,string>                       // [cls].[Allergies]
        ; ``Vitals`` : Result<Vital array,string>                            // [cls].[Vitals]
        
        // Additional fields
        ; ``Gender`` : string option                                         // [Enrollment].[Gender]
        ; ``Preferred Language``: string option                              // [Enrollment].[PreferredLanguage]
        ; ``Last Encounter Date``: Result<System.DateTime,string>            // [Enrollment].
        ; ``Race``: string option                                            // [Enrollment].[Ethnicity]
        }
