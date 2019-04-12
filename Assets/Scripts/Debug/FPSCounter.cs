using UnityEngine;
using System.Collections;

//**********************************************//
//                                              //
//**********************************************//

[AddComponentMenu("Utilities/FPSCounter")]
public class FPSCounter : MonoBehaviour
{

    [Header("Display")]

    [SerializeField]
    private int _fontSize = 18;

    [SerializeField]
    private Rect _rect = new Rect(10.0f, 10.0f, 100.0f, 300.0f);

    [Header("Settings")]

    [SerializeField]
    //Update the background color depend on the FPS
    private bool _isColorUpdated = true;

    [SerializeField]
    //Allow the dragging of the FPS window
    private bool _isDragAllowed = true;

    [SerializeField]
    //The update frequency
    private float _updateFrequency = 0.5f;

    [SerializeField]
    //How many decimal to display
    private int _numDecimal = 2;

    //**********************************************//
    //                                              //
    //**********************************************//

    private string _fpsText = "";
    private float _accum = 0.0f;
    private int _frames = 0;

    private Color _backColor = Color.white;
    private GUIStyle _style;

    //**********************************************//
    //                                              //
    //**********************************************//

    //------------------------------------------------
    private void Start()
    {
        StartCoroutine(FPS());
    }

    //------------------------------------------------
    private void Update()
    {
        _accum += Time.timeScale / Time.deltaTime;
        ++_frames;
    }

    //**********************************************//
    //                                              //
    //**********************************************//

    //------------------------------------------------
    private void OnGUI()
    {
        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label);
            _style.normal.textColor = Color.white;
            _style.alignment = TextAnchor.MiddleCenter;
            _style.fontSize = _fontSize;
        }

        GUI.color = _isColorUpdated ? _backColor : Color.white;
        _rect = GUI.Window(0, _rect, DoMyWindow, "");
    }

    //------------------------------------------------
    private void DoMyWindow(int windowID)
    {
        GUI.Label(new Rect(0.0f, 0.0f, _rect.width, _rect.height), _fpsText + " fps", _style);
        if (_isDragAllowed)
        {
            GUI.DragWindow(new Rect(0.0f, 0.0f, Screen.width, Screen.height));
        }
    }

    //**********************************************//
    //                                              //
    //**********************************************//

    //------------------------------------------------
    IEnumerator FPS()
    {
        while (true)
        {
            float fps = _accum / _frames;
            _fpsText = fps.ToString("f" + Mathf.Clamp(_numDecimal, 0, 10));
            _accum = 0.0f;
            _frames = 0;

            _backColor = (fps >= 40.0f) ? Color.green : ((fps > 10.0f) ? Color.red : Color.yellow);

            yield return new WaitForSeconds(_updateFrequency);
        }
    }
}