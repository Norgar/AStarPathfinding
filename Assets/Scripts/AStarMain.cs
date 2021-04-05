using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum RestrictionType
{
    None,
    Wall
}

public enum MarkType
{
    None,
    Path,
    Start,
    Finish,
    Open,
    Closed
}

public enum EditMode
{
    Debug,
    None,
    SetStart,
    SetFinish,
}

public class AStarMain : MonoBehaviour
{
    [SerializeField] private Texture2D _mazeMap;
    [SerializeField] private Transform _mazeRoot;
    [SerializeField] private Button _buttonStart;
    [SerializeField] private Button _buttonReset;
    [SerializeField] private Button _buttonPause;
    [SerializeField] private MazeElement _mazeElement;
    [SerializeField] private int HeuristicFactor;
    [SerializeField] [Range(0, 1)] private float _noizeSensitivity;
    [SerializeField] private float delay;
    [SerializeField] private EditMode _editMode;

    public MazeBuilder MazeBuilder;
    public MazeGenerator MazeGenerator;
    public AStarAlgorithm AStarAlgorithm;

    public static EditMode EditMode;

    private void Awake()
    {
        _buttonStart.onClick.AddListener(OnStartClick);
        _buttonReset.onClick.AddListener(OnResetClick);
        _buttonPause.onClick.AddListener(OnPauseClick);

        MazeBuilder = new MazeBuilder();
        MazeGenerator = new MazeGenerator();
        AStarAlgorithm = new AStarAlgorithm();

        MazeBuilder.MazeEdit += MazeGenerator.OnMazeEdit;

        MazeGenerator.Generate(_mazeMap, _noizeSensitivity);
        MazeBuilder.Build(MazeGenerator.GetMaze, _mazeRoot, _mazeElement);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ++_editMode;
            ++EditMode;

            if (_editMode > EditMode.SetFinish)
            {
                _editMode = 0;
                EditMode = 0;
            }

            Debug.Log("edit mode set to: " + _editMode);
        }
    }

    private void OnStartClick()
    {
        AStarAlgorithm.FindThePath(MazeGenerator.GetMaze, (int)MazeBuilder.Start.x, (int)MazeBuilder.Start.y, (int)MazeBuilder.Finish.x, (int)MazeBuilder.Finish.y, out MazeBuilder.Path, out MazeBuilder.Open, out MazeBuilder.Closed, HeuristicFactor);

        Debug.Log("open: " + MazeBuilder.Open.Count + " closed: " + MazeBuilder.Closed.Count + " path: " + MazeBuilder.Path.Count);

        if (delay > 0)
            coroutine = StartCoroutine(ShowPath());
        else
        {
            MazeBuilder.ShowOpen();
            MazeBuilder.ShowClosed();
            MazeBuilder.ShowPath();
        }
    }

    private void OnResetClick()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        pause = false;

        MazeBuilder.Reset(MazeGenerator.GetMaze, _mazeRoot);
    }


    Coroutine coroutine;
    bool pause = false;

    private void OnPauseClick()
    {
        pause = !pause;
    }

    private IEnumerator ShowPath()
    {
        var count = 0;

        foreach (var pass in MazeBuilder.Open)
            foreach (var item in pass.Value)
                ++count;

        Debug.Log("count: " + count);

        for (int i = 0; i < count; i++)
        {
            yield return new WaitWhile(() => pause);

            if (MazeBuilder.Open.ContainsKey(i))
            {
                yield return new WaitForSeconds(delay);

                foreach (var item in MazeBuilder.Open[i])
                    MazeBuilder.MarkPoint(new Vector2(item.X, item.Y), MarkType.Open, item);
            }

            if (MazeBuilder.Closed.ContainsKey(i))
            {
                yield return new WaitForSeconds(delay * 2);

                foreach (var item in MazeBuilder.Closed[i])
                {
                    MazeBuilder.MarkPoint(new Vector2(item.X, item.Y), MarkType.Closed, item);
                    //Debug.Log("close node pos: x:" + item.X + " y:" + item.Y + " g:" + item.GCost + " h: " + item.HCost + " f:" + item.FCost);
                }
            }
        }

        //foreach (var item in MazeBuilder.Open)
        //{
        //    yield return new WaitForSeconds(delay);

        //    MazeBuilder.MarkPoint(new Vector2(item.X, item.Y), MarkType.Open);
        //}

        //foreach (var item in MazeBuilder.Closed)
        //{
        //    yield return new WaitForSeconds(delay);

        //    MazeBuilder.MarkPoint(new Vector2(item.X, item.Y), MarkType.Closed);
        //}

        foreach (var item in MazeBuilder.Path)
        {
            yield return new WaitForSeconds(delay);

            MazeBuilder.MarkPoint(new Vector2(item.X, item.Y), MarkType.Path, item);
        }
    }
}
