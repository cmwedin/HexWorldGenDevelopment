using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoords coords;
    public HexGridChunk chunk;

    public Color color { get { return _color; }
                         set { if (_color == value) { return; }
                                   _color = value;
                                   Refresh(); } }

    Color _color;
    
    void OnEnable() {
        Refresh();
    }

    public int Elevation { get { return _elevation; }
                           set { if (_elevation == value) { return; }
                                 _elevation = value;
                                 Vector3 pos = transform.localPosition;
                                 pos.y = value * HexMetrics.elevationStep;
                                 pos.y += (HexMetrics.SampleNoise(pos).y * 2f - 1f) 
                                          * HexMetrics.elevationPerturbStrength;
                                 transform.localPosition = pos; 
        
                                 Vector3 uiPos = uiRect.localPosition;
                                 uiPos.z = _elevation * -HexMetrics.elevationStep;
                                 uiRect.localPosition = uiPos; 
                                 if (hasOutgoingRiver && _elevation < GetNeighbor(outgoingRiver)._elevation)
                                    { RemoveOutgoingRiver(); }
                                 if (hasIncomingRiver && _elevation > GetNeighbor(incomingRiver)._elevation)
                                    { RemoveIncomingRiver(); }
                                 Refresh(); } }
    
    int _elevation = int.MinValue;

    [SerializeField]
    HexCell[] neighbors;
    public RectTransform uiRect;

    public Vector3 pos {
        get {
            return transform.localPosition;
        }
    }

    public HexCell GetNeighbor (HexDirection direction) {
        return neighbors[(int)direction];
    }

    public HexEdgeType GetEdgeType (HexDirection direction) {
        return HexMetrics.GetEdgeType(_elevation, neighbors[(int)direction]._elevation);
    }

    public HexEdgeType GetEdgeType (HexCell otherCell) {
        return HexMetrics.GetEdgeType(_elevation, otherCell._elevation);
    }

    public void SetNeighbor (HexDirection direction, HexCell cell) {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    void Refresh () {
        if (chunk) { chunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++) {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk) { neighbor.chunk.Refresh(); }
            }
        }
    }

    void RefreshSelfOnly () {
        chunk.Refresh();
    }

    //river variabless
    public bool hasRiver { get { return hasIncomingRiver || hasOutgoingRiver; } }
    public bool hasRiverTerminus { get { return hasIncomingRiver != hasOutgoingRiver; } }

    public bool hasRiverThroughEdge(HexDirection direction) { return
           hasIncomingRiver && incomingRiver == direction ||
           hasOutgoingRiver && outgoingRiver == direction; }

    bool _hasIncomingRiver, _hasOutgoingRiver;
    HexDirection _incomingRiver, _outgoingRiver;
    public bool hasIncomingRiver { get { return _hasIncomingRiver; } }
    public bool hasOutgoingRiver { get { return _hasOutgoingRiver; } }
    HexDirection incomingRiver { get { return _incomingRiver; } }
    HexDirection outgoingRiver { get { return _outgoingRiver; } }

    public float StreamBedY { get { return (_elevation + HexMetrics.streamBedElevationOffset) 
                                         * HexMetrics.elevationStep; } }

    //removing rivers
    public void RemoveOutgoingRiver () {
        if (!hasOutgoingRiver) { return; }
        _hasOutgoingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(outgoingRiver);
        neighbor._hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveIncomingRiver()
    {
        if (!hasIncomingRiver) { return; }
        _hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(incomingRiver);
        neighbor._hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveRiver() {
        RemoveIncomingRiver();
        RemoveOutgoingRiver();
    }

    //adding rivers
    public void SetOutgoingRiver (HexDirection direction) {
        if (hasOutgoingRiver && outgoingRiver == direction) { return; }
        HexCell neighbor = GetNeighbor(direction);
        if(!neighbor || _elevation < neighbor._elevation) { return; }
        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiver == direction) { RemoveIncomingRiver(); }

        _hasOutgoingRiver = true;
        _outgoingRiver = direction;
        RefreshSelfOnly();

        neighbor.RemoveIncomingRiver();
        neighbor._hasIncomingRiver = true;
        neighbor._incomingRiver = direction.Opposite();
        neighbor.RefreshSelfOnly();
    }

    //general river functions
}
