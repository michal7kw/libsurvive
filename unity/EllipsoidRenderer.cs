using UnityEngine;

public class EllipsoidRenderer : MonoBehaviour
{
    private Mesh ellipsoidMesh;
    private Material ellipsoidMaterial;

    private void Start()
    {
        ellipsoidMesh = new Mesh();
        ellipsoidMaterial = new Material(Shader.Find("Standard"));
        ellipsoidMaterial.color = Color.green;
        ellipsoidMaterial.SetFloat("_Mode", 3);
        ellipsoidMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        ellipsoidMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        ellipsoidMaterial.SetInt("_ZWrite", 0);
        ellipsoidMaterial.DisableKeyword("_ALPHATEST_ON");
        ellipsoidMaterial.EnableKeyword("_ALPHABLEND_ON");
        ellipsoidMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        ellipsoidMaterial.renderQueue = 3000;
    }

public void UpdateEllipsoid(Matrix4x4 u, Vector3 s, Matrix4x4 v)    
{
         float upscale = gameObject.name.EndsWith("'") ? 2f : 100f;

        for (int i = 0; i < 3; i++)
        {
            s[i] = Mathf.Max(Mathf.Sqrt(s[i]), 1e-5f) * upscale;
        }

        Matrix4x4 sm = Matrix4x4.Scale(s);
        Matrix4x4 vm = v.transpose;

        Matrix4x4 matrix = u * sm * vm;

        ellipsoidMesh.Clear();

        int numSegments = 32;
        int numStacks = 16;

        Vector3[] vertices = new Vector3[(numSegments + 1) * (numStacks + 1)];
        int[] indices = new int[numSegments * numStacks * 6];

        float segmentStep = 2f * Mathf.PI / numSegments;
        float stackStep = Mathf.PI / numStacks;

        int vertexIndex = 0;
        for (int i = 0; i <= numStacks; i++)
        {
            float stackAngle = Mathf.PI / 2 - i * stackStep;
            float xy = Mathf.Cos(stackAngle);
            float z = Mathf.Sin(stackAngle);

            for (int j = 0; j <= numSegments; j++)
            {
                float segmentAngle = j * segmentStep;

                float x = xy * Mathf.Cos(segmentAngle);
                float y = xy * Mathf.Sin(segmentAngle);

                vertices[vertexIndex] = matrix.MultiplyPoint3x4(new Vector3(x, y, z));

                vertexIndex++;
            }
        }

        int triangleIndex = 0;
        for (int i = 0; i < numStacks; i++)
        {
            for (int j = 0; j < numSegments; j++)
            {
                int a = i * (numSegments + 1) + j;
                int b = a + 1;
                int c = (i + 1) * (numSegments + 1) + j;
                int d = c + 1;

                indices[triangleIndex++] = a;
                indices[triangleIndex++] = c;
                indices[triangleIndex++] = b;

                indices[triangleIndex++] = b;
                indices[triangleIndex++] = c;
                indices[triangleIndex++] = d;
            }
        }

        ellipsoidMesh.vertices = vertices;
        ellipsoidMesh.triangles = indices;
        ellipsoidMesh.RecalculateNormals();
    }

    private void OnRenderObject()
    {
        ellipsoidMaterial.SetPass(0);
        Graphics.DrawMeshNow(ellipsoidMesh, Matrix4x4.identity);
    }
}