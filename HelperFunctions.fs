module HelperFunctions
    open Types
    open Validation

    let getCCd (path:string) : CCD.ClinicalDocument = 
        CCD.Load(path)

    let findColumnIndex (columnTh:string) (table:CCD.Table) : int =
        table.Thead.Tr.Ths |> Array.findIndex (fun t -> t = columnTh)

    let findTableWithCCd (ccd:CCD.ClinicalDocument) (sectionTitle:string) : CCD.Table =
        ccd.Component.StructuredBody.Components
        |> Array.filter (fun t -> t.Section.Title = sectionTitle)
        |> Array.map (fun t -> t.Section.Text.Table)
        |> Array.head

    // todo, change to cell instead of index
    let findRowByIndex (table:CCD.Table) (amt:int) : CCD.Tr2 =
        table.Tbody.Trs 
        |> Array.skip amt 
        |> Array.head

    let cleanTel(str:string) =
        str.Replace("tel: ", "")

    let getElementValueByTag (name:string) (element:System.Xml.Linq.XElement) : Result<string,string> =
        let maybeElement = 
            element.Elements()
            |> Seq.tryFind(fun t -> t.Name.LocalName = name)
        match maybeElement with
        | Some element -> Ok element.Value
        | None -> Err "No Element found"
    
    let genderStringToGenderTypeId (str:string) : int option =
        match str with
        | "MC" -> Some 0
        | "HP" -> Some 1
        | "WP" -> Some 2
        | _ -> None