open FSharp.Data.Sql
open System
open Types
open HelperFunctions
open Validation
open Log

let findCCD (path:string) : Result<CCDRecord, string> =
    try
        match getCCd path with
        | Error str -> Error str
        | Ok ccd ->
            let findTable = findTableWithCCd ccd
        
            let ssn = 
                ccd.RecordTarget.PatientRole.Ids 
                |> Array.filter (fun t -> t.Root = "2.16.840.1.113883.4.1") 
                |> Array.map (fun t -> t.Extension)
                |> Array.tryHead
                |> Result.fromOption "SSN not found"

            let dob =
                ccd.RecordTarget.PatientRole.Patient.BirthTime.Value
        
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
        
            let maritalStatus =
                ccd.RecordTarget.PatientRole.Patient.MaritalStatusCode
                |> Option.map (fun t -> t.DisplayName)

            let smokingStatus =
                findTable "SOCIAL HISTORY"
                |> Result.bind (getCell 0 1)
                
            // todo
            //let alcoholStatus = ?

            let gender = ccd.RecordTarget.PatientRole.Patient.AdministrativeGenderCode.DisplayName
            let preferredLanguage = ccd.RecordTarget.PatientRole.Patient.LanguageCommunication.LanguageCode.Code
            let lastEncounterDate = 
                ccd.DocumentationOf.ServiceEvent.EffectiveTime.High
                |> Option.map (fun t -> t.Value)
            let race = 
                ccd.RecordTarget.PatientRole.Patient.EthnicGroupCode
                |> Option.map (fun t -> t.DisplayName)
            let firstName = ccd.RecordTarget.PatientRole.Patient.Name.Given
            let lastName = ccd.RecordTarget.PatientRole.Patient.Name.Family
            let middleInitial = ccd.RecordTarget.PatientRole.Patient.Name.Use
            let faciltyName = ccd.Custodian.AssignedCustodian.RepresentedCustodianOrganization.Name
            
            // ##### Start CCD Table Section ##### \\

            let allergies : Result<Allergy array,string> =
                // todo: put mk-ma if no allergies?
                let tableResult = findTable "ALLERGIES, ADVERSE REACTIONS, ALERTS"
                let row (row:CCD.Tr2) : Allergy =
                    { name = row.Tds.[0].Content.XElement.Value
                    ; reaction = row.Tds.[3].Content.String
                    }

                tableResult
                |> Result.map (fun t -> t.Tbody.Trs)
                |> Result.map (fun t -> Array.map row t )

            let problems : Result<Problem array,string> =
                let tableResult = findTable "PROBLEMS"
                let row (row:CCD.Tr2) : Problem =
                    { name = row.Tds.[0].Content.XElement.Value
                    ; code = row.Tds.[1].Content.Number
                    ; codeSystem = row.Tds.[2].Content.XElement.Value
                    ; date = row.Tds.[3].Content.String |> Option.defaultValue "" |> (dateFromString "Problem Effective Date")
                    ; status = row.Tds.[4].Content.XElement.Value
                    }

                tableResult
                |> Result.map (fun t -> t.Tbody.Trs)
                |> Result.map (fun t -> Array.map row t )


            // todo
            //<title>MEDICATIONS</title>
            //<title>ENCOUNTER DIAGNOSIS</title>
                //Encounter notes? this thing?
            //<title>PROCEDURES</title>
            //<title>IMMUNIZATIONS</title>
                
            //<title>VITAL SIGNS</title>

            // ##### End   CCD Table Section ##### \\

            Ok  { ``File Name`` = path
                ; ``Last Four of Social Security Number`` = 
                    ssn
                    |> Result.bind (isNotNullOrEmpty "Social Security Number")
                    |> Result.bind (exactly 11 "Social Security Number")
                    |> Result.bind (takeLast 4 "Social Security Number")
                ; ``8 digit Date of Birth`` =
                    dob
                    |> dateFromString "Date of Birth"
                ; ``Medical Record Number`` = 
                    mrn
                    |> (isNotNullOrEmpty "Medical Record Number")
                ; ``Home Phone`` = homePhone
                ; ``Work Phone`` = workPhone
                ; ``Cell Phone`` = cellPhone
                ; ``Preferred Phone Type Id`` = preferredPhoneTypeId
                ; ``Marital Status`` = maritalStatus
                ; ``Smoking Status`` = smokingStatus
                ; ``Address`` = ``Address``
                ; ``City`` = city
                ; ``State`` = state
                ; ``Zip Code`` = 
                    zipCodeElement
                    |> Result.fromOption "zip code not found"
                    |> Result.bind (getElementValueByTag "postalCode")
                    |> Result.bind (between 5 9 "postalCode")
                ; ``Primary Insurance`` = primaryInsurance
                ; ``Secondary Insurance`` = secondaryInsurance
                ; ``Last Encounter Date`` = 
                    lastEncounterDate
                    |> Result.fromOption "No encounter Date found"
                    |> Result.bind (dateFromString "Last Encounter Date")
                ; ``First Name`` = Some firstName
                ; ``Last Name`` = Some lastName
                ; ``Middle Initial`` = Some middleInitial
                ; ``Facility Name`` = Some faciltyName
                // CLS table section
                ; ``Allergies`` = allergies
                ; ``Diagnoses & Active Problem List`` = problems

                // Additional fields
                ; ``Gender`` = Some gender
                ; ``Preferred Language`` = Some preferredLanguage
                ; ``Race`` = race
                }
    with ex ->
        Error ex.Message

let ``Validate Last Encounter within the last 12 months`` (ccd:CCDRecord) =
    match ccd.``Last Encounter Date`` with 
    | Error t -> Error t
    | Ok dt -> 
        if dt.AddYears(1) < DateTime.UtcNow then
            Error (sprintf "Last Encounter Date not within 1 year of %s" (DateTime.UtcNow.ToShortDateString()))
        else
            Ok ccd
    
let ``Validate First Name and Last Name are present`` (ccd:CCDRecord) =
    if ccd.``First Name``.IsNone then
        Error "First Name required"
    else if ccd.``Last Name``.IsNone then
        Error "Last Name required"
    else
        Ok ccd  

let ``Validate Date of Birth, or MRN number or Patient Id`` (ccd:CCDRecord) =
    match (ccd.``8 digit Date of Birth``, ccd.``Medical Record Number``) with
    | (Ok _, Ok _) -> Ok ccd
    | (Ok _, Error _) -> Ok ccd
    | (Error _, Ok _) -> Ok ccd
    | (Error err, Error _) -> Error err

let ``Validate at least one phone number`` (ccd:CCDRecord) =
    match (ccd.``Home Phone``, ccd.``Work Phone``, ccd.``Cell Phone``) with
    | (Some _, _, _) -> Ok ccd
    | (_, Some _, _) -> Ok ccd
    | (_ , _, Some _) -> Ok ccd
    | _ -> Error "At least one phone number is required"

let ``Validate Medicare is Primary or Secondary Insurance`` (ccd:CCDRecord) =
    let cleanInsuranceStr (resultStr : Result<string, string>) : string =
        resultStr
        |> Result.toOption
        |> Option.defaultValue ""
        |> (fun t -> t.ToUpper())
    [ ccd.``Primary Insurance`` |> cleanInsuranceStr
    ; ccd.``Secondary Insurance`` |> cleanInsuranceStr
    ]
    |> List.exists(fun t -> t.Contains("MEDICARE"))
    |> (fun t -> if t then Ok ccd else Error "MEDICARE is the required insurance type")
    

let validateCCD (record:CCDRecord) : Result<CCDRecord, string> =
    record
    |> ``Validate Last Encounter within the last 12 months`` 
    |> Result.bind ``Validate First Name and Last Name are present``
    |> Result.bind ``Validate Date of Birth, or MRN number or Patient Id``
    |> Result.bind ``Validate Medicare is Primary or Secondary Insurance``
    |> Result.bind ``Validate at least one phone number``

[<STAThread>]
[<EntryPoint>]
let main _ =
    let files : string [] = 
        //[| """R:\IT\CCDS\DELORAS_G_EASON_06212018120821_COMPLETE_CCDA.xml""" |]
        System.IO.Directory.GetFiles("R:\IT\CCDS")

    let ccds : Result<CCDRecord, string> array =
        files
        |> Array.map findCCD
        |> Array.map (Result.bind validateCCD)
    let ctx = Sql.GetDataContext()

    let facilityName = 
        ccds 
        |> Array.choose Result.toOption
        |> Array.map (fun t -> t.``Facility Name``.Value)
        |> Array.head

    let facilityId = 
        ctx.Hco.Hcos 
        |> Seq.where(fun t -> t.FacilityName.Contains(facilityName))
        |> Seq.map(fun t -> t.Id) 
        |> Seq.head
    

    let insertEnrollmentIDs (ccd:CCDRecord) : Result<int,string> * string =
        try
            //let mrn = ccd.``Medical Record Number`` |> Result.toOption
            if ccd.``Facility Name``.Value <> facilityName then
                failwith "invalid facility name"
            let firstName = ccd.``First Name``.Value
            let lastName = ccd.``Last Name``.Value
            let existingRecord =
                query {
                    for e in ctx.Ptn.Enrollment do
                    where (e.FirstName.Contains(firstName))
                    where (e.LastName.Contains(lastName))
                    where (e.FacilityId = facilityId)
                    // where (e.MedicalRecordNumber = mrn) //Can't bind this
                        // No mapping exists from object type Microsoft.FSharp.Core.FSharpOption`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] to a known managed provider native type.
                    select e.Id
                }
                |> Seq.toList
                |> List.tryHead

            match existingRecord with
            | Some id ->
                //Error (sprintf "Record already exists: %s" (string ccd))
                ctx.Ptn.Enrollment 
                |> Seq.find(fun t -> t.Id = id)
                |> (fun t -> 
                    t.Delete()
                    ctx.SubmitUpdates()
                )
                (Ok id), (sprintf "Record exists already: %d" id)
            | None ->
                let row = ctx.Ptn.Enrollment.Create()
                row.SsnNumber <- ccd.``Last Four of Social Security Number`` |> Result.toOption
                row.FirstName <- ccd.``First Name``.Value
                row.MiddleName <- ccd.``Middle Initial``
                row.LastName <- ccd.``Last Name``.Value
                row.FacilityId <- facilityId
                row.MiddleName <- ccd.``Middle Initial``
                row.DoB <- ccd.``8 digit Date of Birth`` |> Result.toOption
                row.HomeAddress <- ccd.Address
                row.HomeCity <- ccd.City
                row.State <- ccd.State
                row.HomeZip <- ccd.``Zip Code`` |> Result.toOption
                row.MedicalRecordNumber <- ccd.``Medical Record Number`` |> Result.toOption
                row.HomePhone <- ccd.``Home Phone``
                row.WorkPhone <- ccd.``Work Phone``
                row.CellPhone <- ccd.``Cell Phone``
                row.PrimaryPhoneNumberTypeId <- ccd.``Preferred Phone Type Id``
                row.MaritalStatus <- ccd.``Marital Status``
                row.PrimaryInsurance <- ccd.``Primary Insurance`` |> Result.toOption
                row.SecondaryInsurance <- ccd.``Secondary Insurance`` |> Result.toOption
                // Additional fields
                row.Gender <- ccd.Gender
                row.PreferredLanguage <- ccd.``Preferred Language``
                row.OptIn <- false //TODO, this can be true if 'At least 2 qualifying diagnoses'
                row.AdmitDate <- Some System.DateTime.UtcNow
                //ctx.SubmitUpdates()
                
                ((Ok row.Id), writeRecordRow ccd)
        with ex ->
            ((Error ex.Message), (sprintf "error: %s" ex.Message))

    let insertedEnrollmentIDs =
        ccds 
        |> Array.choose Result.toOption
        //|> Array.distinct //TODO, this is a thing yah?
        |> Array.map insertEnrollmentIDs
    
    let erroredEnrollments =
       ccds
       |> Array.choose(fun t -> match t with | Ok _ -> None | Error err -> Some err)
       
    erroredEnrollments |> Array.map(fun t -> printf "Error: %s\n" t) |> ignore
    printf "\n\n"
    printf "%d total files available\n" (Array.length files)
    printf "%d files processed\n" (Array.length insertedEnrollmentIDs)
    printf "%d files errored\n" (Array.length erroredEnrollments)

    let x = 
        insertedEnrollmentIDs 
        |> Array.map snd
        |> Array.reduce(fun t y -> t + "\n" + y)
    
    (writeRecordHeader + x) |> System.Windows.Forms.Clipboard.SetText

    printf "\nPress any key to continue"
    //System.Console.ReadLine() |> ignore

    //insertedEnrollmentIDs
    //|> Array.choose Result.toOption
    //|> Array.map (fun (enrollmentId:int) -> 
    //                let row = 
    //                    ctx.Ptn.Enrollment
    //                    |> Seq.find (fun t -> t.Id = enrollmentId) 
    //                row.Delete()
    //                ctx.SubmitUpdates()
    //                ()
    //)|> ignore
    
    
    0