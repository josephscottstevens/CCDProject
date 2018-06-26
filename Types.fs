//let [<Literal>] ConnectionString = "Data Source=localhost;Initial Catalog=NavcareDB_interface2;Integrated Security=True;"
//type Sql = SqlDataProvider<ConnectionString = ConnectionString, DatabaseVendor = Common.DatabaseProviderTypes.MSSQLSERVER, UseOptionTypes = true>
module Types
    type Result<'T> = Success of 'T | Error of string
    let [<Literal>] sampleProvider = """R:\IT\CCDS\sampleData.xml"""
    type CCD = FSharp.Data.XmlProvider<sampleProvider>
    //todo, map use property
    // mc = mobile
    // hp = home phone
    // wp = work phone
    let getCCd (path:string) : CCD.ClinicalDocument = 
        CCD.Load(path)
    
    // START HELPER FUNCTIONS
    let findColumnIndex (columnTh:string) (table:CCD.Table) : int =
        table.Thead.Tr.Ths |> Array.findIndex (fun t -> t = columnTh)

    let findTableWithCCd (ccd:CCD.ClinicalDocument) (sectionTitle:string) : CCD.Table =
        ccd.Component.StructuredBody.Components
        |> Array.filter (fun t -> t.Section.Title = sectionTitle)
        |> Array.map (fun t -> t.Section.Text.Table)
        |> Array.head

    let findRowByIndex (table:CCD.Table) (amt:int) : CCD.Tr2 =
        table.Tbody.Trs 
        |> Array.skip amt 
        |> Array.head
    let cleanTel(str:string) =
        str.Replace("tel: ", "")
    // END HELPER FUNCTIONS
    // preferred?
    // end todo


    //todo, date int to SQL UTC date
    let dateFromInt(intDate:int) =
        intDate
    // end todo
    type SSN = SSN of string
    let tryCreateSSN (str:string) : Result<SSN> =
        if System.String.IsNullOrWhiteSpace str then
            Error "SSN cannot be null or empty"
        else
            if str.Length <> 4 then
                Error "SSN mus best exactly 4 characters"
            else
                Success (SSN str)

    // 21 required fields - 2 optional
    type CCDRecord = 
        { ssn : SSN
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

    type CCDResult =
        | Success of CCDRecord
        | Error of string
    
