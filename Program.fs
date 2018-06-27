open FSharp.Data.Sql
open System
open Types
open HelperFunctions
open Validation

let findCCD (path:string) : Result<CCDRecord, string> =
    try
        match getCCd path with
        | Error str -> Error str
        | Ok ccd ->
            let findTable = findTableWithCCd ccd
        
            //todo, validate 10 characters long, then take last 4
            let ssn = 
                ccd.RecordTarget.PatientRole.Ids 
                |> Array.filter (fun t -> t.Root = "2.16.840.1.113883.4.1") 
                |> Array.map (fun t -> t.Extension)
                |> Array.tryHead
                |> Result.fromOption "SSN not found"

            let dob =
                ccd.RecordTarget.PatientRole.Patient.BirthTime.XElement.Value
        
            let mrn = 
                ccd.RecordTarget.PatientRole.Ids 
                |> Array.filter (fun t -> t.Root = "2.16.840.1.113883.19.5.99999.2") 
                |> Array.map (fun t -> t.Extension)
                |> Array.head
        
            let Address = 
                ccd.RecordTarget.PatientRole.Addr
                |> Option.map(fun t -> t.StreetAddressLine)
            
            let city = 
                ccd.RecordTarget.PatientRole.Addr
                |> Option.map(fun t -> t.City)

            let state = 
                ccd.RecordTarget.PatientRole.Addr
                |> Option.map(fun t -> t.State)

            let zipCodeElement =
                ccd.RecordTarget.PatientRole.Addr
                |> Option.map(fun t -> t.XElement)
            
            let phoneNumber (phoneType : string) = 
                ccd.RecordTarget.PatientRole.Telecoms
                |> Array.where (fun t -> t.Use = phoneType)
                |> Array.map( fun t -> t.Value)
                |> Array.tryHead
                |> Option.map cleanTel

            let homePhone = phoneNumber "HP"
            let workPhone = phoneNumber "WP"
            let cellPhone = phoneNumber "MC"
            let preferredPhoneTypeId = 
                ccd.RecordTarget.PatientRole.Addr
                |> Option.map (fun t -> t.Use)
                |> Option.bind genderStringToGenderTypeId
            
            //todo
            //let emergencyContacts = ccd.RecordTarget.PatientRole.?
                //maybe it is the proxy fields?
    
            let getInsurance (rowIndex:int) : Result<string,string> =
                let tableResult = findTable "INSURANCE PROVIDERS"
                let rowResult : Result<CCD.Tr2,string> =
                    tableResult
                    |> Result.bind (findRowByIndex rowIndex)
                let indexResult : Result<int,string> =
                    tableResult
                    |> Result.bind (findColumnIndex "Payer Name")
                let getVal (row:CCD.Tr2) (index:int) =
                    row.Tds.[index].XElement.Value
                Result.bind2 getVal rowResult indexResult

            let primaryInsurance = getInsurance 0
            let secondaryInsurance = getInsurance 1
        
            //
            //let maritalStatus =

            // todo
            //let smokingStatus =
            // either in social history table or table is blank

            // possible values [NonSmoker, Smoker]
            //let getSmokingStatus (rowIndex:int) : string =
            //    let table = findTable "SOCIAL HISTORY"
            //    let row = findRowByIndex table rowIndex
            //    let columnIndex = findColumnIndex "Payer Name" table
            //    row.Tds.[columnIndex].XElement.Value
            
            // todo
            //let alcoholStatus =

            // todo
            //<title>PROBLEMS</title>
            //<title>MEDICATIONS</title>
            //<title>ENCOUNTER DIAGNOSIS</title>
                //Encounter notes? this thing?
            //<title>PROCEDURES</title>
            //<title>IMMUNIZATIONS</title>
            //<title>ALLERGIES, ADVERSE REACTIONS, ALERTS</title>
                // put mk-ma if no allergies
            //<title>VITAL SIGNS</title>


            // FINAL - three groups
            // Can Process
            // Cannot parse (exception thrown)
        
            // Duplicates
            // todo, validate at least 1 phone number
            let gender = ccd.RecordTarget.PatientRole.Patient.AdministrativeGenderCode.DisplayName
            let preferredLanguage = ccd.RecordTarget.PatientRole.Patient.LanguageCommunication.LanguageCode.Code
            Ok  { ``Last Four of Social Security Number`` = 
                    ssn
                    |> Result.bind isNotNullOrEmpty 
                    |> Result.bind (exactly 11)
                    |> Result.bind (takeLast 4)
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
                    |> Result.fromOption "zip code not found"
                    |> Result.bind (getElementValueByTag "postalCode")
                    |> Result.bind (between 5 9)
                ; ``Primary Insurance`` = primaryInsurance
                ; ``Secondary Insurance`` = secondaryInsurance
                ; ``Last Encounter Date`` = lastEncounterDate
                // Additional fields
                ; ``Gender`` = Some gender
                ; ``Preferred Language`` = Some preferredLanguage
                ; ``Race`` = Some race
                }
    with ex ->
        Error ex.Message

[<STAThread>]
[<EntryPoint>]
let main _ =
    let ccds : Result<CCDRecord, string> array =
        System.IO.Directory.GetFiles("R:\IT\CCDS")
        |> Array.filter (fun t -> t <> sampleProvider)
        |> Array.map (fun t -> findCCD t)

    // todo - filter duplicates
    // todo - check for existing enrollment
    // •	Visit within the last 12 months (see <title>ENCOUNTER DIAGNOSIS</title>)
    // •	Medicare as primary insurance
    // •	At least 2 qualifying diagnoses
    //      This auto qualifies them
    
    ccds 
    |> Array.map (fun t -> 
                    match t with 
                    | Ok ccd -> printfn "Success - %s" (string ccd)
                    | Error errStr -> printfn "Error - %s" errStr
        )
    |> ignore
    0