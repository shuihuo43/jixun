using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SectorRange : MonoBehaviour
{
    public enum WarningShape
    {
        Sector,
        Box
    }


    [Header("形状")]
    [SerializeField]
    private WarningShape warningShape = WarningShape.Sector;


    [SerializeField]
    [Range(1f, 360f)]
    private float angle = 360;


    [SerializeField]
    [Range(0.1f, 20f)]
    private float radius = 3f;


    [SerializeField]
    [Range(0f, 20f)]
    private float innerRadius = 0f;


    [SerializeField]
    [Range(1, 60)]
    private int quality = 6;



    [Header("朝向")]
    [SerializeField]
    private float direction = 0f;


    /// <summary>
    /// 0度=右
    /// 90度=上
    /// </summary>
    public float Direction
    {
        get => direction;
        set => direction = value;
    }



    public WarningShape Shape
    {
        get => warningShape;
        set => warningShape = value;
    }




    public void SyncFromShapeArea(ShapeArea sa)
    {
        if (sa == null)
            return;


        warningShape =
            (int)sa.Shape == 0
            ?
            WarningShape.Sector
            :
            WarningShape.Box;


        angle = sa.Angle;
        radius = sa.Radius;

        boxWidth = sa.BoxWidth;
        boxHeight = sa.BoxHeight;


        direction = sa.Direction;
    }






    [Header("进度")]
    [SerializeField]
    [Range(0f, 1f)]
    private float process = 0.5f;



    public float Process
    {
        get => process;
        set => process = Mathf.Clamp01(value);
    }



    public float SectorAngle
    {
        get => angle;
        set => angle = Mathf.Clamp(value, 1f, 360f);
    }



    public float SectorRadius
    {
        get => radius;
        set => radius = value;
    }







    [Header("外层")]
    [SerializeField]
    private Material outerMaterial;


    [SerializeField]
    private Color outerColor =
        new Color(1, 1, 1, 0.3f);




    [Header("内层")]
    [SerializeField]
    private Material innerMaterial;


    [SerializeField]
    private Color innerColor =
        new Color(1, 0, 0, 0.6f);




    private GameObject outerObj;
    private MeshFilter outerFilter;
    private MeshRenderer outerRenderer;
    private Material outerInst;



    private GameObject innerObj;
    private MeshFilter innerFilter;
    private MeshRenderer innerRenderer;
    private Material innerInst;





    [Header("矩形")]
    [SerializeField]
    private float boxWidth = 2f;


    [SerializeField]
    private float boxHeight = 1f;







    void Update()
    {
        EnsureObjects();



        if (warningShape == WarningShape.Box)
        {

            float fillWidth =
                boxWidth * process;


            ApplyBoxMesh(
                outerFilter,
                outerRenderer,
                ref outerInst,
                outerMaterial,
                outerColor,
                boxWidth,
                boxHeight,
                0
            );



            ApplyBoxMesh(
                innerFilter,
                innerRenderer,
                ref innerInst,
                innerMaterial,
                innerColor,
                fillWidth,
                boxHeight,
                1
            );

        }
        else
        {

            float innerLayerRadius =
                Mathf.Lerp(
                    innerRadius,
                    radius,
                    process
                );



            ApplyLayer(
                outerObj,
                outerFilter,
                outerRenderer,
                ref outerInst,
                outerMaterial,
                outerColor,
                angle,
                radius
            );



            ApplyLayer(
                innerObj,
                innerFilter,
                innerRenderer,
                ref innerInst,
                innerMaterial,
                innerColor,
                angle,
                innerLayerRadius
            );
        }

    }






    private void EnsureObjects()
    {

        if (outerObj == null)
        {
            outerObj =
                new GameObject("SectorOuter");


            outerObj.transform
                .SetParent(transform, false);



            outerFilter =
                outerObj.AddComponent<MeshFilter>();


            outerRenderer =
                outerObj.AddComponent<MeshRenderer>();
        }




        if (innerObj == null)
        {

            innerObj =
                new GameObject("SectorInner");


            innerObj.transform
                .SetParent(transform, false);



            innerFilter =
                innerObj.AddComponent<MeshFilter>();


            innerRenderer =
                innerObj.AddComponent<MeshRenderer>();

        }

    }






    private void ApplyLayer(
        GameObject obj,
        MeshFilter filter,
        MeshRenderer renderer,
        ref Material inst,
        Material src,
        Color color,
        float layerAngle,
        float layerRadius)
    {


        bool visible =
            layerRadius >
            innerRadius + 0.001f
            &&
            layerAngle > 0;



        obj.SetActive(visible);


        if (!visible)
            return;




        if (inst == null)
        {

            if (src != null)
                inst = new Material(src);

            else
                inst =
                new Material(
                    Shader.Find("Sprites/Default")
                );



            if (!inst.HasProperty("_Color"))
                inst.shader =
                    Shader.Find("Sprites/Default");

        }




        filter.mesh =
            BuildMesh(
                layerAngle,
                layerRadius
            );



        inst.color = color;


        renderer.sharedMaterial = inst;


        renderer.sortingOrder =
            renderer == innerRenderer
            ?
            1
            :
            0;

    }
    private Mesh BuildMesh(float meshAngle, float meshRadius)
    {

        int segCount = quality;

        float eachAngle =
            meshAngle / segCount;



        List<Vector3> vertices =
            new List<Vector3>();

        List<Vector2> uvs =
            new List<Vector2>();





        // 实心扇形
        if (innerRadius <= 0f)
        {

            vertices.Add(Vector3.zero);

            uvs.Add(new Vector2(0.5f, 0));



            for (int i = 0; i <= segCount; i++)
            {

                Vector3 dir =
                    Quaternion.Euler(
                        0,
                        0,
                        direction
                        +
                        meshAngle / 2
                        -
                        eachAngle * i
                    )
                    *
                    Vector2.right;



                vertices.Add(
                    dir * meshRadius
                );


                uvs.Add(
                    new Vector2(
                        (float)i / segCount,
                        1
                    )
                );

            }

        }

        // 环形扇区
        else
        {

            for (int i = 0; i <= segCount; i++)
            {

                Vector3 dir =
                    Quaternion.Euler(
                        0,
                        0,
                        direction
                        +
                        meshAngle / 2
                        -
                        eachAngle * i
                    )
                    *
                    Vector2.right;



                vertices.Add(
                    dir * innerRadius
                );


                uvs.Add(
                    new Vector2(
                        (float)i / segCount,
                        0
                    )
                );

            }





            for (int i = 0; i <= segCount; i++)
            {

                Vector3 dir =
                    Quaternion.Euler(
                        0,
                        0,
                        direction
                        +
                        meshAngle / 2
                        -
                        eachAngle * i
                    )
                    *
                    Vector2.right;



                vertices.Add(
                    dir * meshRadius
                );


                uvs.Add(
                    new Vector2(
                        (float)i / segCount,
                        1
                    )
                );

            }

        }





        List<int> triangles =
            new List<int>();




        if (innerRadius <= 0f)
        {

            for (int i = 0; i < segCount; i++)
            {

                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i + 2);

            }

        }

        else
        {

            int n = segCount + 1;


            for (int i = 0; i < segCount; i++)
            {

                int i0 = i;
                int i1 = i + 1;

                int o0 = n + i;
                int o1 = n + i + 1;



                triangles.Add(i0);
                triangles.Add(o0);
                triangles.Add(i1);



                triangles.Add(o0);
                triangles.Add(o1);
                triangles.Add(i1);

            }

        }




        Mesh mesh =
            new Mesh();


        mesh.vertices =
            vertices.ToArray();


        mesh.triangles =
            triangles.ToArray();


        mesh.uv =
            uvs.ToArray();


        mesh.RecalculateNormals();

        mesh.RecalculateBounds();


        return mesh;

    }









    private void ApplyBoxMesh(
        MeshFilter filter,
        MeshRenderer renderer,
        ref Material inst,
        Material src,
        Color color,
        float w,
        float h,
        int sortingOrder)
    {


        bool visible =
            w > 0.001f
            &&
            h > 0.001f;



        if (filter == innerFilter)
            innerObj.SetActive(visible);

        else
            outerObj.SetActive(visible);



        if (!visible)
            return;




        if (inst == null)
        {

            inst =
                src != null
                ?
                new Material(src)
                :
                new Material(
                    Shader.Find("Sprites/Default")
                );


            if (!inst.HasProperty("_Color"))
                inst.shader =
                    Shader.Find("Sprites/Default");

        }




        filter.mesh =
            BuildBoxMesh(w, h);



        inst.color = color;


        renderer.sharedMaterial = inst;


        renderer.sortingOrder =
            sortingOrder;

    }









    private Mesh BuildBoxMesh(
        float w,
        float h)
    {


        float hw = w / 2f;
        float hh = h / 2f;



        Vector3 Rotate(Vector3 pos)
        {

            return
                Quaternion.Euler(
                    0,
                    0,
                    direction
                )
                *
                pos;

        }





        Mesh mesh =
            new Mesh();



        mesh.vertices =
            new Vector3[]
            {

                Rotate(
                    new Vector3(
                        -hw,
                        -hh
                    )
                ),


                Rotate(
                    new Vector3(
                        hw,
                        -hh
                    )
                ),


                Rotate(
                    new Vector3(
                        hw,
                        hh
                    )
                ),


                Rotate(
                    new Vector3(
                        -hw,
                        hh
                    )
                )

            };




        mesh.triangles =
            new int[]
            {
                0,2,1,
                0,3,2
            };



        mesh.uv =
            new Vector2[]
            {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(1,1),
                new Vector2(0,1)
            };



        mesh.RecalculateNormals();

        mesh.RecalculateBounds();


        return mesh;

    }
    private void OnDrawGizmos()
    {

        float eachAngle =
            angle / quality;


        Vector3 center =
            transform.position;



        Vector3 startDir =
            Quaternion.Euler(
                0,
                0,
                direction + angle / 2
            )
            *
            Vector2.right;



        Vector3 endDir =
            Quaternion.Euler(
                0,
                0,
                direction - angle / 2
            )
            *
            Vector2.right;





        // 外边界
        Gizmos.color = Color.yellow;


        Gizmos.DrawLine(
            center,
            center + startDir * radius
        );


        Gizmos.DrawLine(
            center,
            center + endDir * radius
        );





        // 外弧
        Gizmos.color = Color.green;


        Vector3 last =
            center + startDir * radius;



        for (int i = 1; i <= quality; i++)
        {

            Vector3 dir =
                Quaternion.Euler(
                    0,
                    0,
                    direction
                    +
                    angle / 2
                    -
                    eachAngle * i
                )
                *
                Vector2.right;



            Vector3 next =
                center + dir * radius;



            Gizmos.DrawLine(
                last,
                next
            );


            last = next;

        }







        // 当前蓄力进度
        float innerLayerRadius =
            Mathf.Lerp(
                innerRadius,
                radius,
                process
            );



        if (innerLayerRadius >
            innerRadius + 0.001f)
        {

            Gizmos.color =
                Color.red;



            Vector3 lastInner =
                center +
                startDir *
                innerLayerRadius;




            for (int i = 1; i <= quality; i++)
            {

                Vector3 dir =
                    Quaternion.Euler(
                        0,
                        0,
                        direction
                        +
                        angle / 2
                        -
                        eachAngle * i
                    )
                    *
                    Vector2.right;



                Vector3 next =
                    center +
                    dir *
                    innerLayerRadius;



                Gizmos.DrawLine(
                    lastInner,
                    next
                );


                lastInner = next;

            }




            Gizmos.DrawLine(
                center + startDir * innerRadius,
                center + startDir * innerLayerRadius
            );


            Gizmos.DrawLine(
                center + endDir * innerRadius,
                center + endDir * innerLayerRadius
            );

        }







        // 内圈
        if (innerRadius > 0f)
        {

            Gizmos.color =
                Color.yellow;



            Gizmos.DrawLine(
                center,
                center + startDir * innerRadius
            );


            Gizmos.DrawLine(
                center,
                center + endDir * innerRadius
            );



            Gizmos.color =
                Color.green;



            last =
                center +
                startDir *
                innerRadius;



            for (int i = 1; i <= quality; i++)
            {

                Vector3 dir =
                    Quaternion.Euler(
                        0,
                        0,
                        direction
                        +
                        angle / 2
                        -
                        eachAngle * i
                    )
                    *
                    Vector2.right;



                Vector3 next =
                    center +
                    dir *
                    innerRadius;



                Gizmos.DrawLine(
                    last,
                    next
                );


                last = next;

            }

        }

    }









    /// <summary>
    /// 攻击完成后渐隐销毁
    /// </summary>
    public void DestroyWithFade(
        float fadeDuration = 0.15f)
    {

        StopAllCoroutines();


        StartCoroutine(
            FadeAndDestroy(
                fadeDuration
            )
        );

    }






    private IEnumerator FadeAndDestroy(
        float duration)
    {

        float outerStartAlpha =
            outerInst != null
            ?
            outerInst.color.a
            :
            0f;



        float innerStartAlpha =
            innerInst != null
            ?
            innerInst.color.a
            :
            0f;





        float elapsed = 0f;



        while (elapsed < duration)
        {

            elapsed += Time.deltaTime;


            float t =
                elapsed / duration;



            float alpha =
                1f - t;





            if (outerInst != null)
            {

                Color c =
                    outerInst.color;


                c.a =
                    outerStartAlpha *
                    alpha;


                outerInst.color = c;

            }






            if (innerInst != null)
            {

                Color c =
                    innerInst.color;


                c.a =
                    innerStartAlpha *
                    alpha;


                innerInst.color = c;

            }




            yield return null;

        }





        Destroy(gameObject);

    }

}