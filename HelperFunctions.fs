module HelperFunctions
    open Types

    let getCCd (path:string) : CCD.ClinicalDocument = 
        CCD.Load(path)

    let findColumnIndex (columnTh:string) (table:CCD.Table) : int =
        table.Thead.Tr.Ths |> Array.findIndex (fun t -> t = columnTh)

    let findTableWithCCd (ccd:CCD.ClinicalDocument) (sectionTitle:string) : CCD.Table =
        ccd.Component.StructuredBody.Components
        |> Array.filter (fun t -> t.Section.Title = sectionTitle)
        |> Array.map (fun t -> t.Section.Text.Table)
        |> Array.head

    let findRowByIndex (table:CCD.Table) (amt:int) : CCD.Tr2 =
        table.Tbody.Trs 
        |> Array.skip amt 
        |> Array.head
    let cleanTel(str:string) =
        str.Replace("tel: ", "")
    
    //todo, date int to SQL UTC date
    let dateFromInt(intDate:int) =
        intDate
    // end todo
 