module HelperFunctions
    open Types
    open Validation

    let getCCd (path:string) : CCD.ClinicalDocument = 
        CCD.Load(path)

    let findColumnIndex (columnTh:string) (table:CCD.Table) : Result<int, string> =
        table.Thead.Tr.Ths 
        |> Array.tryFindIndex (fun t -> t = columnTh)
        |> fromOption ""

    let findTableWithCCd (ccd:CCD.ClinicalDocument) (sectionTitle:string) =
        if Array.isEmpty ccd.Component.StructuredBody.Components then
            Error (sprintf "Could not find table by title: %s" sectionTitle)
        else 
            ccd.Component.StructuredBody.Components
            |> Array.filter (fun t -> t.Section.Title = sectionTitle)
            |> Array.map (fun t -> t.Section.Text.Table)
            |> Array.tryHead
            |> fromOption ""
            
    let findRowByIndex (amt:int) (table:CCD.Table) =
        if Array.isEmpty table.Tbody.Trs then
            Error "Error, no rows in table"
        else
            table.Tbody.Trs 
            |> Array.skip amt 
            |> Array.tryHead
            |> fromOption "Error no value"

    let cleanTel(str:string) =
        str.Replace("tel: ", "")

    let getElementValueByTag (name:string) (element:System.Xml.Linq.XElement) : Result<string,string> =
        let maybeElement = 
            element.Elements()
            |> Seq.tryFind(fun t -> t.Name.LocalName = name)
        match maybeElement with
        | Some element -> Ok element.Value
        | None -> Error "No Element found"
    
    let genderStringToGenderTypeId (str:string) : int option =
        match str with
        | "MC" -> Some 0
        | "HP" -> Some 1
        | "WP" -> Some 2
        | _ -> None