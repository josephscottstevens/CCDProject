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
    type CCDRecord = 
        { ssn : Result<string,string>
        //; dob : Date
        ; streetAddressLine : string
        ; cityLine : string
        ; stateLine : string
        ; postalCodeLine : int
        ; country : string
        //; Contact Preferences (from discussion with patient)?
        ; ``Medical Record Number`` : string
        ; phoneNumbers : string array // currently wrong
        //maritalStatus : string
        //smokingStatus : string
        //alcoholStatus : string
        //encounterNotes : string
        //; homePhone : string option
        //; workPhone : string option
        //; cellPhone : string option
                    // at least 1?

        ; birthTime : int
        ; primaryInsurance : string
        ; secondaryInsurance : string
        //; ``Diagnoses & Active Problem List`` : ?
        //; ``Active Medications`` : ?
        //; ``Past Medical History`` : ?
        //; ``Immunizations/Screenings`` : ?
        //; Allergies : ?
        //; Vitals : ?
        //; ``Encounter Notes`` : ?
        //; ``Faxed Consent`` : ?
        }