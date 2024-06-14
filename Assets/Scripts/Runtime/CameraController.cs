using UnityEngine;

public class CameraController : MonoBehaviour {
    public new Camera camera;
    public Vector2Int range = new(3, 20);
    public float boundary = 30f;
    public float moveScale = 0.1f;

    private void Awake() {
        camera = this.GetComponent<Camera>();
    }
    
    private void Update() {
        var scroll = Input.mouseScrollDelta;
        if (scroll.y < 0) {
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize + 1, range.x, range.y);
        }

        if (scroll.y > 0) {
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize - 1, range.x, range.y);
        }

        var mousePos = Input.mousePosition;
        Vector3 dir = Vector3.zero;
        if (mousePos.x <= boundary && mousePos.x >= 0)
            dir += Vector3.left;
        if (mousePos.x >= Screen.width - boundary && mousePos.x <= Screen.width)
            dir += Vector3.right;
        if (mousePos.y <= boundary && mousePos.y >= 0)
            dir += Vector3.down;
        if (mousePos.y >= Screen.height - boundary && mousePos.y <= Screen.height)
            dir += Vector3.up;
        this.transform.position += dir * moveScale;
    }
}