module Validation
    type Result<'Ok,'Err> = 
        | Ok of 'Ok
        | Err of 'Err
    
    let andAlso (callback : 'Ok -> Result<'Ok,'Err>) (result : Result<'Ok,'Err>) : Result<'Ok,'Err> =
        match result with 
            | Ok value -> callback value
            | Err msg -> Err msg

    let isNotNullOrEmpty (input:string) : Result<string,string> =
        if System.String.IsNullOrWhiteSpace input then
            Err "SSN cannot be null or empty"
        else
            Ok input
    // todo validate
    let takeLast (amount:int) (str:string) : Result<string,string> = 
        Ok (str.Substring(str.Length - amount))

    let exactly (amount:int) (str:string) : Result<string,string> = 
        if str.Length <> amount then 
            Err (sprintf "SSN mus best exactly %d characters" amount)
        else
            Ok str

    let between (minimum:int) (maximum:int) (str:string) : Result<string,string> =
        if str.Length < minimum then
            Err "string is too small"
        else if str.Length > maximum then
            Err "string is too long"
        else
            Ok str

    let allInt (str:string) : Result<string,string> =
        if Seq.forall System.Char.IsDigit str then 
            Ok str
        else 
            Err "String is not all digits"

    let dateFromString(str:string) : Result<System.DateTime,string> =
        isNotNullOrEmpty str
        |> andAlso (exactly 8)
        |> (fun result -> 
            match result with 
            | Ok str ->
                let yearPart = int (str.Substring(0, 4))
                let monthPart = int (str.Substring(4, 2))
                let dayPart = int (str.Substring(6, 2))
                Ok (System.DateTime(yearPart, monthPart, dayPart, 0, 0, 0, System.DateTimeKind.Utc))
            | Err t ->
                Err t
        )
                
            