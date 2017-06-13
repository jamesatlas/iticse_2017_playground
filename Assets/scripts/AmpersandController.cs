using UnityEngine;
using System.Collections;

public class AmpersandController : PlayerController {
  public float barbLength;

  private LineRenderer lineRenderer;
  private LineRenderer leftBarbRenderer;
  private LineRenderer rightBarbRenderer;
  private CellController targetCell;
  
  private Vector2 targetPosition;
  private Coroutine caster;
  private StarController star;

  override public void Start() {
    base.Start();
    this.Type = "R";
    lineRenderer = GetComponent<LineRenderer>();
    leftBarbRenderer = transform.Find("leftbarb").GetComponent<LineRenderer>();
    rightBarbRenderer = transform.Find("rightbarb").GetComponent<LineRenderer>();
    star = GameObject.Find("/players/star").GetComponent<StarController>();
    targetCell = null;
    caster = null;
  }
  
  override public void Update() {
    base.Update();

    // Emit feeler pointer on left-click.
    if (Input.GetMouseButtonDown(0)) {
      if (caster != null) {
        StopCoroutine(caster);
      }
      caster = StartCoroutine(CastPointer()); 
    }

    // Cancel pointer on right-click.
    if (Input.GetMouseButtonDown(1)) {
      Depoint();  
    }

    // Only update pointer if we're not currently sending out a feeler ray.
    if (IsPointerAttached()) {
      Vector2 diff = targetPosition - (Vector2) transform.position;
      diff.Normalize();
      RaycastHit2D hit = Physics2D.Raycast(transform.position, diff, Mathf.Infinity, Utilities.GROUND_MASK);

      // If our ray hits a different object than it did before, that means some
      // other object got in the way.
      if (hit.collider.gameObject != targetCell.gameObject &&
        (targetCell.pointer == null || hit.collider.gameObject != targetCell.pointer.gameObject)) {
        Depoint();
      } else {
        Vector2 perp = new Vector3(-diff.y, diff.x);
        lineRenderer.SetPosition(0, (Vector2)transform.position + diff * 0.3f);
        leftBarbRenderer.SetPosition(0, targetPosition + barbLength * (perp - 1.5f * diff));
        rightBarbRenderer.SetPosition(0, targetPosition - barbLength * (perp + 1.5f * diff));


      }
    }

    if (Input.GetButtonDown("Attach")) {
      GameObject p = GetOnPointer();
      if (p == null && IsConnectedToOther()) {
        p = star.GetOnPointer();
      }
      if (p != null) {
        PointerController sourcePointer = p.GetComponent<PointerController>();
        sourcePointer.Target = targetCell;
        if (IsPointerAttached()) {
          Depoint();
        }
      }
    }

  }

  void Depoint() {
    lineRenderer.enabled = false;
    leftBarbRenderer.enabled = false;
    rightBarbRenderer.enabled = false;
    targetCell = null;
  }

  void PointAt(Vector2 position) {
    Vector2 diff = position - (Vector2) transform.position;
    diff.Normalize();
    Vector2 perp = new Vector3(-diff.y, diff.x);
    lineRenderer.SetPosition(0, (Vector2) transform.position + diff * 0.3f); 
    leftBarbRenderer.SetPosition(0, position + barbLength * (perp - 1.5f * diff)); 
    rightBarbRenderer.SetPosition(0, position - barbLength * (perp + 1.5f * diff)); 
    lineRenderer.SetPosition(1, position); 
    leftBarbRenderer.SetPosition(1, position); 
    rightBarbRenderer.SetPosition(1, position); 
  }

  IEnumerator CastPointer() {
    lineRenderer.enabled = true;
    leftBarbRenderer.enabled = true;
    rightBarbRenderer.enabled = true;

    Vector3 mousePixels = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
    Vector2 to = Camera.main.ScreenToWorldPoint(mousePixels);
    Vector2 from = transform.position;
    float maximumLength = Vector2.Distance(from, to);
    Vector2 diff = to - from;
    diff.Normalize();

    float startTime = Time.time;
    float elapsedTime = 0.0f;
    float targetTime = 0.5f;
    bool isHit = false;
    targetCell = null;

    RaycastHit2D hit;
    while (elapsedTime < targetTime && !isHit) {
      float proportion = elapsedTime / targetTime;
      float length = proportion * maximumLength;
      hit = Physics2D.Raycast(from, diff, length, Utilities.GROUND_MASK);
      if (hit.collider != null) {
        PointAt(hit.point);
        GameObject hitObject = hit.collider.gameObject;
        if (hitObject.tag == "pointer") {
          Transform hitParent = hitObject.transform.parent;
          // is it part of a linked cell?
          if (hitParent != null && hitParent.gameObject.tag == "linkedCell") {
            targetCell = hitParent.GetComponentInChildren<CellController>();
          }
        }
        else if (hitObject.tag == "cell") {
          targetCell = hitObject.GetComponent<CellController>();
        }
       
        targetPosition = hit.point;
        isHit = true;
      } else {
        PointAt(from + diff * length); 
      }
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    lineRenderer.enabled = targetCell != null;
    leftBarbRenderer.enabled = targetCell != null;
    rightBarbRenderer.enabled = targetCell != null;
    caster = null;
  }

  public CellController Target {
    get {
      return targetCell;
    }
  }

  bool IsPointerAttached() {
    return caster == null && lineRenderer.enabled;
  }

  override public bool IsTransmittable() {
    return IsPointerAttached();
  }

  override public IEnumerator Transmit() {
    GameObject cell = targetCell.gameObject.transform.Find("canvas/text").gameObject;
    GameObject payload = Instantiate(cell);
    payload.transform.SetParent(targetCell.gameObject.transform.Find("canvas"));
    payload.transform.position = cell.transform.position;

    Vector2 startPosition = payload.transform.position;
    Vector2 endPosition = star.gameObject.transform.position;

    float startTime = Time.time;
    float targetTime = 1.0f;
    float elapsedTime = 0.0f;

    while (elapsedTime <= targetTime) {
      payload.transform.position = Vector2.Lerp(startPosition, endPosition, elapsedTime / targetTime);
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    star.Acquire(targetCell.Label);
    Destroy(payload);
  }
}
