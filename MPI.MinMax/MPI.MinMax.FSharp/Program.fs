#light

open System
open MPI

[<EntryPoint>]
let main args =
    let env = new Environment(ref args)
    
    let elementsPerProcessor = 500    
    let world = Communicator.world
    let random = Random()
    
    if world.Rank = 0 then
        let vector : int[][] = Array.init world.Size (fun _ -> Array.init elementsPerProcessor (fun _ -> random.Next(-100000, 100000)))

        let subVector = world.Scatter<int[]>(vector, 0)
        let minValueSubVector = subVector |> Array.min
        let maxValueSubVector = subVector |> Array.max
        let minMaxTuple = (minValueSubVector, maxValueSubVector)
        printfn $"Root found min value %d{minValueSubVector} and max value %d{maxValueSubVector}"
        let minMaxTuples = world.Gather(minMaxTuple, 0)
        let minValue = minMaxTuples |> Seq.map (fst) |> Seq.min
        let maxValue = minMaxTuples |> Seq.map (snd) |> Seq.max
        printfn $"Min and max values are %d{minValue} and %d{maxValue} accordingly"
    else
        let subVector = world.Scatter<int[]>(0)        
        let minValueSubVector = subVector |> Array.min
        let maxValueSubVector = subVector |> Array.max
        let minMaxTuple = (minValueSubVector, maxValueSubVector)
        printfn $"Process with rank %d{world.Rank} found min value %d{minValueSubVector} and max value %d{maxValueSubVector}"
        world.Gather(minMaxTuple, 0) |> ignore

    env.Dispose()
    0