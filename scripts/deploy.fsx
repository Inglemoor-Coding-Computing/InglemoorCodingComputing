#!/usr/bin/dotnet fsi
open System.IO.Compression

ZipFile.CreateFromDirectory("InglemoorCodingComputing/publish", "package.zip");