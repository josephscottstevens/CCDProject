//let [<Literal>] ConnectionString = "Data Source=localhost;Initial Catalog=NavcareDB_interface2;Integrated Security=True;"
//type Sql = SqlDataProvider<ConnectionString = ConnectionString, DatabaseVendor = Common.DatabaseProviderTypes.MSSQLSERVER, UseOptionTypes = true>
module Types
    open Validation
    
    let [<Literal>] sampleProvider = """R:\IT\CCDS\sampleData.xml"""
    type CCD = FSharp.Data.XmlProvider<sampleProvider>
    //todo, map use property
    // mc = mobile
    // hp = home phone
    // wp = work phone
    // preferred?
    // end todo

    // 21 required fields - 2 optional
    // Question: Always the home address\city\state\zip?

    // required: DOB OR MRN OR PATIENT_ID
    // required: firstname
    // required: lastname
    // required: facilityid
    type CCDRecord = 
        { ``Last Four of Social Security Number`` : Result<string,string>   // [Enrollment].[SSNNumber]
        ; ``8 digit Date of Birth`` : Result<System.DateTime,string>        // [Enrollment].[DoB]
        ; ``Address`` : string                                              // [Enrollment].[HomeAddress]
        ; ``City`` : string                                                 // [Enrollment].[HomeCity]
        ; ``State`` : string                                                // [Enrollment].[HomeState]
        ; ``Zip Code`` : Result<string, string>                             // [Enrollment].[HomeZip]
        ; ``Medical Record Number`` : string                                // [Enrollment].[MedicalRecordNumber]
        ; ``Home Phone`` : string option                                    // [Enrollment].[HomePhone]
        ; ``Work Phone`` : string option                                    // [Enrollment].[WorkPhone]
        ; ``Cell Phone`` : string option                                    // [Enrollment].[CellPhone]
        ; ``Preferred Phone Type Id`` : int option                          // [Enrollment].[PrimaryPhoneNumberTypeId]
        //-?-maritalStatus : string                                         // [Enrollment].[MaritalStatus]
        //smokingStatus : string                                            // [Enrollment].
        //-?-alcoholStatus : string                                         // [Enrollment].

        ; ``Primary Insurance`` : Result<string,string>                     // [Enrollment].[PrimaryInsurance]
        ; ``Secondary Insurance`` : Result<string,string>                   // [Enrollment].[SecondaryInsurance]
        //; ``Diagnoses & Active Problem List`` : ?
        //; ``Active Medications`` : ? active medications
        //; ``Past Medical History`` : ?
        //; ``Immunizations/Screenings`` : ?
        //; ``Allergies`` : ?
        //; ``Vitals`` : ?
        //; ``Encounter Notes`` : ?
        
        // Additional fields
        ; ``Gender`` : string option                                        // [Enrollment].[Gender]
        ; ``Preferred Language``: string option                             // [Enrollment].[PreferredLanguage]
        }