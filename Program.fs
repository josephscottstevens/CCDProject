open System.Xml
open System.Xml.XPath
open System.Linq
open FSharp.Charting
open FSharp.Data.Sql
open FSharp.Data
open System

//let [<Literal>] ConnectionStringProd = "Data Source=10.8.100.4;Initial Catalog=NavcareDBProd;user id=USCareNet;password=BHI_US_2022_2022; "
//type SqlProd = SqlDataProvider<ConnectionString = ConnectionStringProd, DatabaseVendor = Common.DatabaseProviderTypes.MSSQLSERVER, UseOptionTypes = true>
//let ctx = SqlProd.GetDataContext()

type CCD = XmlProvider<"""R:\IT\CCDS\sampleData.xml""">

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

[<STAThread>]
[<EntryPoint>]
let main _ =
    let ccd = CCD.Load("""R:\IT\CCDS\WILLIE_MAE_JONES_06222018093118_COMPLETE_CCDA.xml""")
        
    let getInsurance (amt:int) =
        ccd.Component.StructuredBody.Components
        |> Array.filter (fun t -> t.Section.Title = "INSURANCE PROVIDERS")
        |> Array.map (fun t -> t.Section.Text.Table.Tbody.Trs |> Array.skip amt |> Array.head)
        |> Array.head
        //|> (fun t -> t.Tds |> Array.map(fun tt -> tt)

    let ssn = ccd.RecordTarget.PatientRole.Ids 
              |> Array.filter (fun t -> t.Root = "2.16.840.1.113883.4.1") 
              |> Array.map (fun t -> t.Extension)
              |> Array.head

    let mrn = ccd.RecordTarget.PatientRole.Ids 
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
    let primaryInsurance = getInsurance 0
    let secondaryInsurance = getInsurance 1

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

    0