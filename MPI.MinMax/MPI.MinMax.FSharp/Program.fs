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
        printfn "Root found min value %d and max value %d" minValueSubVector maxValueSubVector
        let minMaxTuples = world.Gather(minMaxTuple, 0)
        let minValue = minMaxTuples |> Seq.map (fun x -> fst(x)) |> Seq.min
        let maxValue = minMaxTuples |> Seq.map (fun x -> snd(x)) |> Seq.max
        printfn "Min and max values are %d and %d accordingly" minValue maxValue
    else
        let subVector = world.Scatter<int[]>(0)        
        let minValueSubVector = subVector |> Array.min
        let maxValueSubVector = subVector |> Array.max
        let minMaxTuple = (minValueSubVector, maxValueSubVector)
        printfn "Process with rank %d found min value %d and max value %d" world.Rank minValueSubVector maxValueSubVector
        world.Gather(minMaxTuple, 0) |> ignore

    env.Dispose()
    0