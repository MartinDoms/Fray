namespace Fray

open System.Windows.Media.Media3D;

type Intersection = { normal:Vector3D; point:Point3D; transformation:Matrix3D; material:Fray.Material; t:float }