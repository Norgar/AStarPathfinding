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
    private int _x, _y;

    Coroutine coroutine;

    private void Awake()
    {
        _buttonStart.onClick.AddListener(OnStartClick);
        _buttonReset.onClick.AddListener(OnResetClick);

        MazeBuilder = new MazeBuilder();
        MazeGenerator = new MazeGenerator();
        AStarAlgorithm = new AStarAlgorithm();

        MazeGenerator.GenerateMaze(_mazeMap, _noizeSensitivity);
        _rawImage.texture = MazeGenerator.GenerateTexture();

        SetEditeModeHint(EditMode);
        SetResult(string.Empty);

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
            case EditMode.SetFinish:
                MazeGenerator.SetFinish(x, y);
                break;
            default:
                SetResult("Nothing happens!\nYou've just clicked at x:" + x + " y:" + y);
                break;
        }
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
        AStarAlgorithm.FindThePath(MazeGenerator, out MazeBuilder.Path, out MazeBuilder.Open, out MazeBuilder.Closed, out result, _heuristicFactor);
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

        MazeGenerator.GenerateMaze(_mazeMap, _noizeSensitivity);
        _rawImage.texture = MazeGenerator.GenerateTexture();
    }

    private void ShowResult(List<Node> path, Dictionary<int, List<Node>> open, Dictionary<int, List<Node>> closed)
    {
        foreach (var pass in open)
            foreach (var item in pass.Value)
                MazeGenerator.MarkCell(item.X, item.Y, MarkType.Open);

        foreach (var pass in closed)
            foreach (var item in pass.Value)
                MazeGenerator.MarkCell(item.X, item.Y, MarkType.Closed);

        foreach (var item in path)
            MazeGenerator.MarkCell(item.X, item.Y, MarkType.Path);
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
                    MazeGenerator.MarkCell(item.X, item.Y, MarkType.Open);
            }

            if (MazeBuilder.Closed.ContainsKey(i))
            {
                yield return new WaitForSeconds(delay);

                foreach (var item in MazeBuilder.Closed[i])
                    MazeGenerator.MarkCell(item.X, item.Y, MarkType.Closed);
            }
        }

        foreach (var item in MazeBuilder.Path)
        {
            yield return new WaitForSeconds(delay);

            MazeGenerator.MarkCell(item.X, item.Y, MarkType.Path);
        }
    }

    private void SetEditeModeHint(EditMode editMode) => _editModeHint.text = "Current edit mode: " + editMode + "\n(press RMB to change)";

    private void SetResult(string result) => _resultText.text = result;
}
