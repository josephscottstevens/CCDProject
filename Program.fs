open FSharp.Data.Sql
open System
open Types
open HelperFunctions
open Validation

let findCCD (path:string) : Result<CCDRecord, string> =
    try
        let ccd : CCD.ClinicalDocument = getCCd path
        let findTable = findTableWithCCd ccd
        
        let ssn = 
            ccd.RecordTarget.PatientRole.Ids 
            |> Array.filter (fun t -> t.Root = "2.16.840.1.113883.4.1") 
            |> Array.map (fun t -> t.Extension)
            |> Array.head

        let ``Medical Record Number`` = 
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
        Ok  { ssn = isNotNullOrEmpty ssn
                   |> andThen exactlyFour
            ; ``Medical Record Number`` = ``Medical Record Number``
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
        Err ex.Message

[<STAThread>]
[<EntryPoint>]
let main _ =
    
    let ccds : Result<CCDRecord, string> array =
        System.IO.Directory.GetFiles("R:\IT\CCDS")
        |> Array.filter (fun t -> t <> sampleProvider)
        |> Array.map (fun t -> findCCD t)
    
    ccds 
    |> Array.map (fun t -> 
                    match t with 
                    | Ok ccd -> printfn "Success - %s" (string ccd)
                    | Err errStr -> printfn "Error - %s" errStr
        )
    |> ignore
    0