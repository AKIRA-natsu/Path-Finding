using System.Linq;
using UnityEngine;

public class MapGridController : MonoBehaviour, IGrid {
    private MeshRenderer render;
    private TextMesh text;
    [SerializeField]
    private GridState state;
    public GridState State {
        get => state;
        set {
            state = value;
            render.material.color = GetGridColor(value);
        }
    }
    public Vector2Int position;

    private readonly static Color NoneColor = Color.white;
    private readonly static Color StartColor = Color.green;
    private readonly static Color EndColor = Color.red;
    private readonly static Color BlockColor = Color.gray;
    private readonly static Color PathColor = Color.cyan;
    private readonly static Color InOpenColor = Color.blue;
    private readonly static Color InCloseColor = Color.magenta;
    private readonly static Color SelectColor = Color.yellow;

    private static Color GetGridColor(GridState type) {
        return type switch {
            GridState.None => NoneColor,
            GridState.Start => StartColor,
            GridState.End => EndColor,
            GridState.Block => BlockColor,
            GridState.Path => PathColor,
            GridState.InOpen => InOpenColor,
            GridState.InClose => InCloseColor,
            _ => NoneColor,
        };
    }
    
    private void Awake() {
        render = this.GetComponent<MeshRenderer>();
        text = new GameObject().AddComponent<TextMesh>();
        text.transform.SetParent(this.transform);
        text.transform.localPosition = Vector3.zero;
        text.transform.localScale = Vector3.one * .2f;
        text.color = Color.black;
        text.anchor = TextAnchor.MiddleCenter;
    }
    
    private void OnMouseEnter() {
        render.material.color = SelectColor;
    }

    private void OnMouseExit() {
        render.material.color = GetGridColor(state);
    }

    private void OnMouseDown() {
        var target = MapController.Blush;
        if (target == GridState.Start || target == GridState.End) {
            var grids = this.transform.parent.GetComponentsInChildren<MapGridController>();
            var grid = grids.SingleOrDefault(grid => grid.State == target);
            if (grid != null) {
                if (grid == this)
                    return;
                grid.State = GridState.None;
            }
        }

        this.state = MapController.Blush;
    }

    public void ShowOrUpdateAStarHint(int g, int h, int f, Vector2 value) {
        if(State == GridState.None || State == GridState.InOpen) {
            State = GridState.InOpen;
            // if(m_isCanShowHint) {
            //     gText.text = $"G:\n{g.ToString()}";
            //     hText.text = $"H:\n{h.ToString()}";
            //     fText.text = $"F:\n{f.ToString()}";
            //     Arrow.SetActive(true);
            //     Arrow.transform.up = -forward;
            // }
            text.text = $" F: {f}\n G: {g}\n H: {h}";
        }
    }

    public void ChangeInOpenStateToInClose() {
        if (State == GridState.InOpen)
            State = GridState.InClose;
    }

    public void ChangeToPathState() {
        if(State == GridState.InOpen || State == GridState.InClose)
            State = GridState.Path;
    }

    public void ClearAStarHint() {
        text.text = "";
        // gText.text = "";
        // hText.text = "";
        // fText.text = "";
        if(State == GridState.InOpen || State == GridState.InClose || State == GridState.Path)
            State = GridState.None;
        // Arrow.SetActive(false);
    }
}