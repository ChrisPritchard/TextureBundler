open System.IO
open System.Drawing
open System.Drawing.Imaging

let patterns = [|"*.bmp";"*.gif";"*.png";"*.jpg"|]

let bundle files outputPath =
    let length = Seq.length files
    let dim = length |> float |> sqrt
    let cols, rows = ceil dim |> int, floor dim |> int

    let allImages = 
        files |> Seq.map (fun (p: string) -> 
        Path.GetFileNameWithoutExtension p, Bitmap.FromFile p)
        |> Seq.toList

    let allDims = allImages |> Seq.map (fun (_, i) -> i.Width, i.Height)
    let cellWidth, cellHeight = allDims |> Seq.map fst |> Seq.max, allDims |> Seq.map snd |> Seq.max
    
    use targetImage = new Bitmap (cols * cellWidth, rows * cellHeight)
    use graphics = Graphics.FromImage targetImage

    let positions = 
        [0..rows-1] |> List.collect (fun r -> 
        [0..cols-1] |> List.filter (fun c -> c + (r * cols) < length)
                    |> List.map (fun c ->
            let x, y = c * cellWidth, r * cellHeight
            let imageInfo = allImages.[c + (r * cols)]
            let image = snd imageInfo
            let destRect = new Rectangle (x, y, image.Width, image.Height)
            graphics.DrawImage (image, destRect) |> ignore
            fst imageInfo, x, y, image.Width, image.Height))

    let positionText = 
        sprintf "name,x,y,width,height\r\n%s"
            <| (positions 
                |> List.map (fun (name, x, y, w, h) -> sprintf "%s,%i,%i,%i,%i" name x y w h)
                |> String.concat "\r\n")

    targetImage.Save ((Path.Combine(outputPath, "./output.png")), ImageFormat.Png) |> ignore
    File.WriteAllText ((Path.Combine(outputPath, "./output.csv")), positionText) |> ignore

[<EntryPoint>]
let main argv =
    let (path, out) =
        match argv with
        | [|inPath|] -> inPath,"./"
        | [|inPath;outPath|] -> inPath,outPath
        | _ -> "./","./"
    if Directory.Exists path |> not then
        printfn "Directory not found"
        -1
    else
        let files = patterns |> Array.collect (fun p -> Directory.GetFiles (path, p)) |> Array.sort
        if not <| Array.isEmpty files then 
            bundle files out
            printfn "Done"
            0
        else
            printfn "No files found"
            -2