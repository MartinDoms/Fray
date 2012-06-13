namespace Fray

open System

type Color(r: float, g: float, b:float) =
    member this.r = r
    member this.g = g
    member this.b = b

    static member ( * ) (c1:Color, c2:Color) =
        Color (c1.r*c2.r, c1.g*c2.g, c1.b*c2.b)
    static member ( * ) (c:Color, s:float) =
        Color (c.r*s, c.g*s, c.b*s)
    static member ( + ) (c1:Color, c2:Color) =
        let r = Math.Min (c1.r+c2.r, 1.0)
        let g = Math.Min (c1.g+c2.g, 1.0)
        let b = Math.Min (c1.b+c2.b, 1.0)
        Color (r,g,b)
    static member Zero = Color(0.0,0.0,0.0)
    static member DivideByInt (c:Color, num:int) = 
        Color(c.r/float(num), c.g/float(num), c.b/float(num))

    static member Average (colors:Color seq) =
        let r = (Seq.sumBy (fun (x:Color) -> x.r) colors)/float(Seq.length colors)
        let g = (Seq.sumBy (fun (x:Color) -> x.g) colors)/float(Seq.length colors)
        let b = (Seq.sumBy (fun (x:Color) -> x.b) colors)/float(Seq.length colors)
        Color(r,g,b)