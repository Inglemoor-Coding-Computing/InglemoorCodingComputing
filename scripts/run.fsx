#!/usr/bin/dotnet fsi
#load "utils.fsx"
open Utils
open System
open System.IO
open System.IO.Compression
open System.Threading

while true do
    printfn "starting ..."

    cd ["bin"]

    let bin = Environment.CurrentDirectory

    let proc =
        try
            let file = 
                DirectoryInfo(".").EnumerateFiles()
                |> Seq.find (fun x -> x.Name = "InglemoorCodingComputing.dll")

            printfn $"EXECUTING: {file.FullName}"

            execP "dotnet" $" \"{file.FullName}\""
            |> Some
        with _ -> 
            None

    try
        cd []
        
        let mutable broken = false;
        while not broken do
            // check every 3 seconds
            Thread.Sleep 3000
            DirectoryInfo(".").EnumerateFiles()
            |> Seq.tryFind (fun x -> x.Name = "package.zip")
            |> function
            | Some zip -> 
                printfn "Updating"
                proc 
                |> Option.iter (fun proc -> proc.Kill())

                Directory.Delete(bin, true)
                cd ["bin"]

                ZipFile.ExtractToDirectory(zip.FullName, bin)

                File.Delete zip.FullName

                broken <- true;
            | _ -> 
                ()
            broken <-
                match proc with 
                | None -> true
                | Some x -> x.HasExited
    with _ ->
        ()
    printfn "process ended, waiting to restart"
    Thread.Sleep 1000
    printf "re"