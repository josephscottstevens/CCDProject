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
    type CCDRecord = 
        { ``Last Four of Social Security Number`` : Result<string,string>                   // [Enrollment].[SSNNumber]
        ; ``8 digit Date of Birth`` : Result<System.DateTime,string>          // [Enrollment].[DoB]
        ; ``Address`` : string                              // [Enrollment].[HomeAddress]
        ; ``City`` : string                                 // [Enrollment].[HomeCity]
        ; ``State`` : string                                // [Enrollment].[HomeState]
        ; ``Zip Code`` : Result<string, string>                              // [Enrollment].[HomeZip]
        //; Contact Preferences (from discussion with patient)?
        ; ``Medical Record Number`` : string                // [Enrollment].[MedicalRecordNumber]
        //; homePhone : string option                       // [Enrollment].
        //; workPhone : string option                       // [Enrollment].
        //; cellPhone : string option                       // [Enrollment].
        //maritalStatus : string                            // [Enrollment].
        //smokingStatus : string                            // [Enrollment].
        //alcoholStatus : string                            // [Enrollment].
        //encounterNotes : string                           // [Enrollment].
        

        ; ``Primary Insurance`` : string
        ; ``Secondary Insurance`` : string
        //; ``Diagnoses & Active Problem List`` : ?
        //; ``Active Medications`` : ?
        //; ``Past Medical History`` : ?
        //; ``Immunizations/Screenings`` : ?
        //; ``Allergies`` : ?
        //; ``Vitals`` : ?
        //; ``Encounter Notes`` : ?
        //; ``Faxed Consent`` : ?
        ; ``Gender`` : string option
        ; ``Preferred Language``:string option
          //; ``Faxed Consent`` : ?
        }