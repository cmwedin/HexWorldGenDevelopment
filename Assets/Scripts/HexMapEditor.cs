using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    public Color[] colors;
    private Color activeColor;
    private int activeElevation;
    int brushSize;

    public HexGrid hexGrid;

    bool applyColor;
    bool applyElevation;
    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;

    enum OptionalToggle { Ignore, Yes, No }
    OptionalToggle riverMode;


    void Awake () {
        SelectColor(0);
    }

    void Update()
    {
        if (   Input.GetMouseButton(0) 
            && !EventSystem.current.IsPointerOverGameObject()) 
            { HandleInput(); }
        else { previousCell = null; }
    }

    void HandleInput() {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit)) {
            HexCell currentCell = hexGrid.GetCell(hit.point);
            if (previousCell && previousCell != currentCell) { ValidateDrag(currentCell); }
            else { isDrag = false; }
            EditCells(currentCell);
            previousCell = currentCell;
        } else { previousCell = null; }
    }

    void ValidateDrag(HexCell currentCell) {
        for (dragDirection = HexDirection.NE;
             dragDirection <= HexDirection.NW;
             dragDirection++
        ) {
            if (previousCell.GetNeighbor(dragDirection) == currentCell) {
                isDrag = true;
                return;
            }

        }
        isDrag = false;
    }

    void EditCells ( HexCell center) {
        int centerX = center.coords.X;
        int centerZ = center.coords.Z;

        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++,  r++) {
            for (int x = centerX - r; x <= centerX + brushSize; x++) {
                EditCell(hexGrid.GetCell(new HexCoords(x, z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
            for (int x = centerX - brushSize; x <= centerX + r; x++) {
                EditCell(hexGrid.GetCell(new HexCoords(x, z)));
            }
        }
    }

    void EditCell (HexCell cell) {
        if (cell)
        {
            if (applyColor) { cell.color = activeColor; }
            if (applyElevation) cell.Elevation = activeElevation;
            if (riverMode == OptionalToggle.No) { cell.RemoveRiver(); }
            else if (isDrag && riverMode == OptionalToggle.Yes) {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell) otherCell.SetOutgoingRiver(dragDirection); }
        }
    }

    public void SelectColor (int index) {
        applyColor = index >= 0;
        if (applyColor) { activeColor = colors[index]; }
        //UnityEngine.Debug.Log("active color is " + activeColor.ToString());
    }

        public void SetApplyColor (bool toggle) {
        applyColor = toggle;
    }

    public void SetApplyElevation (bool toggle) {
        applyElevation = toggle;
    }

    public void SetElevation (float elevation) {
        activeElevation = (int)elevation;
    }

    public void SetBrushSize (float size) {
        brushSize = (int)size;
    }

    public void SetRiverMove (int mode) {
        riverMode = (OptionalToggle)mode;
    }
}
