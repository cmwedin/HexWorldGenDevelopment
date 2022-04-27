using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    Mesh hexMesh;
    static List<Vector3> vertices = new List<Vector3>();
    static List<int> triangles = new List<int>();
    static List<Color> colors = new List<Color>();
    MeshCollider meshCollider;

    public struct EdgeVertices
    {
        public Vector3 v1, v2, v3, v4, v5;

        public EdgeVertices(Vector3 corner1, Vector3 corner2)
        {
            v1 = corner1;
            v2 = Vector3.Lerp(corner1, corner2, .25f);
            v3 = Vector3.Lerp(corner1, corner2, .5f);
            v4 = Vector3.Lerp(corner1, corner2, .75f);
            v5 = corner2;
        }

        public EdgeVertices(Vector3 corner1, Vector3 corner2, float outerStep)
        {
            v1 = corner1;
            v2 = Vector3.Lerp(corner1, corner2, outerStep);
            v3 = Vector3.Lerp(corner1, corner2, .5f);
            v4 = Vector3.Lerp(corner1, corner2, 1f - outerStep);
            v5 = corner2;
        }

        public static EdgeVertices TerraceLerp(EdgeVertices a, EdgeVertices b, int step)
        {
            EdgeVertices result;
            result.v1 = HexMetrics.TerraceLerp(a.v1, b.v1, step);
            result.v2 = HexMetrics.TerraceLerp(a.v2, b.v2, step);
            result.v3 = HexMetrics.TerraceLerp(a.v3, b.v3, step);
            result.v4 = HexMetrics.TerraceLerp(a.v4, b.v4, step);
            result.v5 = HexMetrics.TerraceLerp(a.v5, b.v5, step);
            return result;
        }
    }



    void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        hexMesh.name = "Hex Mesh";
    }

    public void Triangulate(HexCell[] cells)
    {
        hexMesh.Clear();
        vertices.Clear();
        colors.Clear();
        triangles.Clear();
        for (int i = 0; i < cells.Length; i++)
        { Triangulate(cells[i]); }
        UnityEngine.Debug.Log("colors has length" + colors.Count);
        UnityEngine.Debug.Log("vertices has length" + vertices.Count);
        UnityEngine.Debug.Log("cells has length" + cells.Length);

        hexMesh.vertices = vertices.ToArray();
        hexMesh.colors = colors.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexMesh;
    }

    void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.pos;
        EdgeVertices e = new EdgeVertices(
            center + HexMetrics.GetFirstSolidCorner(direction),
            center + HexMetrics.GetSecondSolidCorner(direction)
        );
        if (cell.hasRiver)
        {
            if (cell.hasRiverThroughEdge(direction)) {
                e.v3.y = cell.StreamBedY;
                if (cell.hasRiverTerminus) { TriangulateRiverTerminus(direction, cell, center, e); }
                else { TriangulateWithRiver(direction, cell, center, e); }
            }
            
        } else { TriangulateEdgeFan(center, e, cell.color); }

        if (direction <= HexDirection.SE) { TriangulateConnection(direction, cell, e); }
    }

    void TriangulateWithRiver(
        HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e
    ) {
        Vector3 centerL = center + HexMetrics.GetFirstSolidCorner(direction.Previous()) * 0.25f;
        Vector3 centerR = center + HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;

        EdgeVertices m = new EdgeVertices(Vector3.Lerp(centerL, e.v1, 0.5f),
                                            Vector3.Lerp(centerR, e.v5, 0.5f),
                                            1f / 6f);
        m.v3.y = center.y = e.v3.y;
        TriangulateEdgeStrip(m, cell.color, e, cell.color);

        AddTriangle(centerL, m.v1, m.v2);
        AddTriangleColor(cell.color);
        AddQuad(centerL, center, m.v2, m.v3);
        AddQuadColor(cell.color, cell.color);
        AddQuad(center, centerR, m.v3, m.v4);
        AddQuadColor(cell.color, cell.color);
        AddTriangle(centerR, m.v4, m.v5);
        AddTriangleColor(cell.color);
    }

    void TriangulateRiverTerminus(
            HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e
        )
    {
        EdgeVertices m = new EdgeVertices(Vector3.Lerp(center, e.v1, 0.5f),
                                            Vector3.Lerp(center, e.v1, 0.5f));
        m.v3.y = e.v3.y;

        TriangulateEdgeStrip(m, cell.color, e, cell.color);
        TriangulateEdgeFan(center, m, cell.color);
    }

    void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null) { return; }

        Vector3 bridge = HexMetrics.GetBridge(direction);
        bridge.y = neighbor.pos.y - cell.pos.y;
        EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge,
                                            e1.v5 + bridge);

        if (cell.hasRiverThroughEdge(direction)) { e2.v3.y = neighbor.StreamBedY; }

        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
        {
            TriangulateEdgeTerraces(e1, cell, e2, neighbor);
        }
        else
        {
            TriangulateEdgeStrip(e1, cell.color, e2, neighbor.color);
        }

        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if (direction <= HexDirection.E && nextNeighbor != null)
        {
            Vector3 v5 = e1.v5 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.pos.y;

            //find bottom cell
            if (cell.Elevation <= neighbor.Elevation)
            {
                if (cell.Elevation <= nextNeighbor.Elevation)
                {
                    TriangulateCorner(e1.v5, cell, e2.v5, neighbor, v5, nextNeighbor);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
                }
            }
            else if (neighbor.Elevation <= nextNeighbor.Elevation)
            {
                TriangulateCorner(e2.v5, neighbor, v5, nextNeighbor, e1.v5, cell);
            }
            else
            {
                TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
            }
        }
    }

    void TriangulateEdgeTerraces(
        EdgeVertices begin, HexCell beginCell,
        EdgeVertices end, HexCell endCell
    ) {
        EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1);

        TriangulateEdgeStrip(begin, beginCell.color, e2, c2);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {

            EdgeVertices e1 = e2;
            Color c1 = c2;
            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);
            TriangulateEdgeStrip(e1, c1, e2, c2);

        }

        TriangulateEdgeStrip(e2, c2, end, endCell.color);
    }

    void TriangulateCorner(
        Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    ) {
        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);


        if (leftEdgeType == HexEdgeType.Slope)
        {
            if (rightEdgeType == HexEdgeType.Slope)
            {
                //ssf
                TriangulateCornerTerraces(bottom, bottomCell,
                                            left, leftCell,
                                            right, rightCell);
            }
            else if (rightEdgeType == HexEdgeType.Flat)
            {
                //ssf rotation
                TriangulateCornerTerraces(left, leftCell,
                                            right, rightCell,
                                            bottom, bottomCell);
            }
            else
            {
                //sc?
                TriangulateCornerTerracesCliff(bottom, bottomCell,
                                                left, leftCell,
                                                right, rightCell);
            }
            return;
        }
        if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
            {
                //ssf rotation
                TriangulateCornerTerraces(right, rightCell,
                                            bottom, bottomCell,
                                            left, leftCell);
            }
            else
            {
                //sc?    
                TriangulateCornerCliffTerraces(
                        bottom, bottomCell,
                        left, leftCell,
                        right, rightCell);
            }
            return;
        }
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            if (leftCell.Elevation < rightCell.Elevation)
            {
                TriangulateCornerCliffTerraces(right, rightCell,
                                                bottom, bottomCell,
                                                left, leftCell);
            }
            else
            {
                TriangulateCornerTerracesCliff(left, leftCell,
                                            right, rightCell,
                                            bottom, bottomCell);
            }
            return;

        }
        else
        {
            AddTriangle(bottom, left, right);
            AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
        }


    }

    void TriangulateCornerTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    ) {
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
        Color c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);
        Color c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, 1);

        AddTriangle(begin, v3, v4);
        AddTriangleColor(beginCell.color, c3, c4);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);

        }

        AddQuad(v3, v4, left, right);
        AddQuadColor(c3, c4, leftCell.color, rightCell.color);

    }

    void TriangulateCornerTerracesCliff(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    ) {
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        if (b < 0) { b = -b; }
        Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(right), b);
        Color boundaryColor = Color.Lerp(beginCell.color, rightCell.color, b);

        TriangulateBoundaryTriangle(begin, beginCell,
                                    left, leftCell,
                                    boundary, boundaryColor);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell,
                                        right, rightCell,
                                        boundary, boundaryColor);
        }
        else
        {
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }

    void TriangulateCornerCliffTerraces(
    Vector3 begin, HexCell beginCell,
    Vector3 left, HexCell leftCell,
    Vector3 right, HexCell rightCell
    ) {
        float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        if (b < 0) { b = -b; }
        Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(left), b);
        Color boundaryColor = Color.Lerp(beginCell.color, leftCell.color, b);

        TriangulateBoundaryTriangle(right, rightCell,
                                    begin, beginCell,
                                    boundary, boundaryColor);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell,
                                        right, rightCell,
                                        boundary, boundaryColor);
        }
        else
        {
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }

    void TriangulateBoundaryTriangle(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 boundary, Color boundaryColor
    )
    {
        Vector3 v2 = Perturb(HexMetrics.TerraceLerp(begin, left, 1));
        Color c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);

        AddTriangleUnperturbed(Perturb(begin), v2, boundary);
        AddTriangleColor(beginCell.color, c2, boundaryColor);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = Perturb(HexMetrics.TerraceLerp(begin, left, i));
            c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            AddTriangleUnperturbed(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
        }

        AddTriangleUnperturbed(v2, Perturb(left), boundary);
        AddTriangleColor(c2, leftCell.color, boundaryColor);
    }

    void AddTriangleColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }
    void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }

    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertIndex = vertices.Count;

        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));

        triangles.Add(vertIndex);
        triangles.Add(vertIndex + 1);
        triangles.Add(vertIndex + 2);
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertIndex = vertices.Count;

        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));
        vertices.Add(Perturb(v4));

        triangles.Add(vertIndex);
        triangles.Add(vertIndex + 2);
        triangles.Add(vertIndex + 1);

        triangles.Add(vertIndex + 1);
        triangles.Add(vertIndex + 2);
        triangles.Add(vertIndex + 3);
    }

    void AddQuadColor(Color c1, Color c2)
    {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);

    }

    void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }

    Vector3 Perturb(Vector3 pos)
    {
        Vector4 sample = HexMetrics.SampleNoise(pos);
        pos.x += (sample.x * 2f - 1f) * HexMetrics.cellPerturbStrength;
        //pos.y += (sample.y * 2f - 1f) * HexMetrics.cellPerturbStrength;
        pos.z += (sample.z * 2f - 1f) * HexMetrics.cellPerturbStrength;
        return pos;
    }

    void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color)
    {
        AddTriangle(center, edge.v1, edge.v2);
        AddTriangleColor(color);
        AddTriangle(center, edge.v2, edge.v3);
        AddTriangleColor(color);
        AddTriangle(center, edge.v3, edge.v4);
        AddTriangleColor(color);
        AddTriangle(center, edge.v4, edge.v5);
        AddTriangleColor(color);
    }

    void TriangulateEdgeStrip(
        EdgeVertices e1, Color c1,
        EdgeVertices e2, Color c2
    ) {
        AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        AddQuadColor(c1, c2);
        AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        AddQuadColor(c1, c2);
        AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
        AddQuadColor(c1, c2);
        AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
        AddQuadColor(c1, c2);
    }

    void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertIndex = vertices.Count;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(vertIndex);
        triangles.Add(vertIndex + 1);
        triangles.Add(vertIndex + 2);

    }
}
