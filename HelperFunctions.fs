module HelperFunctions
    open Types

    let getCCd (path:string) : Result<CCD.ClinicalDocument,string> = 
        if System.IO.File.Exists(path) = false then
            Error (sprintf "No file found at: %s" path)
        else if System.IO.FileInfo(path).Length <= 10L then
            Error (sprintf "File empty at: %s" path)
        else 
            Ok (CCD.Load(path))

    let findColumnIndex (columnTh:string) (table:CCD.Table) : Result<int, string> =
        try
            table.Thead.Tr.Ths 
            |> Array.tryFindIndex (fun t -> t = columnTh)
            |> Result.fromOption ""
        with ex ->
            Error ex.Message

    let findTableWithCCd (ccd:CCD.ClinicalDocument) (sectionTitle:string) =
        if Array.isEmpty ccd.Component.StructuredBody.Components then
            Error (sprintf "Could not find table by title: %s" sectionTitle)
        else 
            try
                ccd.Component.StructuredBody.Components
                |> Array.filter (fun t -> t.Section.Title = sectionTitle)
                |> Array.filter (fun t -> t.Section.Text.XElement.Value.ToLower() <> "no records"                       )
                |> Array.map (fun t -> t.Section.Text.Table)
                |> Array.tryHead
                |> Result.fromOption ""
             with ex ->
                Error ex.Message
            
    let findRowByIndex (amt:int) (table:CCD.Table) =
        if Array.isEmpty table.Tbody.Trs then
            Error "Error, no rows in table"
        else
            table.Tbody.Trs 
            |> Array.skip amt 
            |> Array.tryHead
            |> Result.fromOption "Error no value"
    let getCell (rowIdx:int) (colIdx:int) (t:CCD.Table) : Result<string,string> =
        
        Ok t.Tbody.Trs.[rowIdx].Tds.[colIdx].Content.XElement.Value

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