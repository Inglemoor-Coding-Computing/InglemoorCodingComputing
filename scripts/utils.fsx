open System
open System.IO
open System.Diagnostics

let exec file args = 
    use proc = Process.Start(ProcessStartInfo(file, args, UseShellExecute=true))
    proc.WaitForExit()

let execP file args = 
    Process.Start(ProcessStartInfo(file, args, UseShellExecute=true))

let execR file args = 
    use proc = Process.Start(ProcessStartInfo(file, args, UseShellExecute=true))
    proc.WaitForExit()
    proc.ExitCode

let cd directory =
    let d = Path.Combine([ __SOURCE_DIRECTORY__ ] @ directory |> List.toArray)
    if Directory.Exists(d) |> not then
        Directory.CreateDirectory(d) |> ignore
    Environment.CurrentDirectory <- d
