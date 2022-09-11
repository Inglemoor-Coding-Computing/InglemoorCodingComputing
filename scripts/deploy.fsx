#load "utils.fsx"
open Utils
open System

cd [ "src"; "InglemoorCodingComputing" ]

// Pull from main
exec "git" "pull"

printfn "fetch complete"

// Build
let name = DateTime.UtcNow.ToString("s").Replace(':', '_')
cd [ "bin"; name ]

let output = Environment.CurrentDirectory

cd [ "src"; "InglemoorCodingComputing"; "src"; "InglemoorCodingComputing" ]

exec "dotnet" "clean"

exec "npx" "tailwindcss -c tailwind.config.js -i Styles/app.css -o wwwroot/tailwind.css --minify"

let result = execR "dotnet" $"publish -c Release --output \"{output}\""

if result = 0 then
    printfn "build complete"
else
    printfn $"build failed: exit code {result}"
printfn $"generated: {name}"

kill ()