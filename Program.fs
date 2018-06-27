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
        let ssn = 
            ccd.RecordTarget.PatientRole.Ids 
            |> Array.filter (fun t -> t.Root = "2.16.840.1.113883.4.1") 
            |> Array.map (fun t -> t.Extension)
            |> Array.head

        let dob =
            ccd.RecordTarget.PatientRole.Patient.BirthTime.XElement.Value

        let mrn = 
            ccd.RecordTarget.PatientRole.Ids 
            |> Array.filter (fun t -> t.Root = "2.16.840.1.113883.19.5.99999.2") 
            |> Array.map (fun t -> t.Extension)
            |> Array.head
    
        let Address = ccd.RecordTarget.PatientRole.Addr.StreetAddressLine
        let city = ccd.RecordTarget.PatientRole.Addr.City
        let state = ccd.RecordTarget.PatientRole.Addr.State
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
        let preferredPhoneTypeId = genderStringToGenderTypeId ccd.RecordTarget.PatientRole.Addr.Use 
            
        //todo
        //let emergencyContacts = ccd.RecordTarget.PatientRole.?
    
        let getInsurance (rowIndex:int) : Result<string,string> =
            let tableResult = findTable "INSURANCE PROVIDERS"
            let rowResult : Result<CCD.Tr2,string> =
                tableResult
                |> andThen (findRowByIndex rowIndex)
            let indexResult : Result<int,string> =
                tableResult
                |> andThen (findColumnIndex "Payer Name")
            match (rowResult, indexResult) with
            | (Ok row, Ok index) ->
                Ok row.Tds.[index].XElement.Value
            | (Error t, _) ->
                Error t
            | (_, Error t) ->
                Error t

        let primaryInsurance = getInsurance 0
        let secondaryInsurance = getInsurance 1
    
        //
        //let maritalStatus =

        // todo
        //let smokingStatus =
        // either in social history table or table is blank
        // possible values [NonSmoker, ?]
        //let getSmokingStatus (rowIndex:int) : string =
        //    let table = findTable "SOCIAL HISTORY"
        //    let row = findRowByIndex table rowIndex
        //    let columnIndex = findColumnIndex "Payer Name" table
        //    row.Tds.[columnIndex].XElement.Value
            
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
                ssn
                |> isNotNullOrEmpty 
                |> andThen (exactly 11)
                |> andThen (takeLast 4)
            ; ``8 digit Date of Birth`` =
                dob
                |> dateFromString 
            ; ``Medical Record Number`` = mrn
            ; ``Home Phone`` = homePhone
            ; ``Work Phone`` = workPhone
            ; ``Cell Phone`` = cellPhone
            ; ``Preferred Phone Type Id`` = preferredPhoneTypeId
            ; ``Address`` = ``Address``
            ; ``City`` = city
            ; ``State`` = state
            ; ``Zip Code`` = 
                zipCodeElement
                |> getElementValueByTag "postalCode"
                |> andThen (between 5 9)
            ; ``Primary Insurance`` = primaryInsurance
            ; ``Secondary Insurance`` = secondaryInsurance

            // Additional fields
            ; ``Gender`` = Some gender
            ; ``Preferred Language`` = Some preferredLanguage
            }
    with ex ->
        Error ex.Message

[<STAThread>]
[<EntryPoint>]
let main _ =
    
    let xyz = findCCD """R:\IT\CCDS\JAMES_COOLEY_06212018104058_COMPLETE_CCDA.xml"""
    //let xyz = findCCD """R:\IT\CCDS\MARGARET_(PEGGY)_P_WILSON_06252018111540_COMPLETE_CCDA.xml"""
    let z = xyz

    //let ccds : Result<CCDRecord, string> array =
    //    System.IO.Directory.GetFiles("R:\IT\CCDS")
    //    |> Array.filter (fun t -> t <> sampleProvider)
    //    |> Array.map (fun t -> findCCD t)

    // todo - filter duplicates
    // todo - check for existing enrollment
    // •	Visit within the last 12 months (see <title>ENCOUNTER DIAGNOSIS</title>)
    // •	Medicare as primary insurance
    // •	At least 2 qualifying diagnoses
    //      This auto qualifies them
    
    //ccds 
    //|> Array.map (fun t -> 
    //                match t with 
    //                | Ok ccd -> printfn "Success - %s" (string ccd)
    //                | Error errStr -> printfn "Error - %s" errStr
    //    )
    //|> ignore
    0