module Log
    open System
    open Microsoft.FSharp.Reflection
    open Types

    let onlyLettersOrSpaces (c:char) : bool =
        Char.IsLetterOrDigit c = true || c = ' '

    let filterOnlyLettersOrSpaces (str:string) : string =
        String.filter onlyLettersOrSpaces str

    let writeStrStr (t:Result<string,string>) : string =
        match t with
        | Ok str -> str
        | Error str -> sprintf "=HYPERLINK(\"%s\")" (filterOnlyLettersOrSpaces str)
    
    let writeDt (t:Result<DateTime,string>) : string =
        match t with
        | Ok (dt:DateTime) -> dt.ToString()
        | Error str  -> sprintf "=HYPERLINK(\"%s\")" (filterOnlyLettersOrSpaces str)
    
    let writeRecordHeader : string =
        Reflection.FSharpType.GetRecordFields(typeof<CCDRecord>) 
        |> Seq.map (fun field -> field.Name)
        |> Seq.reduce(fun t y -> t + "\t" + y)

    let writeRecordRow (elem:'record) : string =
        let schemaType=typeof<'record>
        Reflection.FSharpType.GetRecordFields(schemaType) 
        |> Seq.map(fun field -> (FSharpValue.GetRecordField(elem,field)))
        |> Seq.map(fun field ->
            match field with
            | :? string as t -> t
            | :? Option<string> as t -> Option.defaultValue "" t
            | :? int as t -> string t
            | :? Option<int> as t -> match t with | Some a -> string a | None -> ""
            | :? Result<string,string> as t -> writeStrStr t
            | :? Result<DateTime,string> as t -> writeDt t
            | _ ->
                let x = field
                "not implemented"
        )
        |> Seq.reduce(fun t y -> t + "\t" + y)
        //|> (fun t -> writeRecordHeader elem t)