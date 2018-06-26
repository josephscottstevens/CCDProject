open FSharp.Data.Sql
open System
open Types
open HelperFunctions
open Validation
open System.Xml.Linq
open System.Runtime.InteropServices

let findCCD (path:string) : Result<CCDRecord, string> =
    try
        let ccd : CCD.ClinicalDocument = getCCd path
        let findTable = findTableWithCCd ccd
        
        //todo, validate 10 characters long, then take last 4
        let ``Last Four of Social Security Number`` = 
            ccd.RecordTarget.PatientRole.Ids 
            |> Array.filter (fun t -> t.Root = "2.16.840.1.113883.4.1") 
            |> Array.map (fun t -> t.Extension)
            |> Array.head

        let ``8 digit Date of Birth`` =
            ccd.RecordTarget.PatientRole.Patient.BirthTime.XElement.Value

        let ``Medical Record Number`` = 
            ccd.RecordTarget.PatientRole.Ids 
            |> Array.filter (fun t -> t.Root = "2.16.840.1.113883.19.5.99999.2") 
            |> Array.map (fun t -> t.Extension)
            |> Array.head
    
        let ``Address`` = ccd.RecordTarget.PatientRole.Addr.StreetAddressLine
        let ``City`` = ccd.RecordTarget.PatientRole.Addr.City
        let ``State`` = ccd.RecordTarget.PatientRole.Addr.State
        let zipCodeElement = ccd.RecordTarget.PatientRole.Addr.XElement
            
        let phoneNumber (phoneType : string) = 
            ccd.RecordTarget.PatientRole.Telecoms
            |> Array.where (fun t -> t.Use = phoneType)
            |> Array.map( fun t -> t.Value)
            |> Array.tryHead
            |> Option.map cleanTel

        let homePhone = phoneNumber "HP"
        let workPhone = phoneNumber "WP"
        let cellPhone = phoneNumber "MC"
            
            //|> Array.map (fun t -> cleanTel t.Value)
    
        //todo
        //let emergencyContacts = ccd.RecordTarget.PatientRole.?
    
        let getInsurance (rowIndex:int) : string =
            let primaryInsuranceTable = findTable "INSURANCE PROVIDERS"
            let primaryInsuranceRow = findRowByIndex primaryInsuranceTable rowIndex
            let primaryInsuranceColumnIndex = findColumnIndex "Payer Name" primaryInsuranceTable
            primaryInsuranceRow.Tds.[primaryInsuranceColumnIndex].XElement.Value

        let ``Primary Insurance`` = getInsurance 0
        let ``Secondary Insurance`` = getInsurance 1
    
        //
        //let maritalStatus =

        // todo
        //let smokingStatus =
        // either in social history table or table is blank
        // possible values [NonSmoker, ?]
            
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
        // todo, validate at least 1 phone number
        let gender = ccd.RecordTarget.PatientRole.Patient.AdministrativeGenderCode.DisplayName
        let preferredLanguage = ccd.RecordTarget.PatientRole.Patient.LanguageCommunication.LanguageCode.Code
        Ok  { ``Last Four of Social Security Number`` = 
                ``Last Four of Social Security Number``
                |> isNotNullOrEmpty 
                |> andAlso (exactly 11)
                |> andAlso (takeLast 4)
            ; ``8 digit Date of Birth`` =
                ``8 digit Date of Birth``
                |> dateFromString 
            ; ``Medical Record Number`` = ``Medical Record Number``
            ; ``Home Phone`` = homePhone
            ; ``Work Phone`` = workPhone
            ; ``Cell Phone`` = cellPhone
            ; ``Address`` = ``Address``
            ; ``City`` = ``City``
            ; ``State`` = ``State``
            ; ``Zip Code`` = 
                zipCodeElement
                |> getElementValueByTag "postalCode"
                |> andAlso (between 5 9)
            ; ``Primary Insurance`` = ``Primary Insurance``
            ; ``Secondary Insurance`` = ``Secondary Insurance``
            ; ``Gender`` = Some gender
            ; ``Preferred Language`` = Some preferredLanguage
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