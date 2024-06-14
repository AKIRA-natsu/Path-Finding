using System.Collections;
using System.Linq;
using EasyButtons;
using UnityEngine;

public class MapController : MonoBehaviour {
    public Vector2Int size;
    [Min(0)]
    public float interval = .2f;

    public EvaluationFunctionType evaluationFunctionType;
    public static GridState Blush;

    private AStar aStar = new();
    private IEnumerator aStarProcess;

    public bool isStepOneByOne;

    [Button]
    public void UpdateMap() {
        var childcount = this.transform.childCount;
        for (int i = 0; i < childcount; i++) {
            if (Application.isPlaying)
                GameObject.Destroy(this.transform.GetChild(0).gameObject);
            else
                GameObject.DestroyImmediate(this.transform.GetChild(0).gameObject);
        }

        var width = 1f + interval;
        var startX = -(size.x - 1) / 2f * width;
        var startY = -(size.y - 1) / 2f * width;
        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                quad.SetParent(this.transform);
                quad.localPosition = new(startX + i * width, startY + j * width);
                quad.gameObject.AddComponent<MapGridController>().position = new(i, j);
            }
        }
    }
    
    public void SetBlushType(int value) {
        Blush = (GridState)value;
    }

    public void Clear() {
        var grids = this.GetComponentsInChildren<MapGridController>();
        foreach (var grid in grids) {
            grid.State = GridState.None;
        }
        aStar.Clear();
    }

    public void PathFinding() {
        if(!aStar.IsInit) {
            
            IGrid[,] map = new IGrid[size.x, size.y];
            var grids = this.GetComponentsInChildren<MapGridController>();
            foreach (var grid in grids)
                map[grid.position.x, grid.position.y] = grid;

            var start = grids.SingleOrDefault(grid => grid.State == GridState.Start);
            var end = grids.SingleOrDefault(gird => gird.State == GridState.End);

            if (start == null || end == null) {
                Debug.LogError("没有设置起始位置或终点位置");
                return;
            }

            aStar.Init(map, size, start.position, end.position, evaluationFunctionType);
            aStarProcess = aStar.Start();
        }
        if(isStepOneByOne) {
            if(!aStarProcess.MoveNext()) {
                Debug.Log("寻路完成");
            }
        }
        else {
            while(aStarProcess.MoveNext())
                ;
            Debug.Log("寻路完成");
        }
    }
}

