using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StarController : PlayerController {
  private AmpersandController ampersand;
  private Text loot;

  override public void Start() {
    base.Start();
    this.Type = "D";
    loot = transform.Find("canvas/loot").GetComponent<Text>();
    ampersand = GameObject.Find("/players/ampersand").GetComponent<AmpersandController>();
  }

  public void Acquire(string label) {
    loot.text = label;
  }

  override public void Update() {
    base.Update();

    if (Input.GetButtonDown("Dereference")) {
      // check to see if star or ampersand are on same pointer
      GameObject on = GetOnPointer();
      bool connected = IsConnectedToOther();
      if (on == null && connected) {
        on = ampersand.GetOnPointer(); // if ampersand is on a pointer and they are close
      }
      
      if (on != null) {
        PointerController pointer = on.GetComponent<PointerController>();
        if (pointer.Target != null) {
          // teleport the star and ampersand (if connected) on top of the target cell
          CellController cc = pointer.Target;
          Vector2 pos = (Vector2)cc.transform.position;
          BoxCollider2D b = cc.transform.GetComponent<BoxCollider2D>();
          float sizeStar = GetComponent<CircleCollider2D>().radius;
          float sizeAmp = ampersand.GetComponent<CircleCollider2D>().radius;

          gameObject.transform.position = new Vector2(pos.x - sizeStar, pos.y + b.bounds.size.y / 2 + sizeStar);
          if (connected) {
            ampersand.gameObject.transform.position = new Vector2(pos.x, pos.y + b.bounds.size.y / 2 + sizeAmp);
          }
        }
      }
    }
  }

  override public bool IsTransmittable() {
    return loot.text != "";
  }

  override public IEnumerator Transmit() {
    GameObject payload = Instantiate(loot.gameObject);
    payload.transform.SetParent(ampersand.Target.gameObject.transform.Find("canvas"));
    payload.transform.position = ampersand.gameObject.transform.position;

    Vector2 startPosition = ampersand.gameObject.transform.position;
    Vector2 endPosition = ampersand.Target.gameObject.transform.position;

    float startTime = Time.time;
    float targetTime = 1.0f;
    float elapsedTime = 0.0f;

    while (elapsedTime <= targetTime) {
      payload.transform.position = Vector2.Lerp(startPosition, endPosition, elapsedTime / targetTime);
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    Destroy(payload);
    ampersand.Target.Label = loot.text;
  }

  public Vector2 LootPosition {
    get {
      return loot.gameObject.transform.position;
    }
  }
}
