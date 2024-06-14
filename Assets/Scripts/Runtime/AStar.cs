using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EvaluationFunctionType {
    Euclidean,
    Manhattan,
    Diagonal,
}

public class Node {
    public Vector2Int Position { get; private set; }
    public Node parent;

    // 开始到该节点的估价距离
    private int m_g;
    public int G {
        get => m_g;
        set {
            m_g = value;
            m_f = m_g + m_h;
        }
    }

    // 该节点到终点的估价距离
    private int m_h;
    public int H {
        get => m_h;
        set {
            m_h = value;
            m_f = m_g + m_h;
        }
    }

    // 总体估价距离
    private int m_f;
    public int F => m_f;

    public Node(Vector2Int position, Node parent, int g, int h) {
        this.Position = position;
        this.parent = parent;
        m_g = g;
        m_h = h;
        m_f = g + h;
    }
}


public enum GridState {
    None,
    Start,
    End,
    Block,
    Path,
    InOpen,
    InClose,
}

public interface IGrid {
    GridState State { get; }
    void ShowOrUpdateAStarHint(int g, int h, int f, Vector2 value);
    void ChangeInOpenStateToInClose();
    void ChangeToPathState();
    void ClearAStarHint();
}

public class AStar {
    private static int FACTOR = 10;                 // 直线相邻距离
    private static int FACTOR_DIAGONAL = 14;        // 对角相邻距离

    public bool IsInit { get; private set; }

    private IGrid[,] map;
    private Vector2Int mapsize;
    private Vector2Int start, end;
    private EvaluationFunctionType evaluationFunctionType;      // 估价方式

    private Dictionary<Vector2Int, Node> openDic = new();
    private Dictionary<Vector2Int, Node> closeDic = new();

    private Node endNode;

    public void Init(IGrid[,] map, Vector2Int size, Vector2Int start, Vector2Int end, EvaluationFunctionType type = EvaluationFunctionType.Diagonal) {
        this.map = map;
        this.mapsize = size;
        this.start = start;
        this.end = end;
        this.evaluationFunctionType = type;

        openDic.Clear();
        closeDic.Clear();

        endNode = null;
        AddNodeInOpenQueue(new Node(start, null, 0, 0));
        IsInit = true;
    }

    /// <summary>
    /// 计算寻路
    /// </summary>
    /// <returns></returns>
    public IEnumerator Start() {
        while (openDic.Count > 0 && endNode == null) {
            // 按照f升序，f相等按h升序
            openDic = openDic.OrderBy(kv => kv.Value.F).ThenBy(kv => kv.Value.H).ToDictionary(p => p.Key, o => o.Value);
            // 拿第一个节点
            Node node = openDic.First().Value;
            //处理该节点相邻的节点
            OperateNeighborNode(node);
            // 删除节点
            openDic.Remove(node.Position);
            // 处理完加入close
            AddNodeInCloseDic(node);
            yield return null;
        }

        if (endNode == null) {
            Debug.LogError("找不到可用路径");
        } else {
            ShowPath(endNode);
        }
    }

    private void OperateNeighborNode(Node node) {
        for (int i = -1; i < 2; i++) {
            for (int j = -1; j < 2; j++) {
                if (i == 0 && j == 0)
                    continue;
                Vector2Int pos = new(node.Position.x + i, node.Position.y + j);
                // 超过地图
                if (pos.x < 0 || pos.x >= mapsize.x || pos.y < 0 || pos.y >= mapsize.y)
                    continue;
                // 已经处理过
                if (closeDic.ContainsKey(pos))
                    continue;
                if (map[pos.x, pos.y].State == GridState.Block)
                    continue;
                // 相邻加入open中
                if (i == 0 || j == 0)
                    AddNeighborNodeInQueue(node, pos, FACTOR);
                else
                    AddNeighborNodeInQueue(node, pos, FACTOR_DIAGONAL);
            }
        }
    }

    /// <summary>
    /// 讲节点加入open
    /// </summary>
    /// <param name="parentNode"></param>
    /// <param name="pos"></param>
    /// <param name="g"></param>
    private void AddNeighborNodeInQueue(Node parentNode, Vector2Int position, int g) {
        // 当前节点的实际距离g等于上个节点的实际距离加上自己到上个节点的实际距离
        int nodeG = parentNode.G + g;
        //如果该位置的节点已经在open中
        if(openDic.ContainsKey(position)) {
            //比较实际距离g的值，用更小的值替换
            if(nodeG < openDic[position].G) {
                openDic[position].G = nodeG;
                openDic[position].parent = parentNode;
                ShowOrUpdateAStarHint(openDic[position]);
            }
        }
        else {
            //生成新的节点并加入到open中
            Node node = new Node(position, parentNode, nodeG, GetH(position));
            //如果周边有一个是终点，那么说明已经找到了。
            if(position == end)
                endNode = node;
            else
                AddNodeInOpenQueue(node);
        }
    }

    /// <summary>
    /// 加入open中，更新网格状态
    /// </summary>
    /// <param name="node"></param>
    private void AddNodeInOpenQueue(Node node) {
        openDic[node.Position] = node;
        ShowOrUpdateAStarHint(node);
    }

    /// <summary>
    /// 加入close中，并更新网格状态
    /// </summary>
    /// <param name="node"></param>
    private void AddNodeInCloseDic(Node node) {
        closeDic.Add(node.Position, node);
        map[node.Position.x, node.Position.y].ChangeInOpenStateToInClose();
    }

    //寻路完成，显示路径
    private void ShowPath(Node node) {
        while(node != null) {
            map[node.Position.x, node.Position.y].ChangeToPathState();
            node = node.parent;
        }
    }

    private void ShowOrUpdateAStarHint(Node node) {
        map[node.Position.x, node.Position.y].ShowOrUpdateAStarHint(node.G, node.H, node.F,
            node.parent == null ? Vector2.zero : new Vector2(node.parent.Position.x - node.Position.x, node.parent.Position.y - node.Position.y));
    }
    
    //获取估价距离
    int GetH(Vector2Int position) {
        if(evaluationFunctionType == EvaluationFunctionType.Manhattan)
            return GetManhattanDistance(position);
        else if(evaluationFunctionType == EvaluationFunctionType.Diagonal)
            return GetDiagonalDistance(position);
        else
            return Mathf.CeilToInt(GetEuclideanDistance(position));
    }

    //获取曼哈顿距离
    int GetDiagonalDistance(Vector2Int position) {
        int x = Mathf.Abs(end.x - position.x);
        int y = Mathf.Abs(end.y - position.y);
        int min = Mathf.Min(x, y);
        return min * FACTOR_DIAGONAL + Mathf.Abs(x - y) * FACTOR;
    }

    //获取对角线距离
    int GetManhattanDistance(Vector2Int position) {
        return Mathf.Abs(end.x - position.x) * FACTOR + Mathf.Abs(end.y - position.y) * FACTOR;
    }

    //获取欧几里得距离,测试下来并不合适
    float GetEuclideanDistance(Vector2Int position) {
        return Mathf.Sqrt(Mathf.Pow((end.x - position.x) * FACTOR, 2) + Mathf.Pow((end.y - position.y) * FACTOR, 2));
    }

    public void Clear() {
        foreach(var pos in openDic.Keys) {
            map[pos.x, pos.y].ClearAStarHint();
        }
        openDic.Clear();

        foreach(var pos in closeDic.Keys) {
            map[pos.x, pos.y].ClearAStarHint();
        }
        closeDic.Clear();

        endNode = null;

        IsInit = false;
    }
}