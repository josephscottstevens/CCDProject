module Log
    open System
    open Microsoft.FSharp.Reflection
    open Types
    open Result

    let handleError func t : string =
        match t with
        | Ok a -> func a
        | Error str -> sprintf "=HYPERLINK(\"%s\")" (filterOnlyLettersOrSpaces str)

    let reducer (items:array<list<string>>) : string =
        items
        |> Array.map(fun arr -> List.map filterOnlyLettersOrSpaces arr)
        |> Array.map(fun arr -> List.reduce (fun t y -> t + " " + y) arr)
        |> Array.reduce(fun t y -> t + "|" + y) 

    let writeAllergy (items:Allergy array) : string =
        items 
        |> Array.map(fun t -> [t.name; toStr t.reaction])
        |> reducer

    let writeProblems (items:Problem array) : string =
        items
        |> Array.map(fun t -> [ toStr t.name
                              ; intToStr t.code
                              ; toStr t.codeSystem
                              ; handleError dateToStr t.date
                              ; toStr t.status
                              ]
                    )
        |> reducer

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
            | :? Result<DateTime,string> as t -> handleError dateToStr t
            | :? Result<Allergy array,string> as t -> handleError writeAllergy t
            | :? Result<Problem array,string> as t -> handleError writeProblems t
            | :? Result<Medication array,string> as t -> handleError (fun x -> x.ToString()) t
            | :? Result<Vital array,string> as t -> handleError (fun x -> x.ToString()) t
            | :? Result<Encounter array,string> as t -> handleError (fun x -> x.ToString()) t
            | :? Result<Immunization array,string> as t -> handleError (fun x -> x.ToString()) t
            | :? Result<CCDRecord array,string> as t -> handleError (fun x -> x.ToString()) t
            | _ ->
                let x = field
                "not implemented"
        )
        |> Seq.reduce(fun t y -> t + "\t" + y)
        //|> (fun t -> writeRecordHeader elem t)