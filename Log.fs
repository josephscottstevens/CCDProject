module Log
    open System
    open Microsoft.FSharp.Reflection
    open Types

    let onlyLettersOrSpaces (c:char) : bool =
        Char.IsLetterOrDigit c = true || c = ' '

    let filterOnlyLettersOrSpaces (str:string) : string =
        String.filter onlyLettersOrSpaces str

    let handleError func t : string =
        match t with
        | Ok a -> func a
        | Error str -> sprintf "=HYPERLINK(\"%s\")" (filterOnlyLettersOrSpaces str)

    let writeDt (dt:DateTime) : string =
        dt.ToString()
    
    let writeAllergy (allergies:Allergy array) : string =
        allergies 
        |> Array.map(fun t -> (filterOnlyLettersOrSpaces t.name) 
                                + ":" 
                                + (t.reaction |> Option.defaultValue "none" |> filterOnlyLettersOrSpaces)
                    )
        |> Array.reduce(fun t y -> t + ", " + y)
    
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
            | :? Result<string,string> as t -> handleError id  t
            | :? Result<DateTime,string> as t -> handleError writeDt t
            | :? Result<Allergy array,string> as t -> handleError writeAllergy t
            | _ ->
                let x = field
                "not implemented"
        )
        |> Seq.reduce(fun t y -> t + "\t" + y)
        //|> (fun t -> writeRecordHeader elem t)