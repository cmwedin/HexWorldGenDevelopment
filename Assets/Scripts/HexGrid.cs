using UnityEngine;
using UnityEngine.UI;


public class HexGrid : MonoBehaviour
{
    [SerializeField]
    bool showLabel;
    
    int cellCountX = 6;
    int cellCountZ = 6;
    public int chunkCountX = 4, chunkCountZ = 3;

    public Color defaultColor = Color.white;

    public HexGridChunk chunkPrefab;
    public HexCell cellPrefab;
    public Text cellLabelPrefab;
    public Texture2D noiseSource;

    //Canvas gridCanvas;
    //HexMesh hexMesh;
    HexCell[] cells;
    HexGridChunk[] chunks;

    void Awake()
    {
        HexMetrics.noiseSource = noiseSource;

        //gridCanvas = GetComponentInChildren<Canvas>();
        //hexMesh = GetComponentInChildren<HexMesh>();

        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

        CreateChunks();
        CreateCells();
    }

    void CreateChunks() {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for (int z = 0, i=0; z < chunkCountZ; z++) {
            for (int x = 0; x < chunkCountX; x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
            }
        }

    }

    void CreateCells () {
        cells = new HexCell[cellCountZ * cellCountX];
        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    void OnEnable () {
        UnityEngine.Debug.Log("Grid enabled");
        HexMetrics.noiseSource = noiseSource;
        //UnityEngine.Debug.Log("Redrawing Mesh");
        //Refresh();
    }
    
   /* void Start ()
    {
        hexMesh.Triangulate(cells);
    } */

    public HexCell GetCell (Vector3 pos) {
        pos = transform.InverseTransformPoint(pos);
        HexCoords coords = HexCoords.FromPosition(pos);
        int index = coords.X + coords.Z * cellCountX + coords.Z / 2;
        UnityEngine.Debug.Log("got Cell" + coords.ToString());
        return cells[index];
    }

    void CreateCell (int x, int z, int i)
    {
        Vector3 pos;
        pos.x = (x + z*.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        pos.y = 0f;
        pos.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        //cell.transform.SetParent(transform, false);
        cell.transform.localPosition = pos;
        cell.coords = HexCoords.FromOffsetCoords(x, z);
        cell.color = defaultColor;

        if (x > 0) { cell.SetNeighbor(HexDirection.W, cells[i - 1]); }
        if (z > 0) {
            if ((z & 1) == 0) { 
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]); 
                if (x > 0) {
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            } else {
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if (x < cellCountX -1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                }
            }
        }

        Text label = Instantiate<Text>(cellLabelPrefab);
        //label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition =
            new Vector2(pos.x, pos.z);
        if (showLabel) { label.text = cell.coords.ToStringSepLines(); }
        else { label.text = null; }
        cell.uiRect = label.rectTransform;

        AddCellToChunk(x, z, cell);
    }

    public void AddCellToChunk(int x,int z, HexCell cell) {
        int chunkX = x / HexMetrics.chunkSizeX;
        int chunkZ = z / HexMetrics.chunkSizeX;
        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = x - chunkX * HexMetrics.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }

    public HexCell GetCell (HexCoords Coords) {
        int z = Coords.Z;
        if (z < 0 || z >= cellCountZ) { return null; }
        int x = Coords.X + z / 2;
        if (x < 0 || x >= cellCountX) { return null; }
        return cells[x + z * cellCountX];
    }
}
