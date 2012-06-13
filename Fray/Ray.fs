namespace Fray

open System.Windows.Media.Media3D;

type Ray (origin:Point3D, direction:Vector3D) =
    member this.origin = origin
    member this.direction = direction

    member this.atTime t =
        this.origin + t * direction