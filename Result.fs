module Result
    let toOption result = 
        match result with 
        | Ok t -> Some t
        | Error _ -> None

    let fromOption err result =
        match result with
        | Some t -> Ok t
        | None -> Error err
    
    let bind2 func ra rb =
        match (ra, rb) with
        | (Ok a, Ok b) -> Ok (func a b)
        | (Error x, _) -> Error x
        | (_, Error x) -> Error x

    let dateToStr (dt:System.DateTime) : string =
        dt.ToString()

    let toStr (t:string option) : string =
        Option.defaultValue "none" t
    
    let intToStr (someMaybeInt:int64 option) : string =
        match someMaybeInt with 
        | Some t -> string t
        | None -> "" 

    let onlyLettersOrSpaces (c:char) : bool =
        System.Char.IsLetterOrDigit c = true || c = ' '

    let filterOnlyLettersOrSpaces (str:string) : string =
        String.filter onlyLettersOrSpaces str