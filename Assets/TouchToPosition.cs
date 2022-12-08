using OscJack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchToPosition : MonoBehaviour
{
    [SerializeField] OscConnection _connection = null;
    [SerializeField] string _oscAddress = "/unity";
    [SerializeField] Rect _rect;
    [SerializeField] Material _mat;
    private Rect _rectPixels;
    private int _touchCount=0;
    private Vector2[] _pos = new Vector2[11];
    private string _version = "0.1";
    private float _sat;
    private float _hue;

    OscClient _client;
    // Start is called before the first frame update
    void Awake()
    {
        if (_connection != null)
            _client = OscMaster.GetSharedClient(_connection.host, _connection.port);
        else
            _client = null;       
        
        InitSize();
    }

    void InitSize()
    {
        _rectPixels = new Rect(_rect);
        _rectPixels.width = _rect.width * Screen.width;
        _rectPixels.height = _rect.height * Screen.height;
    }

    public void SetHost(string host)
    {
        _client.Dispose();
        _connection.host = host;
        _client = OscMaster.GetSharedClient(_connection.host, _connection.port);
    }

    public void SetPort(string portString)
    {
        int port;
        if (int.TryParse(portString, out port) == false)
            SetPort(port);
    }

    public void SetPort(int port)
    {
        _client.Dispose();
        _connection.port = port;
        _client = OscMaster.GetSharedClient(_connection.host, _connection.port);
    }

    public void SendAmplitude(float v)
    {
        _client.Send($"{_oscAddress}/amplitude", v);
    }

    public void SendSaturation(float v)
    {
        _sat = v;
        _mat.color = Color.HSVToRGB(_hue, _sat, 1);
        _client.Send($"{_oscAddress}/saturation", v);
    }

    public void SendHue(float v)
    {
        _hue = v;
        _mat.color = Color.HSVToRGB(_hue, _sat,1);
        _client.Send($"{_oscAddress}/hue", v);
    }

    void OnGUI()
    {
        // Compute a fontSize based on the size of the screen width.
        GUI.skin.label.fontSize = (int)(Screen.width / 50.0f);
        GUI.Label(new Rect(70, 10, 1200, 70), $"V: {_version}"); 
        //GUI.Label(new Rect(60, 20, 1200, 60 ), _rectPixels.ToString());
        for (int i=0; i < _touchCount; i++)
        {
            GUI.Label(new Rect(70, 30 + 70 * i, 1200, 70), $"p{i}: " + _pos[i].ToString());
        }
    }

    protected bool SendPos(Vector2 p, int i)
    {
        if (_rectPixels.Contains(p))
        {
            Vector2 pCentered = (p - _rectPixels.center) / _rectPixels.size;
            _client.Send($"{_oscAddress}/p{i}_", pCentered.x, pCentered.y);
            _pos[i] = pCentered;
            return true;
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_client == null) return;
        //Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height-Input.mousePosition.y);SendPos(mousePos, 0);
        _touchCount = 0;
        for (int i=0;i<Input.touchCount;i++)
        {
            Touch touch = Input.GetTouch(i);            
            Vector2 pos = touch.position;
            if(SendPos(pos,i))
                _touchCount++; ;            
        }
        _client.Send($"{_oscAddress}/count", _touchCount);
    }
}
