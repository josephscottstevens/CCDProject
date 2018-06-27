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