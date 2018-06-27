module Validation
    //let andThen (callback : 'a -> Result<'Ok,'Error>) (result : Result<'Ok,'Error>) : Result<'Ok,'Error> =
    let map func ra =
        match ra with
        | Ok a -> Ok (func a)
        | Error e -> Error e
    
    let andThen callback result =
        match result with 
            | Ok value -> callback value
            | Error msg -> Error msg

    let isNotNullOrEmpty (input:string) : Result<string,string> =
        if System.String.IsNullOrWhiteSpace input then
            Error "SSN cannot be null or empty"
        else
            Ok input
    // todo validate
    let takeLast (amount:int) (str:string) : Result<string,string> = 
        Ok (str.Substring(str.Length - amount))

    let exactly (amount:int) (str:string) : Result<string,string> = 
        if str.Length <> amount then 
            Error (sprintf "SSN mus best exactly %d characters" amount)
        else
            Ok str

    let between (minimum:int) (maximum:int) (str:string) : Result<string,string> =
        if str.Length < minimum then
            Error "string is too small"
        else if str.Length > maximum then
            Error "string is too long"
        else
            Ok str

    let allInt (str:string) : Result<string,string> =
        if Seq.forall System.Char.IsDigit str then 
            Ok str
        else 
            Error "String is not all digits"

    let fromOption err maybe =
        match maybe with
        | Some t -> Ok t
        | None -> Error err

    let dateFromString(str:string) : Result<System.DateTime,string> =
        isNotNullOrEmpty str
        |> andThen (exactly 8)
        |> (fun result -> 
            match result with 
            | Ok str ->
                let yearPart = int (str.Substring(0, 4))
                let monthPart = int (str.Substring(4, 2))
                let dayPart = int (str.Substring(6, 2))
                Ok (System.DateTime(yearPart, monthPart, dayPart, 0, 0, 0, System.DateTimeKind.Utc))
            | Error t ->
                Error t
        )
                
            