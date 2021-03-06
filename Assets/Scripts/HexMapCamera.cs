using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class HexMapCamera : MonoBehaviour
{
    Transform swivel, stick;
    float zoom = 1f;

    public float stickMinZoom, stickMaxZoom;
    public float swivelMinZoom, swivelMaxZoom;
    public float moveSpeedMinZoom, moveSpeedMaxZoom;
    public float rotationSpeed;

    public HexGrid grid;


    void Awake()
    {
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Zoom
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f) { AdjustZoom(zoomDelta); }

        //Position
        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f) { AdjustPosition(xDelta, zDelta); }

        //rotation
        float rotationDelta = Input.GetAxis("Rotation");
        if (rotationDelta != 0) { AdjustRotation(rotationDelta); }
    }

    void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    void AdjustPosition(float xDelta, float zDelta)
    {

        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;

        Vector3 pos = transform.localPosition;
        pos += direction * distance;
        transform.localPosition = ClampPosition(pos);
    }

    Vector3 ClampPosition(Vector3 pos)
    {
        float xMax = (grid.chunkCountX * HexMetrics.chunkSizeX - 0.5f) * (2f * HexMetrics.innerRadius);
        pos.x = Mathf.Clamp(pos.x, 0f, xMax);
        float zMax = (grid.chunkCountZ * HexMetrics.chunkSizeZ - 1f)* (2f * HexMetrics.outerRadius);
        pos.z = Mathf.Clamp(pos.z, 0f, xMax);

        return pos;
    }

    float rotationAngle;
    void AdjustRotation (float delta) {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0f) { rotationAngle += 360f; }
        else if (rotationAngle >= 360f) { rotationAngle -= 360f; }        
        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }
}
