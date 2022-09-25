#!/usr/bin/dotnet fsi
#load "utils.fsx"
open Utils
open System
open System.IO
open System.IO.Compression
open System.Threading

while true do
    printfn "starting ..."
    try
        cd ["bin"]

        let bin = Environment.CurrentDirectory

        let file = 
            DirectoryInfo(".").EnumerateFiles()
            |> Seq.find (fun x -> x.Name = "InglemoorCodingComputing.dll")

        let proc = execP "sudo" $"-E dotnet \"{file.FullName}\""

        cd []
        
        let mutable broken = false;
        while not broken do
            // check every 10 seconds for a new version
            Thread.Sleep 10000
            DirectoryInfo(".").EnumerateFiles()
            |> Seq.tryFind (fun x -> x.Name = "package.zip")
            |> function
            | Some zip -> 
                proc.Kill();

                Directory.Delete(bin, true)
                cd ["bin"]

                ZipFile.ExtractToDirectory(zip.FullName, bin)

                broken <- true;
            | _ -> 
                ()
    with _ -> 
        ()

    printfn "process ended, waiting to restart"
    Thread.Sleep 5000
    printf "re"