using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointerController : MonoBehaviour
{
  private Text label;
  private CellController targetCell;
  private LineRenderer lineRenderer;

  void Start() {
    label = transform.Find("canvas/text").GetComponent<Text>();
    lineRenderer = GetComponent<LineRenderer>();
    targetCell = null;
  }

  public string Label {
    get {
      return label.text;
    }

    set {
      label.text = value;
    }
  }

  public CellController Target {
    get {
      return targetCell;
    }

    set {
      targetCell = value;
      if (targetCell == null) {
        Label = "○";
        lineRenderer.enabled = false;
      }
      else {
        Label = "●";
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, (Vector2)transform.position);
        lineRenderer.SetPosition(1, (Vector2)targetCell.transform.position);
      }
    }
  }

}