module Validation
    let isNotNullOrEmpty (fieldName:string) (input:string) : Result<string,string> =
        if System.String.IsNullOrWhiteSpace input then
            Error (sprintf "%s cannot be null or empty" fieldName)
        else
            Ok input

    let takeLast (amount:int) (fieldName:string) (str:string) : Result<string,string> = 
        if amount > str.Length then
            Error (sprintf "%s is out of range" fieldName)
        else
            Ok (str.Substring(str.Length - amount))

    let exactly (amount:int) (fieldName:string) (str:string) : Result<string,string> = 
        if str.Length <> amount then 
            Error (sprintf "%s must be exactly %d characters" fieldName amount)
        else
            Ok str

    let between (minimum:int) (maximum:int) (fieldName:string) (str:string) : Result<string,string> =
        if str.Length < minimum then
            Error (sprintf "%s is too small" fieldName)
        else if str.Length > maximum then
            Error (sprintf "%s is too long" fieldName)
        else
            Ok str

    let allInt (str:string) : Result<string,string> =
        if Seq.forall System.Char.IsDigit str then 
            Ok str
        else 
            Error "String is not all digits"

    let dateFromString (fieldName:string) (str:string) : Result<System.DateTime,string> =
        isNotNullOrEmpty fieldName str
        |> Result.bind (between  8 (8+9) fieldName)
        |> (fun result -> 
            match result with 
            | Ok str ->
                let yearPart = int (str.Substring(0, 4))
                let monthPart = int (str.Substring(4, 2))
                let dayPart = int (str.Substring(6, 2))
                if str.Length = 8 then
                    Ok (System.DateTime(yearPart, monthPart, dayPart, 0, 0, 0, System.DateTimeKind.Utc))
                else if str.Length = (8+9) then
                    // looks like this... but like... we don't care about any of that right?
                    // "201803071719-0600"
                    Ok (System.DateTime(yearPart, monthPart, dayPart, 0, 0, 0, System.DateTimeKind.Utc))
                else 
                    Error (sprintf "%s is too long or short" fieldName)
            | Error t ->
                Error t
        )
                
            