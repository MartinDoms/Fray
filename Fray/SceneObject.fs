namespace Fray

open VMath
    
    type Vector3D = System.Windows.Media.Media3D.Vector3D
    type Point3D = System.Windows.Media.Media3D.Point3D
    type Matrix3D = System.Windows.Media.Media3D.Matrix3D

    type Shape =
        | Sphere of Material * Transformation
        | Plane of Material * Transformation
        | BoundPlane of Material * Transformation
        | CompoundShape of Shape list * Transformation

        member s.Intersect (ray:Ray) =

            // a couple of inner utility functions
            let invertRay transformation =
                let inv = VMath.invert transformation
                let invRay =  Ray(inv.Transform(ray.origin), inv.Transform(ray.direction))
                invRay

            let pointToVector (p:Point3D) = Vector3D(p.X,p.Y,p.Z)

            match s with
            | Sphere (material, transformation) ->
                let invRay = invertRay transformation // ray tranformed by inverse matrix
                let A = Vector3D.DotProduct(invRay.direction, invRay.direction)
                let B = Vector3D.DotProduct(pointToVector invRay.origin, invRay.direction)
                let C = Vector3D.DotProduct(pointToVector invRay.origin, pointToVector invRay.origin) - 1.0
                let discr = B*B - A*C
                if discr < 0.0 then []
                else 
                    let normalAtTime t = 
                        invRay.atTime t |> pointToVector |> norm
                    let (t1,t2) = (-B/A + sqrt(discr)/A, -B/A - sqrt(discr)/A)
                    [ { normal = normalAtTime t1; point = ray.atTime t1; material=material; transformation=transformation; t=t1 };
                      { normal = normalAtTime t2; point = ray.atTime t2; material=material; transformation=transformation; t=t2 } ]

            | Plane (material, transformation) ->
                let invRay = invertRay transformation // ray transformed by inverse matrix
                let Sz = invRay.origin.Z
                let Cz = invRay.direction.Z;
                let t = -Sz/Cz;
                if t < 0.0 then []
                else
                    let normal = Vector3D(0.0,0.0,-1.0)
                    [ { normal = normal; point = ray.atTime t; material=material; transformation=transformation; t=t } ]

            | BoundPlane (material, transformation) ->
                let invRay = invertRay transformation // ray transformed by inverse matrix
                let Sz = invRay.origin.Z
                let Cz = invRay.direction.Z
                let t = -Sz/Cz
                if t < 0.0 then []
                else
                    let point = ray.atTime t
                    if point.X > 1.0 || point.X < -1.0 || point.Y > 1.0 || point.Y < -1.0 then []
                    else
                        let normal = Vector3D(0.0,0.0,-1.0)
                        [ { normal = normal; point = point; material=material; transformation=transformation; t=t } ]
            | CompoundShape (shapes, transformation) ->
                let invRay = invertRay transformation
                List.collect (fun (s:Shape) -> (s.Intersect invRay)) shapes
        