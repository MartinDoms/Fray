namespace Fray

open System
open System.Windows.Media.Media3D
open System.Windows.Forms
open System.Windows.Media.Media3D
module Fray =   

    let castRay ray scene =
        let intersectionFilter intersections =
            let i = intersections |> Seq.filter(fun x -> x.t > 0.001) |> Seq.toList
            match i with
            | [] -> None
            | _  -> Some (Seq.minBy(fun x -> x.t) i)
        scene.shapes                           |> 
        Seq.collect (fun x -> x.Intersect ray) |> 
        intersectionFilter

    let castRays (rays:seq<Ray>) scene =
        rays |> Seq.map (fun x -> (x, castRay x scene))

    let colorAt (ray, intersection) scene maxReflections =
        // nested function check if we're in shadow
        let inShadow light point =
            let ray = Ray(intersection.point, light.position - intersection.point)
            let shadowIntersection = castRay ray scene
            match shadowIntersection with
            | None -> false
            | _ -> 
                let (l1, l2) = ((light.position - intersection.point).Length, (shadowIntersection.Value.point - intersection.point).Length)
                l1 > l2

        // nested function for ambient color
        let ambientColorAt (intersection:Intersection) =
            scene.ambientLight * intersection.material.diffuseColor

        // nested function for specular color
        let specularColorAt (intersection:Intersection) hitPoint hitNormal =
            let ks = intersection.material.specularColor
            let V = (scene.camera.position - hitPoint) |> VMath.norm
        
            let specularAtLight light =
                let L = (light.position - hitPoint) |> VMath.norm
                let H = VMath.norm (L + V)
                let Is = light.color * Math.Pow(Math.Max(0.0, Vector3D.DotProduct(H, hitNormal)), intersection.material.shininess)
                ks * Is
            Seq.sumBy(fun x -> specularAtLight x) scene.lights

        //nested function for diffuse color
        let diffuseColorAt (intersection:Intersection) hitPoint hitNormal =
            let kd = intersection.material.diffuseColor

            let diffuseAtLight light =
                let L = VMath.norm (light.position - hitPoint) |> VMath.norm
                let Id = light.color * Math.Max(0.0, Vector3D.DotProduct(L, hitNormal))
                if inShadow light intersection.point then Color(0.0,0.0,0.0)
                else kd * Id         
            Seq.sumBy(fun x -> diffuseAtLight x) scene.lights

        let rec traceColorAt intersection ray currentReflection =
            // some useful values to compute at the start
            let matrix = intersection.transformation |> VMath.transpose |> VMath.invert
            let transNormal = matrix.Transform(intersection.normal) |> VMath.norm
            let hitPoint = intersection.point

            let ambient = ambientColorAt intersection
            let specular = specularColorAt intersection hitPoint transNormal
            let diffuse = diffuseColorAt intersection hitPoint transNormal
            let primaryColor = ambient + diffuse + specular

            if currentReflection = 0 then 
                primaryColor
            else
                let direction = (ray:Ray).direction |> VMath.norm
                let reflectDir = (direction - (2.0 * (Vector3D.DotProduct(direction, transNormal) * transNormal))) |> VMath.norm
                let newRay = Ray(hitPoint, reflectDir)
                let reflectedIntersection = castRay newRay scene
                let reflectivity = intersection.material.reflectivity
                match reflectedIntersection with
                | None -> primaryColor
                | _ -> primaryColor + traceColorAt reflectedIntersection.Value newRay  (currentReflection - 1) * reflectivity

        traceColorAt intersection ray maxReflections   

    let perturbRay (ray:Ray) spawnedRays aperture focalDistance =
        let random = Random()
        let lookAt = ray.origin + ray.direction * focalDistance // focal length
        let rayList = []
        let newRay = (fun () ->
            let dx = aperture * (float(random.Next(100))-50.0)/50.0
            let dy = aperture * (float(random.Next(100))-50.0)/50.0
            let dz = aperture * (float(random.Next(100))-50.0)/50.0
            let newOrigin = Point3D( ray.origin.X + dx,
                                     ray.origin.Y + dy,
                                     ray.origin.Z + dz )
            Ray(newOrigin, lookAt - newOrigin))
        List.map (fun x -> newRay()) [0..spawnedRays]

    let RayTrace width height reflections scene =
        // Vertical and horizontal field of view
        let hfov = scene.camera.fieldOfView
        let vfov = hfov * float(height)/float(width)

        // Pixel width and height
        let pw = 2.0 * System.Math.Tan(float(hfov/2.0))/float(width)
        let ph = 2.0 * System.Math.Tan(float(vfov/2.0))/float(height)    

        // set up the coordinate system
        let n = VMath.norm (scene.camera.position - scene.camera.lookAt)
        let u = VMath.norm (Vector3D.CrossProduct(n, scene.camera.lookUp))
        let v = VMath.norm (Vector3D.CrossProduct(n, u))
        let vpc = scene.camera.position - n

        let result = seq {
            for y in 0..height-1 do
                for x in 0..width-1 do
                    let rayPoint = vpc + float(x-width/2)*pw*u + float(y-height/2)*ph*v
                    let rayDir = VMath.norm (rayPoint - scene.camera.position)
                    let ray = Ray(scene.camera.position, direction = rayDir)
                    let rays = perturbRay ray 24 0.1 7.8
                    let intersections = castRays rays scene
                    let colors = 
                        intersections |> 
                        Seq.filter (fun (r,i) -> i.IsSome) |> 
                        Seq.map (fun (r,i) -> colorAt (r,i.Value) scene reflections) |>
                        Seq.toList
                    let color = 
                        match colors with
                        | [] -> Color(0.0,0.0,0.0)
                        | _ -> colors |> Seq.toList |> Color.Average
                    yield (x, y, color)
        }

        result