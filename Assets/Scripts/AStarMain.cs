using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public enum RestrictionType
{
    None,
    Wall,
    Void
}

public enum MarkType
{
    None,
    Path,
    End,
    Start,
    Open,
    Closed,
    WayPoint,
    WayPointPort
}

public enum EditMode
{
    None,
    Edit,
    SetEnd,
    SetStart,
}

public class AStarMain : MonoBehaviour
{
    [SerializeField] private Canvas _uiCanvas;
    [SerializeField] private Canvas _mapCanvas;
    [SerializeField] private RawImage _rawImage;
    [SerializeField] private Texture2D _mazeMap;
    [SerializeField] private Button _buttonTest;
    [SerializeField] private Button _buttonStart;
    [SerializeField] private Button _buttonClear;
    [SerializeField] private Button _buttonReset;
    [SerializeField] private TextMeshProUGUI _editModeHint;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private int testCount;
    [SerializeField] private float delay;
    [SerializeField] [Range(0, 1)] private float _noizeSensitivity;

    public MazeGenerator MazeGenerator;
    public AStarAlgorithm AStarAlgorithm;
    public ResultDataCollector ResultDataCollector;

    public static EditMode EditMode;
    private int _x, _y;

    Coroutine coroutine;

    private void Awake()
    {
        _buttonTest.onClick.AddListener(OnTestClick);
        _buttonStart.onClick.AddListener(OnStartClick);
        _buttonClear.onClick.AddListener(OnClearClick);
        _buttonReset.onClick.AddListener(OnResetClick);

        ResultDataCollector = new ResultDataCollector();
        MazeGenerator = new MazeGenerator();
        AStarAlgorithm = new AStarAlgorithm();

        MazeGenerator.GenerateMaze(_mazeMap, _noizeSensitivity);
        _rawImage.texture = MazeGenerator.GenerateTexture();
        MazeGenerator.GenerateWayPoints();

        SetEditeModeHint(EditMode);
        SetResultHint(string.Empty);

        _uiCanvas.gameObject.SetActive(true);
        _mapCanvas.gameObject.SetActive(true);
    }

    private void Update()
    {
        SetEditMode();

        if (TryGetPixels(out _x, out _y))
            EditMaze(_x, _y);
    }

    private bool TryGetPixels(out int x, out int y)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(_rawImage.rectTransform, Input.mousePosition) && Input.GetMouseButtonDown(0))
        {
            var rect = _rawImage.rectTransform.rect.size * _mapCanvas.scaleFactor;
            var shift = new Vector3((Screen.width - rect.x) / 2, (Screen.height - rect.y) / 2);
            var pixel = (Input.mousePosition - shift) / _mapCanvas.scaleFactor;
            x = (int)pixel.x;
            y = (int)pixel.y;
            return true;
        }

        x = -1;
        y = -1;
        return false;
    }

    private void EditMaze(int x, int y)
    {
        switch (EditMode)
        {
            case EditMode.Edit:
                MazeGenerator.EditCell(x, y);
                break;
            case EditMode.SetStart:
                MazeGenerator.SetStart(x, y);
                break;
            case EditMode.SetEnd:
                MazeGenerator.SetEnd(x, y);
                break;
            default:
                SetResultHint("Nothing happens!\nYou've just clicked at x:" + x + " y:" + y);
                break;
        }
    }

    private void SetEditMode()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ++EditMode;

            if (EditMode > EditMode.SetStart)
                EditMode = 0;

            SetEditeModeHint(EditMode);
        }
    }

    private void OnTestClick()
    {
        for (int i = 0; i < testCount; i++)
        {
            try
            {
                OnClearClick();
                MazeGenerator.SetRandomEnd();
                MazeGenerator.SetRandomStart();
                AStarAlgorithm.FindThePath(MazeGenerator, ResultDataCollector);
                Debug.Log("test " + i + " has been passed!\t"
                    + "start: " + MazeGenerator.Start + " end: " + MazeGenerator.End);
            }
            catch (Exception e)
            {
                Debug.LogError("test " + i + " has been failed!\t"
                    + "start: " + MazeGenerator.Start + " end: " + MazeGenerator.End + " +\t" + e);
            }
        }
    }

    private void OnStartClick()
    {
        OnClearClick();

        AStarAlgorithm.FindThePath(MazeGenerator, ResultDataCollector);
        SetResultHint(ResultDataCollector.Result);

        if (delay > 0)
            coroutine = StartCoroutine(ShowResultBySteps());
        else
            ShowResultImmediate();
    }

    private void OnResetClick()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        MazeGenerator.GenerateMaze(_mazeMap, _noizeSensitivity);
        _rawImage.texture = MazeGenerator.GenerateTexture();
        MazeGenerator.GenerateWayPoints();
    }

    private void OnClearClick()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        var start = MazeGenerator.Start;
        var finish = MazeGenerator.End;
        _rawImage.texture = MazeGenerator.GenerateTexture();
        MazeGenerator.SetStart(start.x, start.y);
        MazeGenerator.SetEnd(finish.x, finish.y);
        MazeGenerator.GenerateWayPoints();
    }

    private void ShowResultImmediate()
    {
        foreach (var pass in ResultDataCollector.Open)
            foreach (var item in pass.Value)
                MazeGenerator.MarkCell(item.x, item.y, MarkType.Open);

        foreach (var item in ResultDataCollector.Closed)
            MazeGenerator.MarkCell(item.Value.x, item.Value.y, MarkType.Closed);

        foreach (var item in ResultDataCollector.Path)
            MazeGenerator.MarkCell(item.x, item.y, MarkType.Path);
    }

    private IEnumerator ShowResultBySteps()
    {
        for (int i = 0; i < ResultDataCollector.Passes; i++)
        {
            if (ResultDataCollector.Open.ContainsKey(i))
            {
                yield return new WaitForSeconds(delay);

                foreach (var item in ResultDataCollector.Open[i])
                    MazeGenerator.MarkCell(item.x, item.y, MarkType.Open);
            }

            if (ResultDataCollector.Closed.ContainsKey(i))
            {
                yield return new WaitForSeconds(delay);

                var item = ResultDataCollector.Closed[i];
                MazeGenerator.MarkCell(item.x, item.y, MarkType.Closed);
            }
        }

        foreach (var item in ResultDataCollector.Path)
        {
            yield return new WaitForSeconds(delay);

            MazeGenerator.MarkCell(item.x, item.y, MarkType.Path);
        }
    }

    private void SetEditeModeHint(EditMode editMode)
    {
        _editModeHint.text = "Current edit mode: " + editMode + "\n(press RMB to change)";
    }

    private void SetResultHint(string result)
    {
        _resultText.text = result;
    }
}
