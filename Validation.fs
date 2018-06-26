module Validation
    type Result<'Ok,'Err> = 
        | Ok of 'Ok
        | Err of 'Err
    
    let andThen (callback : 'Ok -> Result<'Ok,'Err>) (result : Result<'Ok,'Err>) : Result<'Ok,'Err> =
        match result with 
            | Ok value -> callback value
            | Err msg -> Err msg

    let isNotNullOrEmpty (input:string) : Result<string,string> =
        if System.String.IsNullOrWhiteSpace input then
            Err "SSN cannot be null or empty"
        else
            Ok input

    let exactlyFour (str:string) : Result<string,string> = 
        if str.Length <> 4 then 
            Err "SSN mus best exactly 4 characters"
        else
            Ok str