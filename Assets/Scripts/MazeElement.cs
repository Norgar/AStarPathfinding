using System;
using UnityEngine;

public class MazeElement : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _background;
    [SerializeField] private SpriteRenderer _mark;
    [SerializeField] private Sprite _markPath;
    [SerializeField] private Sprite _markStart;
    [SerializeField] private Sprite _markFinish;
    [SerializeField] private Sprite _none;
    [SerializeField] private Sprite _wall;

    public event Action<RestrictionType, int, int> Edit;
    public event Action<MarkType, int, int> Mark;

    private MarkType _markType;
    private RestrictionType _restrictionType;

    private Node _node;
    private int _x, _y;
    private bool _setStart, _setEnd;

    public void Set(RestrictionType restrictionType, int x, int y, bool edit = false)
    {
        _x = x;
        _y = y;

        _markType = MarkType.None;
        _restrictionType = restrictionType;

        _mark.sprite = null;
        _background.sprite = restrictionType == RestrictionType.Wall
            ? _wall
            : _none;

        if (edit)
            Edit?.Invoke(restrictionType, x, y);
    }

    public void SetMark(MarkType markType, bool invokeAction = true)
    {
        if (_restrictionType != RestrictionType.None)
            return;

        _markType = markType;

        switch (markType)
        {
            case MarkType.Path:
                _mark.sprite = _markPath;
                break;
            case MarkType.Open:
            case MarkType.Start:
                _mark.sprite = _markStart;
                break;
            case MarkType.Finish:
            case MarkType.Closed:
                _mark.sprite = _markFinish;
                break;
            default:
                _mark.sprite = null;
                break;
        }

        if (invokeAction)
            Mark?.Invoke(markType, _x, _y);
    }

    public void MarkAsPath(Vector2 v2)
    {
        if (v2.x == _x && v2.y == _y)
            SetMark(MarkType.Path);
    }

    public void UpdateMark(Vector2 v2, MarkType markType, Node node)
    {
        if (v2.x == _x && v2.y == _y)
            _node = node;

        switch (markType)
        {
            case MarkType.None:
                break;
            case MarkType.Path:
                if (v2.x == _x && v2.y == _y)
                    SetMark(MarkType.Path, false);
                break;
            case MarkType.Start:
                if ((v2.x != _x || v2.y != _y) && _markType == MarkType.Start)
                    SetMark(MarkType.None, false);
                break;
            case MarkType.Finish:
                if ((v2.x != _x || v2.y != _y) && _markType == MarkType.Finish)
                    SetMark(MarkType.None, false);
                break;
            case MarkType.Open:
                if (v2.x == _x && v2.y == _y)
                    SetMark(MarkType.Open, false);
                break;
            case MarkType.Closed:
                if (v2.x == _x && v2.y == _y)
                    SetMark(MarkType.Closed, false);
                break;
            default:
                break;
        }

    }

    public void SetPosition(Vector2 position)
    {
        transform.localPosition = position;
    }

    void OnMouseDown()
    {
        switch (AStarMain.EditMode)
        {
            case EditMode.Debug:
                if (_node != null)
                    Debug.Log("node pos: x:" + _node.X + " y:" + _node.Y + " g:" + _node.GCost + " h: " + _node.HCost + " f:" + _node.FCost);
                else
                    Debug.Log("node is null at x:" + _x + " y:" + _y + "!");
                break;
            case EditMode.SetStart:
                SetMark(MarkType.Start);
                break;
            case EditMode.SetFinish:
                SetMark(MarkType.Finish);
                break;
            default:
                Set(_restrictionType == RestrictionType.None ? RestrictionType.Wall : RestrictionType.None, _x, _y, true);
                break;
        }

    }
}
