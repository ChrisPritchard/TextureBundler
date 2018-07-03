
open System
open System.IO
open System.Drawing
open System.Drawing.Imaging

let patterns = [|"*.bmp";"*.gif";"*.png";"*.jpg"|]

let bundle files outputName =
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
        [0..cols-1] |> List.collect (fun c -> 
        [0..rows-1] |> List.filter (fun r -> c + (r * cols) < length)
                    |> List.map (fun r ->
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

    targetImage.Save (sprintf "./%s.png" outputName, ImageFormat.Png) |> ignore
    File.WriteAllText (sprintf "./%s.csv" outputName, positionText) |> ignore

[<EntryPoint>]
let main argv =
    let path = if Array.length argv > 0 then argv.[0] else "./"
    if Directory.Exists path |> not then
        printfn "Directory not found"
        -1
    else
        let files = patterns |> Array.collect (fun p -> Directory.GetFiles (path, p))
        if not <| Array.isEmpty files then 
            bundle files "output"
            printfn "Done"
            0
        else
            printfn "No files found"
            -2