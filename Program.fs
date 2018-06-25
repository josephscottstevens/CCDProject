﻿open System.Xml
open System.Xml.XPath
open System.Linq
open FSharp.Charting
open FSharp.Data.Sql
open FSharp.Data
open System

//let [<Literal>] ConnectionString = "Data Source=localhost;Initial Catalog=NavcareDB_interface2;Integrated Security=True;"
//type Sql = SqlDataProvider<ConnectionString = ConnectionString, DatabaseVendor = Common.DatabaseProviderTypes.MSSQLSERVER, UseOptionTypes = true>

let [<Literal>] sampleProvider = """R:\IT\CCDS\sampleData.xml"""
type CCD = XmlProvider<sampleProvider>

//todo, map use property
// mc = mobile
// hp = home phone
// wp = work phone

let cleanTel(str:string) =
    str.Replace("tel: ", "")
// preferred?
// end todo


//todo, date int to SQL UTC date
let dateFromInt(intDate:int) =
    intDate
// end todo

type CCDRecord = 
    { ssn : string
    ; mrn : string
    ; streetAddressLine : string
    ; cityLine : string
    ; stateLine : string
    ; postalCodeLine : int
    ; country : string
    ; phoneNumbers : string array
    ; birthTime : int
    ; primaryInsurance : string
    ; secondaryInsurance : string
    }

type CCDResult =
    | Success of CCDRecord
    | Error of string

let getSuccess (ccdResult : CCDResult) : CCDRecord option =
    match ccdResult with 
    | Success ccdRecord -> Some ccdRecord
    | Error _ -> None

let getError (ccdResult : CCDResult) : string option =
    match ccdResult with 
    | Success _ -> None
    | Error str -> Some str

let findCCD (path:string) : CCDResult =
    try
        let ccd = CCD.Load(path)
        // START HELPER FUNCTIONS
        let findColumnIndex (columnTh:string) (table:CCD.Table) : int =
            table.Thead.Tr.Ths |> Array.findIndex (fun t -> t = columnTh)

        let findTable (sectionTitle:string) : CCD.Table =
            ccd.Component.StructuredBody.Components
            |> Array.filter (fun t -> t.Section.Title = sectionTitle)
            |> Array.map (fun t -> t.Section.Text.Table)
            |> Array.head

        let findRowByIndex (table:CCD.Table) (amt:int) : CCD.Tr2 =
            table.Tbody.Trs |> Array.skip amt |> Array.head
        // END HELPER FUNCTIONS

        let ssn = 
            ccd.RecordTarget.PatientRole.Ids 
            |> Array.filter (fun t -> t.Root = "2.16.840.1.113883.4.1") 
            |> Array.map (fun t -> t.Extension)
            |> Array.head

        let mrn = 
            ccd.RecordTarget.PatientRole.Ids 
            |> Array.filter (fun t -> t.Root = "2.16.840.1.113883.19.5.99999.2") 
            |> Array.map (fun t -> t.Extension)
            |> Array.head
    
        let streetAddressLine = ccd.RecordTarget.PatientRole.Addr.StreetAddressLine
        let cityLine = ccd.RecordTarget.PatientRole.Addr.City
        let stateLine = ccd.RecordTarget.PatientRole.Addr.State
        let postalCodeLine = ccd.RecordTarget.PatientRole.Addr.PostalCode
        let country = ccd.RecordTarget.PatientRole.Addr.Country
        let phoneNumbers = 
            ccd.RecordTarget.PatientRole.Telecoms
            |> Array.map (fun t -> cleanTel t.Value)
    
        let birthTime = 
            ccd.RecordTarget.PatientRole.Patient.BirthTime.Value 
            |> dateFromInt
        //todo
        //let emergencyContacts = ccd.RecordTarget.PatientRole.?
    
        let getInsurance (rowIndex:int) : string =
            let primaryInsuranceTable = findTable "INSURANCE PROVIDERS"
            let primaryInsuranceRow = findRowByIndex primaryInsuranceTable rowIndex
            let primaryInsuranceColumnIndex = findColumnIndex "Payer Name" primaryInsuranceTable
            primaryInsuranceRow.Tds.[primaryInsuranceColumnIndex].XElement.Value

        let primaryInsurance = getInsurance 0
        let secondaryInsurance = getInsurance 1
    
        //let xyz = primaryInsurance
        //let secondaryInsurance = findTableRow 1
        //let zz = getColumnIndex "Payer Order" primaryInsurance
        //let yy = zz

        //
        //let maritalStatus =

        // todo
        //let smokingStatus =

        // todo
        //let alcoholStatus =

        // todo
        let encounters =
            //ENCOUNTER DIAGNOSIS
            []
    
        // todo
        let problems =
        
            []
        //<title>PROBLEMS</title>
        //<title>MEDICATIONS</title>
        //<title>ENCOUNTER DIAGNOSIS</title>
        //<title>PROCEDURES</title>
        //<title>IMMUNIZATIONS</title>
        //<title>ALLERGIES, ADVERSE REACTIONS, ALERTS</title>
            // put mk-ma if no allergies
        //<title>VITAL SIGNS</title>

        //Encounter notes?

        // FINAL - three groups
        // Can Process
        // Cannot parse (exception thrown)

        // Duplicates
        Success { ssn = ssn
             ; mrn = mrn
             ; streetAddressLine = streetAddressLine
             ; cityLine = cityLine
             ; stateLine = stateLine
             ; postalCodeLine = postalCodeLine
             ; country = country
             ; phoneNumbers = phoneNumbers
             ; birthTime = birthTime
             ; primaryInsurance = primaryInsurance
             ; secondaryInsurance = secondaryInsurance
             }
    with ex ->
        Error ex.Message

[<STAThread>]
[<EntryPoint>]
let main _ =
    
    let ccds =
        System.IO.Directory.GetFiles("R:\IT\CCDS")
        |> Array.filter (fun t -> t <> sampleProvider)
        |> Array.map (fun t -> findCCD t)
    
    let readyToProcessCCDs = ccds |> Array.choose getSuccess
    let failedCCDs = ccds |> Array.choose getError
    readyToProcessCCDs
    failedCCDs
    0