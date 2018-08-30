// Learn more about F# at http://fsharp.org

open System
open LightStep
open OpenTracing

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let tracerOptions = LightStep.Options("TEST_TOKEN", SatelliteOptions("localhost", 9996, false))
    let tracer = LightStep.Tracer(tracerOptions)
    OpenTracing.Util.GlobalTracer.Register(tracer)
    for i = 0 to 1000 do
        use scope = tracer.BuildSpan("main").StartActive(true)
        scope.Span.Log("test") |> ignore
        Threading.Thread.Sleep(Random().Next(1, 100))
        tracer.ActiveSpan.Log(sprintf "iteration %d" i) |> ignore
    tracer.Flush()
    0 // return an integer exit code
