namespace Fray

open System.Windows.Media.Media3D

module internal VMath =
    type Transformation = Matrix3D    

    let norm (v:Vector3D) = 
        let abs = sqrt (v.X * v.X + v.Y * v.Y + v.Z * v.Z)
        v / abs

    let invert (m:Matrix3D) = 
        let result = Matrix3D ( m.M11, m.M12, m.M13, m.M14,
                                m.M21, m.M22, m.M23, m.M24,
                                m.M31, m.M32, m.M33, m.M34,
                                m.OffsetX,   m.OffsetY,   m.OffsetZ,   m.M44 )
        result.Invert()
        result

    let transpose (m:Matrix3D) =
        let result = Matrix3D  ( m.M11, m.M21, m.M31, m.OffsetX,
                                    m.M12, m.M22, m.M32, m.OffsetY,
                                    m.M13, m.M23, m.M33, m.OffsetZ,
                                    m.M14, m.M24, m.M34, m.M44 )
        result

    let clone (m:Matrix3D) =
        let result = Matrix3D( m.M11, m.M12, m.M13, m.M14,
                                m.M21, m.M22, m.M23, m.M24,
                                m.M31, m.M32, m.M33, m.M34,
                                m.OffsetX, m.OffsetY, m.OffsetZ, m.M44 )
        result