using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
    None,
    Edit,
    SetStart,
    SetFinish,
}

public class AStarMain : MonoBehaviour
{
    [SerializeField] private Canvas _uiCanvas;
    [SerializeField] private Canvas _mapCanvas;
    [SerializeField] private RawImage _rawImage;
    [SerializeField] private Texture2D _mazeMap;
    [SerializeField] private Button _buttonStart;
    [SerializeField] private Button _buttonReset;
    [SerializeField] private TextMeshProUGUI _editModeHint;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private int _heuristicFactor;
    [SerializeField] [Range(0, 1)] private float _noizeSensitivity;
    [SerializeField] private float delay;

    public MazeBuilder MazeBuilder;
    public MazeGenerator MazeGenerator;
    public AStarAlgorithm AStarAlgorithm;

    public static EditMode EditMode;

    private Texture2D _texture2D;
    private int x, y;
    private int startX, startY;
    private int finishX, finishY;

    Coroutine coroutine;

    private Dictionary<MarkType, Color> markColors = new Dictionary<MarkType, Color>
    {
        { MarkType.Closed, Color.green },
        { MarkType.Start, Color.green },
        { MarkType.Open, Color.red },
        { MarkType.None, Color.white },
        { MarkType.Finish, Color.red },
        { MarkType.Path, Color.blue },
    };

    private void Awake()
    {
        _buttonStart.onClick.AddListener(OnStartClick);
        _buttonReset.onClick.AddListener(OnResetClick);

        MazeBuilder = new MazeBuilder();
        MazeGenerator = new MazeGenerator();
        AStarAlgorithm = new AStarAlgorithm();

        MazeGenerator.Generate(_mazeMap, _noizeSensitivity);
        SetTexture();

        SetEditeModeHint(EditMode);
        SetResult(string.Empty);

        _uiCanvas.gameObject.SetActive(true);
        _mapCanvas.gameObject.SetActive(true);
    }

    private void Update()
    {
        SetEditMode();

        if (TryGetPixels(out x, out y))
            EditMaze(x, y);
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
                var t = 1 - MazeGenerator.GetMaze[x, y];
                MazeGenerator.OnMazeEdit((RestrictionType)t, x, y);
                _texture2D.SetPixel(x, y, t == 1 ? Color.black : Color.white);
                break;
            case EditMode.SetStart:
                if (MazeGenerator.GetMaze[x, y] != 0)
                    break;
                _texture2D.SetPixel(startX, startY, markColors[MarkType.None]);
                _texture2D.SetPixel(x, y, markColors[MarkType.Start]);
                startX = x;
                startY = y;
                break;
            case EditMode.SetFinish:
                if (MazeGenerator.GetMaze[x, y] != 0)
                    break;
                _texture2D.SetPixel(finishX, finishY, markColors[MarkType.None]);
                _texture2D.SetPixel(x, y, markColors[MarkType.Finish]);
                finishX = x;
                finishY = y;
                break;
            default:
                break;
        }

        _texture2D.Apply();
    }

    private void SetEditMode()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ++EditMode;

            if (EditMode > EditMode.SetFinish)
                EditMode = 0;

            SetEditeModeHint(EditMode);
        }
    }

    private void OnStartClick()
    {
        string result;
        AStarAlgorithm.FindThePath(MazeGenerator.GetMaze, startX, startY, finishX, finishY, out MazeBuilder.Path, out MazeBuilder.Open, out MazeBuilder.Closed, out result, _heuristicFactor);
        SetResult(result);

        if (delay > 0)
            coroutine = StartCoroutine(ShowPath());
        else
            ShowResult(MazeBuilder.Path, MazeBuilder.Open, MazeBuilder.Closed);
    }

    private void OnResetClick()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        MazeGenerator.Generate(_mazeMap, _noizeSensitivity);
        SetTexture();
    }

    private void SetTexture()
    {
        _texture2D = new Texture2D(_mazeMap.width, _mazeMap.height);

        var index = 0;
        var length = MazeGenerator.GetMaze.GetLength(0);

        startX = startY = 0;
        finishX = finishY = length - 1;

        Color32[] color = new Color32[length * length];

        for (int y = 0; y < length; y++)
            for (int x = 0; x < length; x++)
                color[index++] = MazeGenerator.GetMaze[x, y] == 1 ? Color.black : Color.white;


        _texture2D.filterMode = FilterMode.Point;
        _texture2D.SetPixels32(color);

        _texture2D.SetPixel(startX, startY, markColors[MarkType.Start]);
        _texture2D.SetPixel(finishX, finishY, markColors[MarkType.Finish]);

        _texture2D.Apply();

        _rawImage.texture = _texture2D;
    }

    private void SetEditeModeHint(EditMode editMode)
    {
        _editModeHint.text = "Current edit mode: " + editMode + "\n(press RMB to change)";
    }

    private void SetResult(string result)
    {
        _resultText.text = result;
    }

    private void ShowResult(List<Node> path, Dictionary<int, List<Node>> open, Dictionary<int, List<Node>> closed)
    {
        foreach (var pass in open)
            foreach (var item in pass.Value)
                _texture2D.SetPixel(item.X, item.Y, markColors[MarkType.Open]);

        foreach (var pass in closed)
            foreach (var item in pass.Value)
                _texture2D.SetPixel(item.X, item.Y, markColors[MarkType.Closed]);

        foreach (var item in path)
            _texture2D.SetPixel(item.X, item.Y, markColors[MarkType.Path]);

        _texture2D.Apply();
    }

    private IEnumerator ShowPath()
    {
        var count = 0;

        foreach (var pass in MazeBuilder.Open)
            foreach (var item in pass.Value)
                ++count;

        for (int i = 0; i < count; i++)
        {
            if (MazeBuilder.Open.ContainsKey(i))
            {
                yield return new WaitForSeconds(delay);

                foreach (var item in MazeBuilder.Open[i])
                {
                    _texture2D.SetPixel(item.X, item.Y, markColors[MarkType.Open]);
                    _texture2D.Apply();
                }
            }

            if (MazeBuilder.Closed.ContainsKey(i))
            {
                yield return new WaitForSeconds(delay);

                foreach (var item in MazeBuilder.Closed[i])
                {
                    _texture2D.SetPixel(item.X, item.Y, markColors[MarkType.Closed]);
                    _texture2D.Apply();
                }
            }
        }

        foreach (var item in MazeBuilder.Path)
        {
            yield return new WaitForSeconds(delay);

            _texture2D.SetPixel(item.X, item.Y, markColors[MarkType.Path]);
            _texture2D.Apply();
        }
    }
}
