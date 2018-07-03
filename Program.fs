
open System
open System.IO
open System.Drawing

let pattern = "*.bmp|*.gif|*.png"

let bundle files =
    let dim = Seq.length files |> float |> sqrt
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
        [0..cols] |> List.collect (fun c -> 
        [0..rows] |> List.map (fun r ->
            let x, y = c * cellWidth, r * cellHeight
            let image = allImages.[c + (r * cols)]
            let destRect = new Rectangle (x, y, cellWidth, cellHeight)
            graphics.DrawImage (snd image, destRect) |> ignore
            fst image, x, y))

    let positionText = 
        positions 
        |> List.map (fun (name, x, y) -> sprintf "%s,%i,%i\r\n" name x y)
        |> String.concat "\r\n"
    
    targetImage, positionText

[<EntryPoint>]
let main argv =
    let path = if Array.length argv > 1 then argv.[1] else "."
    let files = Directory.GetFiles (path, pattern)

    if not <| Array.isEmpty files then 
        let image, info = bundle files
        image.Save("./output.png") |> ignore
        File.WriteAllText ("./output.txt", info) |> ignore
        0
    else    
        -1