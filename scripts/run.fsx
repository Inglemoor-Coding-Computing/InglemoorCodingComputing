#load "utils.fsx"
open Utils
open System
open System.IO
open System.Threading

while true do
    printfn "starting ..."
    try
        // find the latest version
        cd ["bin"]
        
        let directories = 
            DirectoryInfo(".").EnumerateDirectories() 
            |> Seq.cache
        
        let newest =
            directories
            |> Seq.maxBy (fun x -> 
                match DateTime.TryParse(x.Name.Replace('_', ':')) with 
                | true, v -> v 
                | _ -> DateTime.MinValue)

        // delete the rest
        for dir in directories do
            if dir.Name <> newest.Name then
                dir.Delete true

        let file =
            newest.EnumerateFiles()
            |> Seq.where (fun x -> x.Name = "InglemoorCodingComputing.dll")
            |> Seq.head

        exec "sudo" $"-E dotnet \"{file.FullName}\" --urls \"http://*:80;https://*:443\""
    with _ ->
        kill ()

    printfn "process ended, waiting to restart"
    Thread.Sleep 5000
    printf "re"